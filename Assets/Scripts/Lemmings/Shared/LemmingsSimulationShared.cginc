////VARIABLES

cbuffer once {float2 _texDimensions;float2 _MaxBound; float2 _MinBound; float _DeltaTime; uint _Instances;};
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
