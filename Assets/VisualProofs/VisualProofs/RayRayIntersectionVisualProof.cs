using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Test.VisualProofs
{
    [Serializable]
    public class RayRayIntersectionTestExample
    {
        public Vector2 Origin;
        public Vector2 Direction;
        public Vector2 Origin1;
        public Vector2 Direction1;

        public Vector3 GetMin()
        {
            return Vector3.Min(
                Vector3.Min(
                    Vector3.Min(
                        Origin,
                        Origin + Direction),
                    Origin1),
                Origin1 + Direction1);
        }

        public Vector3 GetMax()
        {
            return Vector3.Max(
                Vector3.Max(
                    Vector3.Max(
                        Origin,
                        Origin + Direction),
                    Origin1),
                Origin1 + Direction1);
        }

        public Vector3 Size()
        {
            float3 size = GetMax() - GetMin();
            return Vector2.one * Mathf.Max(size.x, size.y);
        }
    }

    public class RayRayIntersectionVisualProof : MonoBehaviour
    {
        public List<RayRayIntersectionTestExample> IntersectionTests = new List<RayRayIntersectionTestExample>();
        public int HztalSeparation;
        public int VtcalSeparation;
        public int rowSize;
        public int columnSize;

        private void OnDrawGizmos()
        {
            for (int i = 0; i < rowSize; i++)
            {
                float x = Mathf.Sin(i * Mathf.PI / (rowSize / 2.0f));
                float y = Mathf.Cos(i * Mathf.PI / (rowSize / 2.0f));
                Vector2 d = new Vector2(x, y);
                for (int j = 0; j < columnSize; j++)
                {
                    Vector2 p = new Vector2(i * HztalSeparation, j * VtcalSeparation);
                    Vector2 p1 = new Vector2(0.5f + i * HztalSeparation, j * VtcalSeparation);
                    x = Mathf.Sin(j * Mathf.PI / (columnSize / 2.0f));
                    y = Mathf.Cos(j * Mathf.PI / (columnSize / 2.0f));
                    DrawIntersection((Vector2) this.transform.position + p, d, (Vector2) this.transform.position + p1, new Vector2(x, y));
                }
            }
        }

        private void DrawIntersection(Vector2 origin, Vector2 direction, Vector2 origin1, Vector2 direction1)
        {
            LemmingsShaderMathUtil util = new LemmingsShaderMathUtil();
            float t = util.Ray2Ray(origin, direction, origin1, direction1);

            if (t == float.NegativeInfinity || t == float.PositiveInfinity || float.IsNaN(t) || t == float.MaxValue || t == float.MinValue)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(origin, origin + direction);
                Gizmos.DrawLine(origin1, origin1 + direction1);
            }
            else
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(origin, origin + direction);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(origin1, origin1 + direction1);
                Gizmos.color = Color.green;

                Handles.Label((Vector3) (origin + origin1) / 2.0f, $"Meet at t:{t} p-> {origin + direction * t}");
            }

            Gizmos.DrawSphere(origin + direction * t, 0.1f);
        }
    }
}