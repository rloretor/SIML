Shader "Instanced/LemmingInstanced"
{
    Properties {}
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100
        ZWrite On
        // Cull off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "AutoLight.cginc"

            struct Lemming
            {
                float2 Position;
                float2 Velocity;
                float2 Acceleration;
            };

            StructuredBuffer<Lemming> _LemmingsBuffer;

            uint _Instances;
            sampler2D _collisionBitMap;
            float4 _collisionBitMap_ST;
            float4 _collisionBitMap_TexelSize;

            float2 _minBounds;
            float2 _maxBounds;
            #define _size (_maxBounds-_minBounds)

            struct appdata
            {
                float4 vertex : POSITION;
                float4 normal:NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color :COLOR0;
                float2 uv : TEXCOORD0;
                float3 v: TEXCOORD1;
            };

            struct f2r
            {
                half4 normal : SV_Target0;
                half4 color : SV_Target1;
                float depth : SV_Depth;
            };


            #define Rot(a)  float2x2(cos(a), sin(a),-sin(a), cos(a))

            float2 computeUV(float2 position)
            {
                return (position - _minBounds) / _size;
            }

            v2f vert(appdata v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                const Lemming lemming = _LemmingsBuffer[instanceID];
                const float S = length(lemming.Velocity);
                const float2 V = normalize(lemming.Velocity);
                v.vertex.y += 0.5;
                v.vertex.x *= (1 - v.vertex.y);
                v.vertex.y *= S * unity_DeltaTime.x;
                //v.vertex.xyz *= 10;
                v.vertex.xy = mul(Rot(atan2(V.x,V.y)), v.vertex.xy);
                v.vertex.xyz += float3(lemming.Position, 0);

                o.uv = computeUV(v.vertex);
                o.vertex = mul(UNITY_MATRIX_VP, v.vertex);
                o.v = float3(lemming.Acceleration, instanceID / 1.0 * _Instances);
                return o;
            }


            float4 frag(v2f i):SV_Target
            {
                return 1; //tex2D(_collisionBitMap, i.uv);
            }
            ENDCG
        }
    }
}