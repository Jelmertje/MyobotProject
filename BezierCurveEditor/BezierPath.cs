using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierPath : MonoBehaviour
{
    [HideInInspector]
    public List<Vector3> points;
    [HideInInspector]
    public List<Vector3> rolls;//TODO orient path roll
    [HideInInspector]
    public bool closed = false;

    private int resolution = 100;
    public int Resolution
    {
        get
        {
            return resolution;
        }
        set
        {
            resolution = value;
            isTableUpToDate = false;
        }
    }

    private float[] lookupTable;
    private bool isTableUpToDate = false;

    [HideInInspector]
    public float insertT;

    private void Start()
    {

    }

    //Information
    public Vector3 this[int i]
    {
        get { return transform.TransformPoint(points[i]); }
    }

    public int SegmentCount()
    {
        return points.Count / 3;
    }

    private Vector3[] LocalSegment(int i)
    {
        Vector3[] segment = new Vector3[4];
        segment[0] = points[i * 3];
        segment[1] = points[i * 3 + 1];
        segment[2] = points[i * 3 + 2];

        segment[3] = i != SegmentCount() - 1 ? points[i * 3 + 3] : !closed ? points[points.Count - 1] : points[0];
        return segment;
    }
    public Vector3[] Segment(int i)
    {
        Vector3[] segment = LocalSegment(i);

        for (int n = 0; n < segment.Length; n++)
        {
            segment[n] = transform.TransformPoint(segment[n]);
        }

        return segment;
    }

    private Vector3 LocalPathPoint(float t)
    {
        int requestedSegment;
        if (t >= 1f)
        {
            t = 1f;
            requestedSegment = SegmentCount() - 1;
        }
        else
        {
            t = Mathf.Clamp01(t) * SegmentCount();
            requestedSegment = (int)t;
            t -= requestedSegment;
        }

        Vector3[] segmentPoints = LocalSegment(requestedSegment);

        return Bezier.CubicPoint(segmentPoints[0], segmentPoints[1], segmentPoints[2], segmentPoints[3], t);
    }

    public Vector3 PathPoint(float t)
    {
        return transform.TransformPoint(LocalPathPoint(t));
    }

    public Quaternion PathDirection(float t)
    {
        int requestedSegment;
        if (t >= 1f)
        {
            t = 1f;
            requestedSegment = SegmentCount() - 1;
        }
        else
        {
            t = Mathf.Clamp01(t) * SegmentCount();
            requestedSegment = (int)t;
            t -= requestedSegment;
        }

        Vector3[] segmentPoints = LocalSegment(requestedSegment);
        Vector3 tangent = Bezier.CubicTangent(segmentPoints[0], segmentPoints[1], segmentPoints[2], segmentPoints[3], t);
        return Bezier.CubicOrientation(tangent, Vector3.up);
    }

    private OrientedPoint LocalPathOrientedPoint(float t)
    {
        int requestedSegment;
        if (t >= 1f)
        {
            t = 1f;
            requestedSegment = SegmentCount() - 1;
        }
        else
        {
            t = Mathf.Clamp01(t) * SegmentCount();
            requestedSegment = (int)t;
            t -= requestedSegment;
        }

        Vector3[] segmentPoints = LocalSegment(requestedSegment);
        return Bezier.CubicOrientedPoint(segmentPoints[0], segmentPoints[1], segmentPoints[2], segmentPoints[3], t, Vector3.up);
    }
    public OrientedPoint PathOrientedPoint(float t)
    {
        OrientedPoint op = LocalPathOrientedPoint(t);
        op.position = transform.TransformPoint(op.position);
        return op;
    }

    public Vector3 PathTangent(float t)
    {
        int requestedSegment;
        if (t >= 1f)
        {
            t = 1f;
            requestedSegment = SegmentCount() - 1;
        }
        else
        {
            t = Mathf.Clamp01(t) * SegmentCount();
            requestedSegment = (int)t;
            t -= requestedSegment;
        }
        Vector3[] segmentPoints = LocalSegment(requestedSegment);
        return Bezier.CubicTangent(segmentPoints[0], segmentPoints[1], segmentPoints[2], segmentPoints[3], t);
    }

    public Vector3 PointTangent(int i){
        if(i % 3 != 0){
            Debug.LogWarning("Point is not an anchorpoint");
            return Vector3.up;
        }else {
            Vector3[] segmentPoints = LocalSegment(i / 3);
            return Bezier.CubicTangent(segmentPoints[0], segmentPoints[1], segmentPoints[2], segmentPoints[3], 0f);
        }
    }

    /// <summary>
    /// Calculates the lookup table.
    /// </summary>
    private void CalculateLookupTable()
    {
        if (!isTableUpToDate)
        {
            lookupTable = new float[resolution];
            lookupTable[0] = 0f;
            Vector3 previous = LocalPathPoint(0f);

            for (int n = 1; n < resolution; n++)
            {
                Vector3 pointAtT = LocalPathPoint((float)n / (resolution - 1));
                lookupTable[n] = lookupTable[n - 1] + (previous - pointAtT).magnitude;

                previous = pointAtT;
            }
            isTableUpToDate = true;
        }
    }

    /// <summary>
    /// Mapped t value.
    /// </summary>
    /// <returns>The mapped t value, the value for wich t represents the distance traveled along the path.</returns>
    /// <param name="t">t</param>
    public float MappedTvalue(float t)
    {
        CalculateLookupTable();

        float t2 = -1f;
        float expectedDistance = t * lookupTable[resolution - 1];

        for (int n2 = 1; n2 < resolution; n2++)
        {
            if (expectedDistance <= lookupTable[n2])
            {
                float tLower = ((float)n2 - 1f) / (float)(resolution - 1);
                float tUpper = (float)n2 / (float)(resolution - 1);
                float vLower = lookupTable[n2 - 1];
                float vUpper = lookupTable[n2];
                t2 = Mathf.Lerp(tUpper, tLower, (vUpper - expectedDistance) / (vUpper - vLower));
                return t2;
            }
        }
        return t2;
    }

    /// <summary>
    /// Mappes the T values.
    /// </summary>
    /// <returns>An array of mapped T values</returns>
    /// <param name="length">Length of the desired array</param>
    public float[] MappedTvalues(int length)
    {
        float[] mapped = new float[length];
        for (int n = 0; n < length; n++)
        {
            mapped[n] = MappedTvalue((float)n / (length - 1));
        }

        return mapped;
    }

    /// <summary>
    /// Aproximate length of this path
    /// </summary>
    /// <returns>The length of this path</returns>
    public float PathLength()
    {
        float length = 0f;

        //We only use the anchor points in this array
        for (int n = 3; n < points.Count; n += 3)
        {
            length += (points[n] - points[n - 3]).magnitude;
        }
        if (closed)
        {
            length += (points[points.Count - 1] - points[0]).magnitude;
        }

        return length;
    }

    //Manipulation
    public void NewPath()
    {
        points.Clear();
        points.Add(transform.position);
        points.Add(transform.position + Vector3.forward);
        points.Add(transform.position + Vector3.forward + Vector3.left);
        points.Add(transform.position + Vector3.forward * 2f + Vector3.left);
        closed = false;

        rolls.Add(Vector3.up);
        rolls.Add(Vector3.up);

        isTableUpToDate = false;
    }

    public void AddPoint()
    {
        points.Add(points[points.Count - 1] + Vector3.left * 2f);
        points.Add(points[points.Count - 1] + Vector3.forward + Vector3.left * 2f);
        points.Add(points[points.Count - 1] + Vector3.forward * 2f + Vector3.left * 2f);

        rolls.Add(Vector3.up);

        isTableUpToDate = false;
    }

    public void InsertPoint(float t)
    {
        Vector3 insertPoint = LocalPathPoint(t);
        int requestedSegment;

        if (t >= 1f)
        {
            t = 1f;
            requestedSegment = SegmentCount() - 1;
        }
        else
        {
            t = Mathf.Clamp01(t) * SegmentCount();
            requestedSegment = (int)t;
            t -= requestedSegment;
        }
        requestedSegment *= 3;

        points.Insert(requestedSegment + 2, insertPoint);
        points.Insert(requestedSegment + 2, insertPoint);
        points.Insert(requestedSegment + 2, insertPoint);

    }

    public void ToggleClosed()
    {
        closed = !closed;
        if (closed)
        {
            points.Add(points[points.Count - 1] + Vector3.forward + Vector3.left);
            points.Add(points[0] - Vector3.forward * 2f);
        }
        else
        {
            points.RemoveRange(points.Count - 2, 2);
        }
        isTableUpToDate = false;
    }

    public void MovePoint(int i, Vector3 newPos)
    {
        //if point i is an anchor point
        if (i % 3 == 0)
        {
            if (i < points.Count - 1)
            {
                points[i + 1] += newPos - points[i];
            }
            if (i > 0)
            {
                points[i - 1] += newPos - points[i];
            }
            if (closed && i == 0)
            {
                points[points.Count - 1] += newPos - points[i];
            }
        }
        else //if point i is a control point
        {
            bool nextIsAnchor = (i + 1) % 3 == 0 ? true : false;

            if (nextIsAnchor)
            {
                if (i < points.Count - 2)
                {
                    points[i + 2] = points[i + 1] - (newPos - points[i + 1]);
                }
                else if (closed)
                {
                    points[1] = points[0] - (newPos - points[0]);
                }
            }
            else
            {
                if (i > 1)
                {
                    points[i - 2] = points[i - 1] - (newPos - points[i - 1]);
                }
                else if (closed)
                {
                    points[points.Count - 1] = points[i - 1] - (newPos - points[i - 1]);
                }
            }
        }

        points[i] = newPos;
        isTableUpToDate = false;
    }

    public void TableOutOfDate()
    {
        isTableUpToDate = false;
    }

    public void ResetRotations()
    {
        rolls.Clear();
        for (int n = closed ? 0 : 1; n < SegmentCount(); n++)
        {
            rolls.Add(Vector3.up);
        }
    }
}
