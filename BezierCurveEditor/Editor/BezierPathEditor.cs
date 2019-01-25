using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierPath))]
public class BezierPathEditor : Editor
{
    BezierPath path;
    int selectedIndex = -1;

    private void Awake()
    {
        path = target as BezierPath;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Create new"))
        {
            if (EditorUtility.DisplayDialog("Create new path","This will delete the current path, are you sure?","yes","cancel"))
            {
                path.TableOutOfDate();
                Undo.RecordObject(path, "New path");
                path.NewPath();
                SceneView.RepaintAll();
                GUIUtility.ExitGUI();
            }
        }
        if (GUILayout.Button("Add point"))
        {
            path.TableOutOfDate();
            Undo.RecordObject(path, "Add point");
            path.AddPoint();
            SceneView.RepaintAll();
        }

        float tempT = GUILayout.HorizontalSlider(path.insertT,0f,1f);
        if (path.insertT != tempT) {
            path.insertT = tempT;
            SceneView.RepaintAll();
        }

        if(GUILayout.Button("Insert point"))
        {
            path.TableOutOfDate();
            Undo.RecordObject(path, "Insert point");
            path.InsertPoint(path.insertT);
            SceneView.RepaintAll();
        }

        if (GUILayout.Button("Toggle closed"))
        {
            path.TableOutOfDate();
            Undo.RecordObject(path, "Toggle closed");
            path.ToggleClosed();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Reset rotations"))
        {
            path.TableOutOfDate();
            Undo.RecordObject(path, "Reset rotations");
            path.ResetRotations();
            SceneView.RepaintAll();
        }
    }

    private void OnSceneGUI()
    {
        //Manipulating the points on the curve
        for (int n = 0; n <= path.points.Count - 1; n++)
        {
            if (n % 3 == 0) {
                Handles.color = Color.yellow;
            }
            else
            {
                Handles.color = Color.white;
            }
            float size = HandleUtility.GetHandleSize(path.points[n]);
            if (n == selectedIndex)
            {
                Vector3 newPos = Handles.PositionHandle(path[n], Quaternion.identity);
                if (path[n] != newPos)
                {
                    Undo.RecordObject(path, "Move point");
                    path.MovePoint(n, newPos-path.transform.position);
                }

                if (n % 3 == 0)
                {
                    Quaternion newRotation = Handles.Disc(Quaternion.identity, path[n], path.PointTangent(n), size*0.5f, false, 1f);
                    //Debug.Log(newRotation.eulerAngles);
                }

            } else if (Handles.Button(path[n], Quaternion.identity, 0.1f*size, 0.15f*size, Handles.DotHandleCap)){
                selectedIndex = n;
            }
        }

        //Draw the bezier
        for (int n = 0; n < path.SegmentCount(); n++) {
            Vector3[] segment = path.Segment(n);
            Handles.DrawBezier(segment[0], segment[3], segment[1], segment[2], Color.red, null, 3);
        }

        //Draw insertion Point
        Vector3 insertionPoint = path.PathPoint(path.insertT);
        float insertionHandleSize = HandleUtility.GetHandleSize(insertionPoint);
        Handles.color = Color.blue;
        if (Handles.Button(insertionPoint, Quaternion.identity, 0.1f*insertionHandleSize, 0.15f*insertionHandleSize, Handles.DotHandleCap))
        {
            Debug.Log("Use 'insert point' to insert a new point here");
        }
    }
}