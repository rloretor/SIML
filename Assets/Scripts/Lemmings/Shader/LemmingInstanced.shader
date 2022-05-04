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
            #include "../Shared/LemmingsSimulationShared.cginc"


            StructuredBuffer<Lemming> _LemmingsBuffer;

            sampler2D _collisionBitMap;
            float4 _collisionBitMap_ST;
            float4 _collisionBitMap_TexelSize;


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


            v2f vert(appdata v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                const Lemming lemming = _LemmingsBuffer[instanceID];
                const float S = length(lemming.Velocity);
                const float2 V = normalize(lemming.Velocity);
                //v.vertex.y += 0.5;
                //v.vertex.x *= (1 - v.vertex.y);
                //v.vertex.y *= S * unity_DeltaTime.x;
                v.vertex.x *= 0.01;
                v.vertex.y *= 0.02;
                //   v.vertex.xy = mul(Rot(atan2(V.x,V.y)), v.vertex.xy);
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