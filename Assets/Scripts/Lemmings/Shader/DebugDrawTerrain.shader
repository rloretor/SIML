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

            uint _instances;
            sampler2D _analysisTexture;
            float4 _analysisTexture_ST;
            float4 _analysisTexture_TexelSize;

            sampler2D _collisionTexture;
            float4 _collisionTexture_ST;
            float4 _collisionTexture_TexelSize;
            float _width;
            float _height;
            float2 _boundsMin;
            float2 _boundsMax;
            #define _size (_boundsMax - _boundsMin)
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

            float2 computeUV(float2 position)
            {
                return (position - _boundsMin) / _size;
            }

            float2 computePos(float2 uv)
            {
                return ((uv * _size) + _boundsMin);
            }

            float2 computePixelUV(float2 uv)
            {
                return (floor(uv * _texDimensions) + 0.5) / _texDimensions;
            }

            #define Rot(a)  float2x2(cos(a), sin(a),-sin(a), cos(a))

            v2f vert(appdata v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                float row = ((instanceID) / _width * 1.0);
                float col = (fmod(instanceID, _width * 1.0));
                float2 uv = float2(col + 0.5, row + 0.5) / _texDimensions;
                float3 data = tex2Dlod(_analysisTexture, float4(uv.xy, 0, 0)).xyz;
                float2 p = computePos(uv);
                float2 pp = _boundsMin + (uv.xy + normalize(data.xy) * data.z) * _size;

                const float2 V = normalize(data.xy);
                v.vertex.y += 0.5;
                v.vertex.x *= 0.3 * (1.1 - v.vertex.y);
                v.vertex.x *= _size.x / _width * 0.3;
                v.vertex.y *= distance(pp, p);
                //  v.vertex.y += distance(pp, p) / 2;
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