Shader "Instanced/DebugDrawTerrain"
{
    Properties {}
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue" = "Transparent"
        }
        LOD 100
        ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "AutoLight.cginc"
            #include "../Shared/LemmingsSimulationShared.cginc"
            uint _instances;

            sampler2D _collisionBitMap;
            float4 _collisionBitMap_ST;
            float4 _collisionBitMap_TexelSize;
            sampler2D _terrainAnalysisTexture;
            float4 _terrainAnalysisTexture_ST;
            float4 _terrainAnalysisTexture_TexelSize;

            float _width;
            float _height;

            #define _texDimensions (float2(_width,_height))

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

            v2f vert(appdata v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                float row = ((instanceID) / _width * 1.0);
                float col = (fmod(instanceID, _width * 1.0));
                float2 uv = float2(col + 0.5, row + 0.5) / _texDimensions;
                float3 data = tex2Dlod(_terrainAnalysisTexture, float4(uv.xy, 0, 0)).xyz;
                float2 p = computePos(uv);
                float2 pp = computePos(uv + data.xy * data.z);

                const float2 V = normalize(data.xy);
                // v.vertex.y += 0.5;
                v.vertex.x *= 0.3 * (1.1 - v.vertex.y);
                v.vertex.x *= WorldSize.x / _width * 0.3;
                v.vertex.y *= distance(pp, p);
                // v.vertex.y += distance(pp, p) / 2;
                v.vertex.xy = mul(Rot(atan2(V.x,V.y)), v.vertex.xy);


                v.vertex.xy += float3(p, 0);
                o.color.xy = uv;

                o.vertex = mul(UNITY_MATRIX_VP, v.vertex);
                o.uv = v.uv;
                o.v = float3(V, data.z);
                return o;
            }


            float4 frag(v2f i):SV_Target
            {
                if (abs(i.v.z) > 0.01)
                {
                    discard;
                }
                return float4(i.v.xy * 2 - 1.0, 1, 1);
            }
            ENDCG
        }
    }
}