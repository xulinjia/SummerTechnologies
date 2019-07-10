// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/OutLight"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Amount("Extrusion Amount", Range(2,10)) = 0
        _OutLightPow("OutLightPow",Float) = 5
        _OutLightStrength("Transparency", Float) = 15 //光晕强度
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
		}

        sampler2D _MainTex;

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }

        ENDCG
            Pass
        {
            Tags{ "LightMode" = "Always" }
            LOD 200
            Cull Front
            //ZWrite Off
            Blend SrcAlpha  One
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            fixed4 _Color;
            float _OutLightPow;
            float _OutLightStrength;
            float _Amount;
             struct v2f
            {
                 float4 vertex:SV_POSITION;
                 float3 normal : NORMAL;
                 float3 worldPos : TEXCOORD0;
             };

             v2f vert(appdata_full IN)
             {
                 v2f o;
                 o.worldPos = mul(unity_ObjectToWorld, IN.vertex).xyz * _Amount;
                 o.vertex = IN.vertex + float4(IN.normal * _Amount, 0);
                 o.vertex = UnityObjectToClipPos(o.vertex);
                 o.normal = UnityObjectToWorldNormal(IN.normal);
                 return o;
             }

             fixed4 frag(v2f i) : COLOR
             {
                 half3 worldViewDir = -normalize(UnityWorldSpaceViewDir(i.worldPos));
                 float m = saturate(dot(worldViewDir, i.normal));
                 float4 color = _Color;
                 color.a = pow(m, _OutLightPow);
                 color.a *= _OutLightStrength * dot(worldViewDir, i.normal);
                 return color;
             }
             ENDCG
        }
    }
    FallBack "Diffuse"
}
