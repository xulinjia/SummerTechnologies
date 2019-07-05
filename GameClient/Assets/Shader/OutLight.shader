Shader "Custom/OutLight"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Amount("Extrusion Amount", Range(-1,1)) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
		struct Input
		{
			float2 uv_MainTex;
			float3 worldPos;
			float3 worldNormal;
			float3 outVertex;
		};

        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf OutLight vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

		float4 LightingOutLight(SurfaceOutput s, float3 lightDir,half3 viewDir, half atten)
		{
			float4 c;
			float NL = max(0,1- dot(s.Normal, viewDir)) * atten;
			c.rgb = s.Albedo * NL ;
			c.a = s.Alpha;
			return c;
		}

		float _Amount;

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
            v.vertex.xyz += v.normal * _Amount;
			o.outVertex.xyz = v.vertex + v.normal * _Amount;
		}

        sampler2D _MainTex;

        half _Glossiness;
        half _Metallic;
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

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200
         Pass
        {
         Tags{ "LightMode" = "Always" }
            LOD 200
            Blend Zero One
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
             struct v2f
            {
            };

            v2f vert(appdata_full IN)
            {

            }

            fixed4 frag(v2f IN) : SV_Target
            {
                return fixed4(1,0,0,0.5);
            }
            ENDCG

        }
    }
    
    FallBack "Diffuse"
}
