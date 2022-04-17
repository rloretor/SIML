Shader "AnalyticalSDFs"
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
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            int ignoreAlpha = 0;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float sdBox(in float2 p, in float2 b)
            {
                float2 d = abs(p) - b;
                return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
            }

            float circleSDF(float2 p)
            {
                return (length(p) - 0.25);
            }

            float sdf(float2 p)
            {
                return sdBox(p, float2(0.5, 0.3));
            }

            float2 Central_Diff(float2 uv)
            {
                float2 d = float2(1, 0);
                const float2 right = uv + d.xy * _MainTex_TexelSize;
                const float2 left = uv - d.xy * _MainTex_TexelSize;
                const float2 top = uv + d.yx * _MainTex_TexelSize;
                const float2 bot = uv - d.yx * _MainTex_TexelSize;
                return normalize(float2(sdf(right) - sdf(left), sdf(top) - sdf(bot)) / 2.0);
            }


            float4 frag(v2f i) : SV_Target
            {
                float2 p = i.uv * 2 - 1.;
                return float4(Central_Diff(p), sdf(p), ceil(max(0, sdf(p)) + ignoreAlpha));
            }
            ENDCG
        }
    }
}