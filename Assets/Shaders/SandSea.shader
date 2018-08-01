﻿Shader "Custom/SandSea" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		[Header(SandSea Settings)]
		_SeaDepth ("SandSea Depth", Float) = 10
		
		[Header(Waves total steepness at most 1)]
		_SpeedGravity ("Speed/Gravity", Float) = 9.8
		_WaveA ("Wave A (dir, steepness, wavelength)", Vector) = (1, 0, 0.5, 10)
		_WaveB ("Wave B", Vector) = (0, 1, 0.25, 10)
		_WaveC ("Wave C", Vector) = (1, 1, 0.15, 4)

		[Header(Player Displacement)]
		_FlatRange ("Flat Range", Float) = 10
		_FlatRangeExt ("Flat Range Extension", Float) = 2
		_DispTex ("Displacement (B/W)", 2D) = "white" {}
		_DispStrength ("Displacement tex strength", Float) = 0.2

		[HideInInspector]
		_PlayerPosition ("Player Position", Vector) = (0, 0, 0, 0)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert addshadow

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float _SpeedGravity;
		float4 _WaveA, _WaveB, _WaveC;

		float3 _PlayerPosition;
		float _FlatRange , _FlatRangeExt, _SeaDepth, _DispStrength;
		sampler2D _DispTex;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		float3 GerstnerWave (float4 wave, float3 p, inout float3 tangent, inout float3 binormal, float distModifier) {
		    float steepness = wave.z * distModifier;
		    float wavelength = wave.w;
		    float k = 2 * UNITY_PI / wavelength;
			float c = sqrt(_SpeedGravity / k);
			float2 d = normalize(wave.xy);
			float f = k * (dot(d, p.xz) - c * _Time.y);
			float a = steepness / k;

			float sinF = sin(f);
			float cosF = cos(f);
			float steepSinF = steepness * sinF;
			float steepCosF = steepness * cosF;

			tangent += float3(
				-d.x * d.x * steepSinF,
				d.x * steepCosF,
				-d.x * d.y * steepSinF
			);
			binormal += float3(
				-d.x * d.y * steepSinF,
				d.y * steepCosF,
				-d.y * d.y * steepSinF
			);
			return float3(
				d.x * (a * cosF),
				a * sinF,
				d.y * (a * cosF)
			);
		}

		float Displacement(float3 worldPoint, float2 texCoord, inout float3 tangent, inout float3 binormal, inout float playerDistModifier) {
			float depthModifier = 1;

			float3 toPlayer = _PlayerPosition.xyz - worldPoint;
			float yModifier = 1 - (toPlayer.y / _SeaDepth);

			if (yModifier < 0)
				yModifier = 0;
			else if (yModifier > 1)
				yModifier = 1;

			if (yModifier > 0){
				float playerDistXZ = sqrt(toPlayer.x * toPlayer.x + toPlayer.z * toPlayer.z);
				if (playerDistXZ < (_FlatRange + _FlatRangeExt) * yModifier) {
					playerDistModifier = (playerDistXZ - _FlatRangeExt * yModifier) / (_FlatRange * yModifier);
					if (playerDistModifier < 0){
						playerDistModifier = 0;
					}

					depthModifier = playerDistModifier + (1 - yModifier);

					#if !defined(SHADER_API_OPENGL)
					fixed4 dispTextSample = tex2Dlod (_DispTex, float4(texCoord, 0, 0));
					depthModifier += (dispTextSample.r - 0.5) * _DispStrength;
					#endif

					if (depthModifier > 1)
						depthModifier = 1;
				}
			}

			if (depthModifier < 1){
				float2 d = normalize(float2(-toPlayer.x, -toPlayer.z));
				tangent += float3(
					-d.x * d.x * (1 - depthModifier),
					d.x * (1 - depthModifier),
					-d.x * d.y * (1 - depthModifier)
				);
				binormal += float3(
					-d.x * d.y * (1 - depthModifier),
					d.y * (1 - depthModifier),
					-d.y * d.y * (1 - depthModifier)
				);
			}

			return _SeaDepth * depthModifier;
		}

		void vert(inout appdata_full vertexData) {
			float3 gridPoint = vertexData.vertex.xyz;
			float3 worldPoint = mul(unity_ObjectToWorld, vertexData.vertex).xyz;
			float3 tangent = float3(1, 0, 0);
			float3 binormal = float3(0, 0, 1);

			float playerDistModifier = 1;

			float3 p = gridPoint;
			p.y += Displacement(worldPoint, vertexData.texcoord.xy, tangent, binormal, playerDistModifier);
			p += GerstnerWave(_WaveA, worldPoint, tangent, binormal, playerDistModifier);
			p += GerstnerWave(_WaveB, worldPoint, tangent, binormal, playerDistModifier);
			p += GerstnerWave(_WaveC, worldPoint, tangent, binormal, playerDistModifier);
			float3 normal = normalize(cross(binormal, tangent));
			vertexData.vertex.xyz = p;
			vertexData.normal = normal;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
