using System;
using System.Net.Mail;
using UnityEngine;

[ExecuteAlways]
public class PenetrationTest : MonoBehaviour
{
    public Transform fixedPos;
    public Transform pixelPos;
    public Transform penPos;
    public Transform vel;

    Vector2 project(Vector2 b, Vector2 a)
    {
        return (Vector2.Dot(a, b) / Vector2.Dot(a, a)) * a;
    }

    private void OnDrawGizmos()
    {
        // DrawProjection();
        DrawIntersection();
    }

    private void DrawProjection()
    {
        Vector2 displ = project(penPos.position - pixelPos.position, fixedPos.position - pixelPos.position);
        Vector2 dir = project(vel.position - penPos.position, fixedPos.position - pixelPos.position);

        Gizmos.color = Color.grey;
        Gizmos.DrawCube(pixelPos.position, Vector3.one);
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(pixelPos.position, fixedPos.position);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(pixelPos.position, penPos.position);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(penPos.position, penPos.position + (Vector3) dir);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(penPos.position, penPos.position + (Vector3) displ);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(penPos.position, vel.position);
    }

    private void DrawIntersection()
    {
        Vector2 p0 = fixedPos.position;
        Vector2 d0 = ((Vector2) penPos.position - p0).normalized;
        Vector2 p1 = pixelPos.position;
        Vector2 s = ((Vector2) vel.position - p1) / 2.0f;
        s.x = Mathf.Abs(s.x);
        s.y = Mathf.Abs(s.y);
        Gizmos.DrawCube(p1, s * 2);

        Vector2 X = new Vector2();
        Vector2 Y = new Vector2();
        X[0] = RayVsRay(p0, d0, p1 - s, Vector2.right);
        Y[0] = RayVsRay(p0, d0, p1 - s, Vector2.up);
        X[1] = RayVsRay(p0, d0, p1 + s, Vector2.right);
        Y[1] = RayVsRay(p0, d0, p1 + s, Vector2.up);

        Gizmos.DrawLine(p0, penPos.position);

        Gizmos.DrawLine(p1, vel.position);

        float tmin = Mathf.Max(Mathf.Min(X[0], X[1]), Mathf.Min(Y[0], Y[1]));
        float tmax = Mathf.Min(Mathf.Max(X[0], X[1]), Mathf.Max(Y[0], Y[1]));

        if (tmax < 0 || tmin > tmax)
        {
            Gizmos.color = Color.black;
        }
        else
        {
            Vector2 p = d0 * (tmin < 0f ? tmax : tmin) + p0;
            Gizmos.DrawWireSphere(p, 20.0f);
            Vector2 n = (p - p1);
            Vector2 a = new Vector2(Mathf.Abs(n.x), Mathf.Abs(n.y));
            if (a.x > a.y)
            {
                n = Vector2.right * Mathf.Sign(n.x);
            }
            else
            {
                n = Vector2.up * Mathf.Sign(n.y);
            }

            Gizmos.DrawLine(p, p + n * 1000);
        }
    }

    float norm(float v, float min, float max)
    {
        return (v - min) / (max - min);
    }

    float RayVsRay(Vector2 a, Vector2 ad, Vector2 b, Vector2 bd)
    {
        float dx = b.x - a.x;
        float dy = b.y - a.y;
        float u = (dy * bd.x - dx * bd.y) / ((bd.x * ad.y) - (bd.y * ad.x));

        return u;
    }
}