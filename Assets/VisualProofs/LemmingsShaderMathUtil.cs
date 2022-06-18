using System;
using System.Drawing;
using System.Net.Sockets;
using Lemmings;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = System.Random;

namespace Test
{
    public class LemmingsShaderMathUtil
    {
        float max(float a, float b) => math.max(a, b);
        float min(float a, float b) => math.min(a, b);

        double max(double a, double b) => math.max(a, b);
        double min(double a, double b) => math.min(a, b);

        //projection b over a
        public static float2 project(float2 b, float2 a)
        {
            return (math.dot(a, b) / math.dot(a, a)) * a;
        }

        public struct Rect
        {
            public Rect(float2 p, float2 s)
            {
                Position = p;
                Size = s;
            }

            public float2 Position;
            public float2 Size;

            public Vector3 V3Pos()
            {
                return new Vector3(Position.x, Position.y, 1);
            }

            public Vector3 V3Size()
            {
                return new Vector3(Size.x, Size.y, 1);
            }

            public float2 min()
            {
                return Position - Size / 2.0f;
            }

            public float2 max()
            {
                return Position + Size / 2.0f;
            }

            public void DebugDrawPixel(Color c)
            {
                var p = V3Pos();
                var s = V3Size() / 2.0f;

                Debug.DrawLine(p + s, p + Vector3.right * s.x - Vector3.up * s.y, c);
                Debug.DrawLine(p + Vector3.right * s.x - Vector3.up * s.y, p - s, c);
                Debug.DrawLine(p - s, p - Vector3.right * s.x + Vector3.up * s.y, c);
                Debug.DrawLine(p - Vector3.right * s.x + Vector3.up * s.y, p + s, c);
            }
        };

        public float Ray2Ray(float2 a, float2 ad, float2 b, float2 bd)
        {
            float dx = (b.x - a.x);
            float dy = (b.y - a.y);
            return (dy * bd.x - dx * bd.y) / ((bd.x * ad.y) - (bd.y * ad.x));
        }

        public bool Ray2Rect(Rect b, float2 p0, float2 d, out double t, out float2 n)
        {
            double2 dinv = 1 / d;
            double t1 = (b.min()[0] - p0[0]) * dinv[0];
            double t2 = (b.max()[0] - p0[0]) * dinv[0];

            double tmin = min(t1, t2);
            double tmax = max(t1, t2);

            for (int i = 1; i < 2; ++i)
            {
                t1 = (b.min()[i] - p0[i]) * dinv[i];
                t2 = (b.max()[i] - p0[i]) * dinv[i];

                tmin = max(tmin, min(min(t1, t2), tmax));
                tmax = min(tmax, max(max(t1, t2), tmin));
            }

            t = tmin;
            if (tmax > max(tmin, 0.0))
            {
                float2 pfinal = p0 + d * (float) t;
                n = (pfinal - b.Position);
                Debug.DrawLine(p0.ToVector2(), pfinal.ToVector2());
                if (pfinal.x > b.min().x && pfinal.x < b.max().x)
                {
                    n = new float2(0, 1) * math.sign(n.y);
                }
                else
                {
                    n = new float2(1, 0) * math.sign(n.x);
                    n = n.yx;
                }

                return true;
            }

            n = float2.zero;
            return false;
        }

