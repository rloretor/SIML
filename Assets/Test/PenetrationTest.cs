using System;
using System.Net.Mail;
using Test;
using Unity.Mathematics;
using UnityEditor;
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
        // DrawIntersection();
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
}