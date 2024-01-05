Shader "VertexAnimation/VertexAnimationExampleShader"
{
    Properties
    {
        [Header(SurfaceData)]
        _BaseColor ("Base color", Color) = (1,1,1,1)
        _BaseMap ("Texture", 2D) = "white" {}
        _Emission ("Emission", Float) = 0

        [Header(VertexAnimation)]
        _AnimTexPos0 ("Animation Texture Position 0", 2D) = "white" {}
        _AnimTexNorm0 ("Animation Texture Normal 0", 2D) = "white" {}
        _AnimTexPos1 ("Animation Texture Position 1", 2D) = "white" {}
        _AnimTexNorm1 ("Animation Texture Normal 1", 2D) = "white" {}
        _AnimTexPos2 ("Animation Texture Position 2", 2D) = "white" {}
        _AnimTexNorm2 ("Animation Texture Normal 2", 2D) = "white" {}
        _AnimTexPos3 ("Animation Texture Position 3", 2D) = "white" {}
        _AnimTexNorm3 ("Animation Texture Normal 3", 2D) = "white" {}

        // _TexelSize ("Texel Size (1/width)", float) = 0.002
        // _AnimParams ("Animation Params (x: index, y: time, z: animLengthInv, w: isLooping (0 or 1))", Vector) = (0, 0, 0, 0)
        // _RandomSeed ("Random Seed", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "./VertexAnimation.hlsl"

            struct Attributes
            {
                // no need to get position and normal from mesh
                // float4 positionOS : POSITION;
                // float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varying
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float fogCoord : TEXCOORD1;
                float3 ambient : TEXCOORD3;
                float3 diffuse : TEXCOORD4;
                float3 emission : TEXCOORD5;
                // UNITY_VERTEX_INPUT_INSTANCE_ID // Have this if you want to use UNITY_ACCESS_INSTANCED_PROP in fragment shader
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _Emission;
            CBUFFER_END

            Varying vert (Attributes IN, uint vertexID : SV_VertexID)
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                Varying OUT = (Varying)0;
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT); // Have this if you want to use UNITY_ACCESS_INSTANCED_PROP in fragment shader
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float3 positionOS;
                float3 normalOS;
                VERTEX_ANIMATION_OUTPUT(positionOS, normalOS, vertexID);

                // Lighting
                // VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.positionOS.xyz);
                // OUT.positionCS = mul(UNITY_MATRIX_VP, float4(positionWS, 1.0));
                OUT.positionCS = TransformObjectToHClip(positionOS);
                float3 normalWS = normalOS;

                float NoL = saturate(dot(normalWS, _MainLightPosition.xyz));
                float3 ambient = SampleSH(normalWS);
                float3 diffuse = NoL * _MainLightColor.rgb;

                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.ambient = ambient;
                OUT.diffuse = diffuse;
                OUT.emission = _Emission;
                OUT.fogCoord = ComputeFogFactor(OUT.positionCS.z);
                return OUT;
            }

            half4 frag (Varying IN) : SV_Target
            {
                // UNITY_SETUP_INSTANCE_ID(IN); // Have this if you want to use UNITY_ACCESS_INSTANCED_PROP in fragment shader
                half shadow = MainLightRealtimeShadow(IN.positionCS);
                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                float3 lighting = IN.diffuse * shadow + IN.ambient;
                half4 color = half4(albedo.rgb * lighting + IN.emission, albedo.a);
                color.rgb = MixFog(color.rgb, IN.fogCoord);
                return color;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #pragma vertex ShadowPassVertexInstancer
            #pragma fragment ShadowPassFragmentInstancer

            // #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"
            // #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "./VertexAnimation.hlsl"

            // Shadow Casting Light geometric parameters. These variables are used when applying the shadow Normal Bias and are set by UnityEngine.Rendering.Universal.ShadowUtils.SetupShadowCasterConstantBuffer in com.unity.render-pipelines.universal/Runtime/ShadowUtils.cs
            // For Directional lights, _LightDirection is used when applying shadow Normal Bias.
            // For Spot lights and Point lights, _LightPosition is used to compute the actual light direction because it is different at each shadow caster geometry vertex.
            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                // float4 positionOS   : POSITION;
                // float3 normalOS     : NORMAL;
                float2 texcoord     : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv           : TEXCOORD0;
                float4 positionCS   : SV_POSITION;
            };

            // SurfaceInput.hlsl contains this
            // TEXTURE2D(_BaseMap);
            // SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _Emission;
            CBUFFER_END

            Varyings ShadowPassVertexInstancer(Attributes input, uint instanceID : SV_InstanceID, uint vertexID : SV_VertexID)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                Varyings output = (Varyings)0;
                UNITY_TRANSFER_INSTANCE_ID(input, output); // Have this if you want to use UNITY_ACCESS_INSTANCED_PROP in fragment shader
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 positionOS;
                float3 normalOS;
                VERTEX_ANIMATION_OUTPUT(positionOS, normalOS, vertexID);

                // output.positionCS = GetShadowPositionHClip(input);
                float3 positionWS = TransformObjectToWorld(positionOS);
                float3 normalWS = TransformObjectToWorldNormal(normalOS);

                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                output.positionCS = positionCS;
                
                return output;
            }

            half4 ShadowPassFragmentInstancer(Varyings input) : SV_TARGET
            {
                // Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
                Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, 0.5);
                return 0;
            }
            ENDHLSL
        }
    }
}
