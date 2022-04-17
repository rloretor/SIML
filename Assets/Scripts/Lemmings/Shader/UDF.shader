Shader "UDF"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
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
            sampler2D _BitMap;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            int isNegative = 1;
            #define T(uv) tex2Dlod(_MainTex,float4(uv,0,0))
            #define cell(uv) tex2D(_MainTex, uv).xyz
            #define D(uv) (sign(isNegative) *length(cell(uv).xy-uv)/2.0)

            float2 Central_Diff(float2 uv)
            {
                float2 d = float2(1, 0);
                float2 right = uv + d.xy * _MainTex_TexelSize;
                float2 left = uv - d.xy * _MainTex_TexelSize;
                float2 top = uv + d.yx * _MainTex_TexelSize;
                float2 bot = uv - d.yx * _MainTex_TexelSize;
                return normalize(float2(D(right) - D(left), D(top) - D(bot)) / 2.0);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                if (!(bool)tex2D(_BitMap, i.uv).r)
                    discard;

                return float4(Central_Diff(i.uv), abs(D(i.uv)), 1);
            }
            ENDCG
        }
    }
}