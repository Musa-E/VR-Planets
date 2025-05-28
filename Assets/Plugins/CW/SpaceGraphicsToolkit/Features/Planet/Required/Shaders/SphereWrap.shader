Shader "Unlit/SphereWrap"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Tiling ("Tiling", Float) = 10
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

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
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
			};

			sampler2D _MainTex;
			float _Tiling;

			float SGT_Hash(float n)
			{
				return frac(sin(n) * 43728.1453);
			}

			float SGT_Noise(float3 x)
			{
				float3 p = floor(x); float3 f = frac(x); f = f * f * (3.0 - 2.0 * f); float n = p.x + p.y * 55.0 + p.z * 101.0;
				return lerp(
					lerp(lerp(SGT_Hash(n      ), SGT_Hash(n +   1.0), f.x), lerp(SGT_Hash(n+ 55.0), SGT_Hash(n +  56.0), f.x), f.y),
					lerp(lerp(SGT_Hash(n+101.0), SGT_Hash(n + 102.0), f.x), lerp(SGT_Hash(n+156.0), SGT_Hash(n + 157.0), f.x), f.y), f.z);
			}

			float2 SGT_Rotate(float2 p, float angle)
			{
				float c = cos(angle); float s = sin(angle); return float2( p.x * c - p.y * s, p.x * s + p.y * c);
			}

			float4 SGT_SampleFlat(sampler2D samp, float2 coord, float noise)
			{
				float i = floor(noise);
				float j = i + 1.0f;
				float p = noise - i; p = p * p * (3.0f - 2.0f * p);

				float2 coordX  = ddx(coord);
				float2 coordY  = ddy(coord);
				float4 sampleA = tex2Dgrad(samp, SGT_Rotate(coord, i), SGT_Rotate(coordX, i), SGT_Rotate(coordY, i));
				float4 sampleB = tex2Dgrad(samp, SGT_Rotate(coord, j), SGT_Rotate(coordX, j), SGT_Rotate(coordY, j));

				return lerp(sampleA, sampleB, p);
			}

			float4 SGT_SampleSpherical(sampler2D samp, float3 direction, float tiling)
			{
				float  u = 0.75f - atan2(direction.z, direction.x) / UNITY_PI * 0.5f;
				float  v = 0.5f + asin(direction.y) / UNITY_PI;
				float  p = saturate((abs(v - 0.5f) - 0.2f) * 20.0f); p = p * p * (3.0f - 2.0f * p);
				float4 c = float4(float2(u, v * 0.5f), direction.xz * 0.25f) * tiling;
				float2 n = SGT_Noise(direction * tiling * 0.2f) * 8 + c.yw * 2;

				return lerp(SGT_SampleFlat(samp, c.xy, n.x), SGT_SampleFlat(samp, c.zw, n.y), p);
			}

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv     = v.uv;
				o.normal = v.normal;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float3 p = i.normal;
				fixed4 col = SGT_SampleSpherical(_MainTex, p, _Tiling);
				return col;
			}
			ENDCG
		}
	}
}
