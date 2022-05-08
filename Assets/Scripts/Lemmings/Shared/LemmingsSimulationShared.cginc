////VARIABLES

cbuffer once {float2 _texDimensions;float2 _MaxBound; float2 _MinBound; float _DeltaTime; uint
_Instances;float2 _lemmingSize;};
#define WorldSize (_MaxBound - _MinBound)

struct Lemming
{
    float2 Position;
    float2 Velocity;
    float2 Acceleration;
};

struct rect
{
    float2 Position;
    float2 Size;

    float2 min()
    {
        Position - Size / 2.0f;
    }

    float2 max()
    {
        Position + Size / 2.0f;
    }
};


////UTILITY
float2 computeUV(float2 position)
{
    return (position - _MinBound) / WorldSize;
}

float2 computePos(float2 uv)
{
    return ((uv * WorldSize) + _MinBound);
}

float2 computePixelUV(float2 uv, float2 d)
{
    return (floor(uv * _texDimensions) + d) / _texDimensions;
}

//projection b over a
float2 project(float2 b, float2 a)
{
    return (dot(a, b) / dot(a, a)) * a;
}

//https://github.com/ashima/webgl-noise/blob/master/src/noise2D.glsl
float hash(float2 x)
{
    return frac(sin(dot(x, float2(12.9898, 78.233))) * 43758.5453);
}


#define Rot(a)  float2x2(cos(a), sin(a),-sin(a), cos(a))


//Colision resolution


float Ray2Ray(float2 a, float2 ad, float2 b, float2 bd)
{
    #define dx (b.x - a.x)
    #define dy  (b.y - a.y)
    return (dy * bd.x - dx * bd.y) / ((bd.x * ad.y) - (bd.y * ad.x));
}

/*
bool Ray2Rect(rect b, float2 p0, float2 d, out double t, out float2 n)
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
        float2 pfinal = p0 + d * (float)t;
        n = (pfinal - b.Position);
        if (pfinal.x > b.min().x && pfinal.x < b.max().x)
        {
            n = float2(0, 1) * sign(n.y);
        }
        else
        {
            n = float2(1, 0) * sign(n.x);
            n = n.yx;
        }

        return true;
    }

    n = 0.0;
    return false;
}
*/
bool Ray2Rect(rect r, float2 p0, float2 D, out float t, out float2 n)
{
    #define _d  float2(1, 0)
    float2 X, Y;
    // find closest X axis and Y axis intersection points
    X[0] = Ray2Ray(p0, D, r.Position - r.Size / 2.0, _d.xy);
    Y[0] = Ray2Ray(p0, D, r.Position - r.Size / 2.0, _d.yx);
    X[1] = Ray2Ray(p0, D, r.Position + r.Size / 2.0, _d.xy);
    Y[1] = Ray2Ray(p0, D, r.Position + r.Size / 2.0, _d.yx);

    n[0] = max(min(X[0], X[1]), min(Y[0], Y[1])); // reusing existing register normal = tmin 
    n[1] = min(max(X[0], X[1]), max(Y[0], Y[1])); // replace r.position by tmin and r.size by tmax
    t = min(n[1], n[0]);
    if (!(n[1] < 0 || n[0] > n[1] || t > 1))
    {
        n = ((p0 + D * t) - r.Position);
        float2 a = float2(abs(n.x), abs(n.y));
        if (a.x <= a.y)
        {
            n = float2(0, 1) * sign(n.y);
        }
        else
        {
            n = float2(1, 0) * sign(n.x);
        }

        return true;
    }

    return false;
}
