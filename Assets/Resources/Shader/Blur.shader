Shader "FastBlur"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    sampler2D _MainTex;

    half4 _MainTex_TexelSize;
    half4 _Parameter;

    struct v2f_tap
    {
        float4 pos : SV_POSITION;
        half2 uv20 : TEXCOORD0;
        half2 uv21 : TEXCOORD1;
        half2 uv22 : TEXCOORD2;
        half2 uv23 : TEXCOORD3;
    };

    v2f_tap vert4Tap(appdata_img v)
    {
        v2f_tap o;

        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv20 = v.texcoord + _MainTex_TexelSize.xy;
        o.uv21 = v.texcoord + _MainTex_TexelSize.xy * half2(-_Parameter.x, -_Parameter.x);
        o.uv22 = v.texcoord + _MainTex_TexelSize.xy * half2(_Parameter.x, -_Parameter.x);
        o.uv23 = v.texcoord + _MainTex_TexelSize.xy * half2(-_Parameter.x, _Parameter.x);

        return o;
    }

    fixed4 fragDownsample(v2f_tap i) : COLOR
    {
        fixed4 color = tex2D(_MainTex, i.uv20) * 0.25;
        color += tex2D(_MainTex, i.uv21) * 0.25;
        color += tex2D(_MainTex, i.uv22) * 0.25;
        color += tex2D(_MainTex, i.uv23) * 0.25;

        //ignore alpha channel to kill artifacts from blurring ui screens
        color.a = 1;
        return color;
    }

    // weight curves

    static const half curve[7] = {0.0205, 0.0855, 0.232, 0.324, 0.232, 0.0855, 0.0205}; // gauss'ish blur weights

    static const half4 curve4[7] = {
        half4(0.0205, 0.0205, 0.0205, 0), half4(0.0855, 0.0855, 0.0855, 0), half4(0.232, 0.232, 0.232, 0),
        half4(0.324, 0.324, 0.324, 1), half4(0.232, 0.232, 0.232, 0), half4(0.0855, 0.0855, 0.0855, 0),
        half4(0.0205, 0.0205, 0.0205, 0)
    };

    struct v2f_withBlurCoords8
    {
        float4 pos : SV_POSITION;
        half4 uv : TEXCOORD0;
        half2 offs : TEXCOORD1;
    };

    struct v2f_withBlurCoordsSGX
    {
        float4 pos : SV_POSITION;
        half2 uv : TEXCOORD0;
        half4 offs[3] : TEXCOORD1;
    };

    v2f_withBlurCoords8 vertBlurHorizontal(appdata_img v)
    {
        v2f_withBlurCoords8 o;
        o.pos = UnityObjectToClipPos(v.vertex);

        o.uv = half4(v.texcoord.xy, 1, 1);
        o.offs = _MainTex_TexelSize.xy * half2(1.0, 0.0) * _Parameter.x;

        return o;
    }

    v2f_withBlurCoords8 vertBlurVertical(appdata_img v)
    {
        v2f_withBlurCoords8 o;
        o.pos = UnityObjectToClipPos(v.vertex);

        o.uv = half4(v.texcoord.xy, 1, 1);
        o.offs = _MainTex_TexelSize.xy * half2(0.0, 1.0) * _Parameter.y;

        return o;
    }

    half4 fragBlur8(v2f_withBlurCoords8 i) : COLOR
    {
        half2 uv = i.uv.xy;
        half2 netFilterWidth = i.offs;
        half2 coords = uv - netFilterWidth * 3.0;

        half4 color = 0;
        for (int l = 0; l < 7; l++)
        {
            half4 tap = tex2D(_MainTex, coords);
            color += tap * curve4[l];
            coords += netFilterWidth;
        }

        return color;
    }
    ENDCG

    SubShader
    {
        ZTest Off Cull Off ZWrite Off Blend Off
        Fog
        {
            Mode off
        }

        // 0
        Pass
        {

            CGPROGRAM
            #pragma vertex vert4Tap
            #pragma fragment fragDownsample
            #pragma fragmentoption ARB_precision_hint_fastest
            ENDCG

        }

        // 1
        Pass
        {
            ZTest Always
            Cull Off

            CGPROGRAM
            #pragma vertex vertBlurVertical
            #pragma fragment fragBlur8
            #pragma fragmentoption ARB_precision_hint_fastest
            ENDCG
        }

        // 2
        Pass
        {
            ZTest Always
            Cull Off

            CGPROGRAM
            #pragma vertex vertBlurHorizontal
            #pragma fragment fragBlur8
            #pragma fragmentoption ARB_precision_hint_fastest
            ENDCG
        }
    }

    FallBack Off
}