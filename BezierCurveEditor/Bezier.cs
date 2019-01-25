using UnityEngine;

public static class Bezier {

    public static Vector3 CubicPoint(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t){
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return
            oneMinusT * oneMinusT * oneMinusT * a +
            3f * oneMinusT * oneMinusT * t * b +
            3f * oneMinusT * t * t * c +
            t * t * t * d;
    }

    public static Vector3 CubicTangent(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return
            3f * oneMinusT * oneMinusT * (b - a) +
            6f * oneMinusT * t * (c - b) +
            3f * t * t * (d - c);
    }

    public static Vector3 CubicNormal(Vector3 tangent, Vector3 up) {
        Vector3 binormal = Vector3.Cross(up, tangent).normalized;
        return Vector3.Cross(tangent, binormal);
    }

    public static Quaternion CubicOrientation(Vector3 tangent, Vector3 up) {
        Vector3 normal = CubicNormal(tangent, up);
        return Quaternion.LookRotation(tangent, normal);
    }

    public static OrientedPoint CubicOrientedPoint(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t, Vector3 roll)
    {
        Vector3 position = CubicPoint(a, b, c, d, t);
        Quaternion rotation = CubicOrientation(CubicTangent(a, b, c, d, t), roll);

        return new OrientedPoint(position, rotation);
    }
}
