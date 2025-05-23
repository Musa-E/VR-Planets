Shader "Azure[Sky] Dynamic Skybox/Fog Scattering Renderer"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }

    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vertex_program
            #pragma fragment fragment_program
            #pragma target 3.0
            #include "UnityCG.cginc"


            //  Start: LuxWater
            #pragma multi_compile __ LUXWATER_DEFERREDFOG

            #if defined(LUXWATER_DEFERREDFOG)
                sampler2D _UnderWaterMask;
                float4 _LuxUnderWaterDeferredFogParams; // x: IsInsideWatervolume?, y: BelowWaterSurface shift, z: EdgeBlend
            #endif
            //  End: LuxWater


            // Textures
            uniform sampler2D _MainTex, _Azure_FogScatteringDataTexture, _CameraDepthTexture;
            uniform float4    _MainTex_TexelSize;

            // Attributes transfered from the mesh data to the vertex program
            struct Attributes
            {
                float4 vertex   : POSITION;
                float4 texcoord : TEXCOORD0;
            };

            // Attributes transfered from the vertex program to the fragment program
            struct Varyings
            {
                float4 Position        : SV_POSITION;
                float2 ScreenUV 	   : TEXCOORD0;
                float2 DepthUV  	   : TEXCOORD1;
            };

            // Vertex shader program
            Varyings vertex_program(Attributes v)
            {
                Varyings Output = (Varyings)0;

                v.vertex.z = 0.1f;
                Output.Position = UnityObjectToClipPos(v.vertex);
                Output.ScreenUV = v.texcoord.xy;
                Output.DepthUV = v.texcoord.xy;

                //#if UNITY_UV_STARTS_AT_TOP
                //if (_MainTex_TexelSize.y < 0.0f)
                //{
                //    Output.ScreenUV.y = 1.0f - Output.ScreenUV.y;
                //    //Output.DepthUV.y = 1.0f - Output.DepthUV.y; // Not sure this one is required!
                //}
                //#endif

                return Output;
            }

            // Fragment shader program
            float4 fragment_program(Varyings Input) : SV_Target
            {
                // Original scene
                float3 screen = tex2D(_MainTex, Input.ScreenUV).rgb;
                float depth = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, Input.DepthUV)));
                if(depth == 1.0f) return float4(screen, 1.0f);

                // Fog scattering data
                float4 fogData = tex2D(_Azure_FogScatteringDataTexture, Input.ScreenUV);



                //  Start: LuxWater
                #if defined(LUXWATER_DEFERREDFOG)
                half4 fogMask = tex2D(_UnderWaterMask, Input.ScreenUV);
                float watersurfacefrombelow = DecodeFloatRG(fogMask.ba);

                // Get distance and lower it a bit in order to handle edge blending artifacts (edge blended parts would not get ANY fog)
                float dist = (watersurfacefrombelow - depth) + _LuxUnderWaterDeferredFogParams.y * _ProjectionParams.w;
                // Fade fog from above water to below water
                float fogFactor = saturate(1.0 + _ProjectionParams.z * _LuxUnderWaterDeferredFogParams.z * dist); // 0.125 
                // Clamp above result to where water is actually rendered
                fogFactor = (fogMask.r == 1) ? fogFactor : 1.0;
                // Mask fog on underwarter parts - only if we are inside a volume (bool... :( )
                if (_LuxUnderWaterDeferredFogParams.x)
                {
                    fogFactor *= saturate(1.0 - fogMask.g * 8.0);
                    if (dist < -_ProjectionParams.w * 4 && fogMask.r == 0 && fogMask.g < 1.0)
                    {
                        fogFactor = 1.0;
                    }
                }
                // Tweak fog factor
                fogData.a *= fogFactor;
                #endif
                //  End: LuxWater



                // Output color
                float4 outputColor = lerp(float4(screen, 1.0f), float4(fogData.rgb, 1.0f), fogData.a);
                return outputColor;
            }

            ENDHLSL
        }
    }
}