Shader "BlitSeedsAlpha"
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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                if (!tex2Dlod(_MainTex, float4(i.uv, 0, 0)).a)
                {
                    return float4(0, 0, 0, 0);
                };
                return float4(
                    i.uv, (i.uv.y * _MainTex_TexelSize.z) + (i.uv.x * _MainTex_TexelSize.w) / (_MainTex_TexelSize.z *
                        _MainTex_TexelSize.w), 1);
            }
            ENDCG
        }
    }
}