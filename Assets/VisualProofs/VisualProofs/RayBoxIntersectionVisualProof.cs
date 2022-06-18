using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Test.VisualProofs
{
    [Serializable]
    public class RayIBoxIntersectionTestExample
    {
        public Vector2 Center;
        public Vector2 rectSize;
        public Vector2 Origin;
        public Vector2 Direction;

        public Vector3 GetMin()
        {
            return Vector3.Min(
                Vector3.Min(
                    Vector3.Min(
                        Origin,
                        Origin + Direction),
                    Center - rectSize / 2.0f),
                Center + rectSize / 2.0f);
        }

        public Vector3 GetMax()
        {
            return Vector3.Max(
                Vector3.Max(
                    Vector3.Max(
                        Origin,
                        Origin + Direction),
                    Center - rectSize / 2.0f),
                Center + rectSize / 2.0f);
        }

        public Vector3 Size()
        {
            float3 size = GetMax() - GetMin();
            return Vector2.one * Mathf.Max(size.x, size.y);
        }
    }

    public class RayBoxIntersectionVisualProof : MonoBehaviour
    {
        public List<RayIBoxIntersectionTestExample> IntersectionTests = new List<RayIBoxIntersectionTestExample>();
        public int testsPerLine = 5;
        public int separationPerLine = 5;

        private void OnDrawGizmos()
        {
            RayIBoxIntersectionTestExample rayRayIntersection = IntersectionTests[0];
            Vector3 offset = transform.position;
            DrawIntersection(rayRayIntersection.Origin, rayRayIntersection.Direction, rayRayIntersection.Center, rayRayIntersection.rectSize, offset);

            int index;
            for (index = 1; index < IntersectionTests.Count; index++)
            {
                rayRayIntersection = IntersectionTests[index];
                if (index % testsPerLine == 0)
                {
                    offset.y = transform.position.y - separationPerLine * index / testsPerLine;
                    offset.x = transform.position.x;
                }
                else
                {
                    offset.x += (IntersectionTests[index].rectSize * 2).x;
                }


                DrawIntersection(rayRayIntersection.Origin, rayRayIntersection.Direction, rayRayIntersection.Center, rayRayIntersection.rectSize, offset);
            }

            index = (int) Mathf.Ceil((float) index / testsPerLine) * testsPerLine;

            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 16; i++)
                {
                    float x = Mathf.Sin(i * Mathf.PI / 8);
                    float y = Mathf.Cos(i * Mathf.PI / 8);
                    Vector2 size = Vector2.one * 2.0f;
                    RayIBoxIntersectionTestExample example = new RayIBoxIntersectionTestExample()
                    {
                        Origin = size / 2 + new Vector2(x, y) * (j),
                        Direction = new Vector2(x, y),
                        Center = Vector2.zero,
                        rectSize = size
                    };


                    if (index % testsPerLine == 0)
                    {
                        offset.y = transform.position.y - separationPerLine * index / testsPerLine;
                        offset.x = transform.position.x;
                    }
                    else
                    {
                        offset.x += (size * 2).x;
                    }

                    index += 1;
                    DrawIntersection(example.Origin, example.Direction, example.Center, example.rectSize, offset);
                }
            }
        }

        private void DrawIntersection(Vector2 origin, Vector2 direction, Vector2 center, Vector2 size, Vector2 displacement)
        {
#if unity_editor
            Vector2 p0 = displacement + origin;
            Vector2 d0 = direction;
            Vector2 p1 = displacement + center;
            Vector2 s = size;

            LemmingsShaderMathUtil util = new LemmingsShaderMathUtil();
            float2 n = new float2();
            double t = 0;
            bool collides = util.Ray2Rect(new LemmingsShaderMathUtil.Rect(p1, s), new float2(p0.x, p0.y), new float2(d0.x, d0.y), out t, out n);
            if (collides)
            {
                Gizmos.color = Color.green;

                Gizmos.DrawSphere(p0 + d0 * (float) t, 0.1f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(p0 + d0 * (float) t, (p0 + d0 * (float) t) + new Vector2(n.x, n.y));
                Gizmos.DrawLine(p0 + Vector2.up * -0.1f, p0 + Vector2.down * 0.1f);
                Gizmos.DrawIcon((p0 + d0 * (float) t) + new Vector2(n.x, n.y), "winbtn_win_max_h", true, Gizmos.color);
                Handles.Label(p0 + d0 * -.3f, $"t {t}");

                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }

            Handles.Label(p0, $"Origin {p0}");
            Handles.Label(p0 + d0 * 0.9f, $"Direction {d0}");
            Gizmos.DrawWireCube(p1, s);
            Gizmos.DrawLine(p0, p0 + d0);
            Gizmos.DrawIcon(p0 + d0, "winbtn_win_max_h", true, Gizmos.color);
#endif
        }
    }
}