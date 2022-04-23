using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Test.VisualProofs
{
    [Serializable]
    public class IntersectionTestExample
    {
        public Rect Rect;
        public Vector2 Origin;
        public Vector2 Direction;

        public Vector3 GetMin()
        {
            return Vector3.Min(
                Vector3.Min(
                    Vector3.Min(
                        Origin,
                        Origin + Direction),
                    Rect.min),
                Rect.max);
        }

        public Vector3 GetMax()
        {
            return Vector3.Max(
                Vector3.Max(
                    Vector3.Max(
                        Origin,
                        Origin + Direction),
                    Rect.min),
                Rect.max);
        }

        public Vector3 Size()
        {
            float3 size = GetMax() - GetMin();
            return Vector2.one * Mathf.Max(size.x, size.y);
        }
    }

    public class RayBoxIntersectionVisualProof : MonoBehaviour
    {
        public List<IntersectionTestExample> IntersectionTests = new List<IntersectionTestExample>();
        public int testsPerLine = 5;
        public int separationPerLine = 5;

        private void OnDrawGizmos()
        {
            IntersectionTestExample intersection = IntersectionTests[0];
            Vector3 offset = transform.position;
            DrawIntersection(intersection.Origin, intersection.Direction, intersection.Rect.center, intersection.Rect.size, offset);

            int index;
            for (index = 1; index < IntersectionTests.Count; index++)
            {
                intersection = IntersectionTests[index];
                if (index % testsPerLine == 0)
                {
                    offset.y = transform.position.y - separationPerLine * index / testsPerLine;
                    offset.x = transform.position.x;
                }
                else
                {
                    offset.x += (IntersectionTests[index].Rect.size * 2).x;
                }


                DrawIntersection(intersection.Origin, intersection.Direction, intersection.Rect.center, intersection.Rect.size, offset);
            }

            index = (int) Mathf.Ceil((float) index / testsPerLine) * testsPerLine;

            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 16; i++)
                {
                    float x = Mathf.Sin(i * Mathf.PI / 8);
                    float y = Mathf.Cos(i * Mathf.PI / 8);
                    Vector2 size = Vector2.one * 2.0f;
                    IntersectionTestExample example = new IntersectionTestExample()
                    {
                        Origin = size / 2 + new Vector2(x, y) * (j),
                        Direction = new Vector2(x, y),
                        Rect = new Rect(Vector2.zero, size)
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
                    DrawIntersection(example.Origin, example.Direction, example.Rect.center, example.Rect.size, offset);
                }
            }
        }

        private void DrawIntersection(Vector2 origin, Vector2 direction, Vector2 center, Vector2 size, Vector2 displacement)
        {
            Vector2 p0 = displacement + origin;
            Vector2 d0 = direction;
            Vector2 p1 = displacement + center;
            Vector2 s = size;

            LemmingsShaderMathUtil util = new LemmingsShaderMathUtil();
            float2 n = new float2();
            float t = 0;
            bool collides = util.Ray2Rect(new LemmingsShaderMathUtil.Rect(p1, s), new float2(p0.x, p0.y), new float2(d0.x, d0.y), out t, out n);
            if (collides)
            {
                Gizmos.color = Color.green;

                Gizmos.DrawSphere(p0 + d0 * t, 0.1f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(p0 + d0 * t, (p0 + d0 * t) + new Vector2(n.x, n.y));
                Gizmos.DrawLine(p0 + Vector2.up * -0.1f, p0 + Vector2.down * 0.1f);
                Gizmos.DrawIcon((p0 + d0 * t) + new Vector2(n.x, n.y), "winbtn_win_max_h", true, Gizmos.color);
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
        }
    }
}