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
