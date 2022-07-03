Shader "JFA_Analysis"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            sampler2D _BitMap;

            uniform bool showDistance;
            uniform bool filterDistance;
            uniform bool boxFilterDistance;
            uniform bool flow;
            uniform bool pack;
            int bitOnValue;


            #define T(uv) tex2Dlod(_MainTex,float4(uv,0,0))
            #define cell(uv) T( uv).xyz
            #define D(uv) (length(cell(uv).xy-uv) )

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float3 hash31(float p)
            {
                float3 p3 = frac(float3(1, 1, 1) * p * float3(.1031, .1030, .0973));
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.xxy + p3.yzz) * p3.zyx);
            }

            float almostIdentity(float x, float m, float n)
            {
                if (x > m) return x;
                float a = 2.0 * n - m;
                float b = 2.0 * m - 3.0 * n;
                float t = x / m;
                return (a * t + b) * t * t + n;
            }

            float BoxFilter(float2 uv)
            {
                float3 acc = 0;
                for (int i = -5; i <= 5; i++)
                {
                    for (int j = -5; j <= 5; j++)
                    {
                        float2 uvTemp = uv + float2(i, j) * _MainTex_TexelSize;
                        acc += D(uvTemp);
                    }
                }
                return acc / (11 * 11);
            }


            float2 Central_Diff(float2 uv)
            {
                float2 d = float2(1, 0);
                float2 right = uv + d.xy * _MainTex_TexelSize;
                float2 left = uv - d.xy * _MainTex_TexelSize;
                float2 top = uv + d.yx * _MainTex_TexelSize;
                float2 bot = uv - d.yx * _MainTex_TexelSize;
                return normalize(float2(D(right) - D(left), D(top) - D(bot)) / 2.0);
            }


            float4 frag(v2f i) : SV_Target
            {
 
                if (showDistance)
                {
                    return D(i.uv);
                }
                if (filterDistance)
                {
                    return 1 - D(i.uv);
                }
                if (boxFilterDistance)
                {
                    return BoxFilter(i.uv);
                }
                if (flow)
                {
                    return float4(Central_Diff(i.uv) * 0.5 + 0.5, 0, 0);
                }

                return float4(cell(i.uv).xyz, 1); //float4(hash31((cell(i.uv).z) * 1013), 1);
            }
            ENDCG
        }
    }
}