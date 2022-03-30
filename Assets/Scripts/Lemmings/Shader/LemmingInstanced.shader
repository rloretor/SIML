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
            };

            StructuredBuffer<Lemming> _LemmingsBuffer;

            uint _Instances;

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
            };

            struct f2r
            {
                half4 normal : SV_Target0;
                half4 color : SV_Target1;
                float depth : SV_Depth;
            };

            float4x4 lookAtMatrix(float3 forward, float3 up)
            {
                float3 z = normalize(forward);
                float3 x = normalize(cross(z, up));
                float3 y = normalize(cross(z, x));

                return float4x4(
                    x.x, y.x, z.x, 0,
                    x.y, y.y, z.y, 0,
                    x.z, y.z, z.z, 0,
                    0, 0, 0, 1
                );
            }

            #define Rot(a)  float2x2(cos(a), sin(a),-sin(a), cos(a))

            v2f vert(appdata v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                const Lemming lemming = _LemmingsBuffer[instanceID];

                const float3 localSpaceCameraPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos.xyz, 1));
                //  float3 camVect = normalize(float3(lemming.Position.xy, 1) - localSpaceCameraPos);
                //const float4x4 rot = lookAtMatrix(v.vertex - localSpaceCameraPos, float3(0, 1, 0));
                //  v.vertex.xz = mul(Rot(acos(_Time.y)), v.vertex.xz);
                v.vertex.y += 0.5;
                v.vertex.xyz *= (5);
                // v.vertex = mul(rot, v.vertex);
                v.vertex.xyz += float3(lemming.Position, 0);

                //o.wPos = v.vertex;
                //o.sphereWPos = boid.position;
                //o.rayD = (o.wPos - _WorldSpaceCameraPos.xyz);
                // o.color = float4(boid.velocity, boid.dummy);
                o.vertex = mul(UNITY_MATRIX_VP, v.vertex);
                o.uv = v.uv;
                return o;
            }


            float4 frag(v2f i):SV_Target
            {
                return float4(1, 1, 1, 1);
            }
            ENDCG
        }
    }
}