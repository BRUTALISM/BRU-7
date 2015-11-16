Shader "BRUTALISM/Selfie"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_Emission ("Emission", Range(0.0, 1.0)) = 0.5
		_AnimationDuration ("Animation Duration", Range(0.0, 5.0)) = 0.5
	}

	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		/*Tags { "RenderType"="Opaque" }*/
		LOD 200

		Pass
		{
			ZWrite On
			ColorMask 0
		}

		CGPROGRAM

		#pragma surface surf Lambert alpha fullforwardshadows vertex:vert
		/*#pragma surface surf Lambert fullforwardshadows vertex:vert*/
		#pragma target 3.0

		struct Input
		{
			float4 color : COLOR;
			float3 worldPos;
			float3 localPos;
			float4 tangent;
			float alpha;
		};

		fixed4 _Color;
		fixed _Emission;
		float _AnimationDuration;

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

			float fadeIn = saturate((_Time.y - v.tangent.x) / _AnimationDuration);
//			float fadeOut = saturate((v.tangent.y - _Time.y) / _AnimationDuration);
			float fadeOut = 1;
			float fadeInOut = fadeIn * step(_Time.y, v.tangent.x + _AnimationDuration) +
			                  fadeOut * step(v.tangent.x + _AnimationDuration, _Time.y);
			/*fadeInOut = smoothstep(0, 1, fadeInOut);*/
			/*v.vertex.xyz *= fadeInOut;*/

			o.localPos = v.vertex.xyz;
			o.tangent = v.tangent;

			// Calculate the alpha for this vertex based on whether we should fade in, show, or fade out
			o.alpha = fadeInOut;
//			o.alpha = 1;
		}

		void surf(Input input, inout SurfaceOutput output)
		{
			fixed4 color = _Emission * input.color + (1 - _Emission) * _Color;

			output.Albedo = color.rgb;
			output.Emission = _Emission * input.color;
			output.Alpha = input.alpha;
		}

		ENDCG
	}

	FallBack "Diffuse"
}
