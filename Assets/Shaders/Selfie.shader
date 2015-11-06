Shader "BRUTALISM/Selfie"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_Emission ("Emission", Range(0.0, 1.0)) = 0.5
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM

		#pragma surface surf Lambert fullforwardshadows vertex:vert
		#pragma target 3.0

		struct Input
		{
			float4 color : COLOR;
			float3 worldPos;
			float3 localPos;
		};

		fixed4 _Color;
		fixed _Emission;

		void vert (inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.localPos = v.vertex.xyz;
		}

		void surf(Input input, inout SurfaceOutput output)
		{
			fixed4 color = _Emission * input.color + (1 - _Emission) * _Color;

			output.Albedo = color.rgb;
			output.Emission = _Emission * input.color;
			output.Alpha = color.a;
		}

		ENDCG
	}

	FallBack "Diffuse"
}