        public bool Ray2Rect(Rect r, float2 p0, float2 D, out float t, out float2 n)
        {
            float2 _d = new float2(1, 0);
            n = new float2(0, 0);
            float2 X = new float2();
            float2 Y = new float2();
            // find closest X axis and Y axis intersection points
            X[0] = Ray2Ray(p0, D, r.Position - r.Size / 2.0f, _d.xy);
            Y[0] = Ray2Ray(p0, D, r.Position - r.Size / 2.0f, _d.yx);
            X[1] = Ray2Ray(p0, D, r.Position + r.Size / 2.0f, _d.xy);
            Y[1] = Ray2Ray(p0, D, r.Position + r.Size / 2.0f, _d.yx);

            n[0] = max(min(X[0], X[1]), min(Y[0], Y[1])); // reusing existing register normal = tmin 
            n[1] = min(max(X[0], X[1]), max(Y[0], Y[1])); // replace r.position by tmin and r.size by tmax
            t = min(n[1], n[0]);
            if (!(n[1] < 0 || n[0] > n[1] || t > 1))
            {
                n = ((p0 + D * t) - r.Position);
                float2 a = new float2(Math.Abs(n.x), Math.Abs(n.y));
                if (a.x <= a.y)
                {
                    n = new float2(0, 1) * math.sign(n.y);
                }
                else
                {
                    n = new float2(1, 0) * math.sign(n.x);
                    n = n.yx;
                }

                return true;
            }

            return false;
        }

        public static float2 computeUV(float2 position, float2 botLeft, float2 topRight)
        {
            return (position - botLeft) / (topRight - botLeft);
        }

        public static float2 computePos(float2 uv, float2 botLeft, float2 topRight)
        {
            return ((uv * (topRight - botLeft)) + botLeft);
        }

        public static float2 computePixelUV(float2 uv, float2 d, float width, float height)
        {
            float2 puv = uv;
            float2 _texDimensions = new float2(width, height);
            puv.x = (Mathf.Floor(uv.x * _texDimensions.x) + d.x) / _texDimensions.x;
            puv.y = (Mathf.Floor(uv.y * _texDimensions.y) + d.y) / _texDimensions.y;
            return puv;
        }

        public static float2 pointInSquarePerimeter(float2 p, float2 v, float2 min, float2 max)
        {
            float2 nPos = math.clamp(p + v, min, max);
            nPos = (nPos - min) / (max - min);
            return (nPos * 2.0f - 1.0f) * (max - min) / 2;
        }

        public static float2 computePixel(float2 uv, float width, float height)
        {
            return uv * new float2(width, height);
        }

        public static float2 GetCardinalDirection(float2 p, float2 v, float2 min, float2 max)
        {
            float2 nPos = math.clamp(p, min, max);
            nPos = (nPos - min) / (max - min);
            nPos = nPos * 2.0f - 1.0f;
            //float2 nPos = pointInSquarePerimeter(p, v, min, max);

            nPos = math.normalize(nPos);
            float2 absnv = math.abs(nPos);
            float m = math.cmin(absnv);
            float2 s = math.sign(nPos) * math.ceil(absnv - m);
            if (s.x == s.y)
            {
                s = new float2(0, 1) * -math.sign(v.y + 0.0001f);
            }

            return s;
        }

        public static void FixCollision(float2 position, float2 prevPos, Rect pixel, float2 lemmingSize, ref LemmingKinematicModel lemming)
        {
            float2 sign = GetCardinalDirection(prevPos, lemming.Velocity, pixel.Position - pixel.Size * 0.5f, pixel.Position + pixel.Size * 0.5f);

            var proj = project((position - pixel.Position), sign.yx);
            // proj = UnityEngine.Random.Range(1.02f, 1.3f) * proj;
            float2 displacement = proj + sign * (pixel.Size * 0.5f + lemmingSize * 0.5f);
//normalization missing
            lemming.Position = pixel.Position + displacement;
            float L = math.length(lemming.Velocity);
            lemming.Velocity += (Vector2) math.abs(sign.yx * 0.01f);

            lemming.Velocity = math.normalize(math.abs(sign.yx)) * (float2) lemming.Velocity;
            //  lemming.Velocity.y *= 0.1f;
            lemming.Velocity = math.normalize(lemming.Velocity) * math.clamp(L, 0, lemmingSize.x * 2);
        }
    }

    public static class float2Extensions
    {
        public static Vector2 ToVector2(this float2 v)
        {
            return new Vector2(v.x, v.y);
        }
    }
}