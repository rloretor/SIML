Shader "Instanced/LemmingInstanced"
{
    Properties {}
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
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

            sampler2D _animationTexture;
            float4 _animationTexture_ST;
            float4 _animationTexture_TexelSize;

            int _animationFrames;


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
                float4 data: TEXCOORD1;
            };


            v2f vert(appdata v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                const Lemming lemming = _LemmingsBuffer[instanceID];
                v.vertex.xy *= _lemmingSize;
                v.vertex.xyz += float3(lemming.Position, 0);
                v.vertex.y -= _lemmingSize * 0.1;
                v.uv.y /= _animationFrames;
                v.uv.y -= frac(floor(_Time.y * lemming.Velocity.x * _animationFrames + instanceID) / _animationFrames);
                v.uv.x *= sign(lemming.Velocity.x+0.001);
                o.uv = v.uv;
                o.vertex = mul(UNITY_MATRIX_VP, v.vertex);
                o.data = float4(lemming.Acceleration.xy, instanceID, 0);
                return o;
            }


            float4 frag(v2f i):SV_Target
            {
                // i.uv.y /= 8;
                float4 tex_2d = tex2D(_animationTexture, i.uv);
                if (tex_2d.a == 0)
                {
                    discard;
                }

                return float4(tex_2d);
            }
            ENDCG
        }
    }
}