Shader "Custom/Toon"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _RampThreshold("RampThreshold",float) = 0.5
        _RampSmooth("RampSmooth",float) = 0
        _SColor("SColor",Color)= (1,1,1,1)
        _HColor("HColor",Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Toon fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        float _RampThreshold;
        float _RampSmooth;
        fixed4 _HColor;
        fixed4 _SColor;

        struct Input
        {
            float2 uv_MainTex;
        };

        half4 LightingToon(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
        {
            float ndl = dot(s.Normal, lightDir) * 0.5 + 0.5;
            half4 c;
            fixed3 ramp = smoothstep(_RampThreshold - _RampSmooth * 0.5, _RampThreshold + _RampSmooth * 0.5, ndl);
            _SColor = lerp(_HColor, _SColor, _SColor.a);
            float3 rampColor = lerp(_SColor.rgb, _HColor.rgb, ramp);
            c.rgb = s.Albedo * _LightColor0.rgb * rampColor;
            c.a = s.Alpha;
            return c;
        }

        fixed4 _Color;
        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
