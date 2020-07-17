
Shader "Unlit/Warping"
{
    Properties
    {
        _NoiseTex("NoiseTexture",2D) = "white" {}
        _DistortTimeFactor("DistortTimeFactor",Range(0,1)) = 1
        _DistortStrength("DistortStrength",Range(0,1)) = 0.2
        _UVLength("_UVLength",Range(0,1)) = 1
    }
    SubShader
    {
        //Tags { "RenderType"="Opaque" }
        //LOD 100
        ZWrite  Off
        GrabPass
        {
            "_GrabTempTex"
        }

        Pass
        {
            Tags
            {
                "RenderType" = "Transparent"
                "Queue" = "Transparent+1"
            }
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
                float4 grabPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float4 pos : SV_POSITION;
            };

            sampler2D  _GrabTempTex;
            float4 _GrabTempTex_ST;
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            float _DistortTimeFactor;
            float _DistortStrength;
            float _UVLength;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.pos);
                o.uv = TRANSFORM_TEX(v.uv, _NoiseTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                float4 offset = tex2D(_NoiseTex,i.uv - _Time.xy * _DistortTimeFactor);
                i.grabPos.xy -= offset.xy * _DistortStrength;
                fixed4 col = tex2Dproj(_GrabTempTex, i.grabPos);
                return col;
            }
            ENDCG
        }
    }
}
