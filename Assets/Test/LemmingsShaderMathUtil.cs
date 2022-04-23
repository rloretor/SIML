using System;
using Unity.Mathematics;

namespace Test
{
    public class LemmingsShaderMathUtil
    {
        float max(float a, float b) => math.max(a, b);
        float min(float a, float b) => math.min(a, b);

        public struct Rect
        {
            public Rect(float2 p, float2 s)
            {
                Position = p;
                Size = s;
            }

            public float2 Position;
            public float2 Size;
        };

        public float Ray2Ray(float2 a, float2 ad, float2 b, float2 bd)
        {
            float dx = (b.x - a.x);
            float dy = (b.y - a.y);
            return (dy * bd.x - dx * bd.y) / ((bd.x * ad.y) - (bd.y * ad.x));
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
                }

                return true;
            }

            return false;
        }
    }
}