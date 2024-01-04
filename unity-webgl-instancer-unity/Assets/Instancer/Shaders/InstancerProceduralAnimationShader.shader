Shader "Instancer/InstancerProceduralAnimationShader"
{
    Properties
    {
        _BaseColor ("Base color", Color) = (1,1,1,1)
        _BaseMap ("Texture", 2D) = "white" {}

        _AnimTexPos ("Animation Texture Position", 2D) = "white" {}
        _AnimTexNorm ("Animation Texture Normal", 2D) = "white" {}
        _TexelSize ("Texel Size (1/width)", float) = 0.002
        _AnimLengthInv ("Animation Length (Seconds, Inversed)", Float) = 1.2
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

            struct Attributes
            {
                // no need to get position and normal from mesh
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varying
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float fogCoord : TEXCOORD1;
                float4 color : TEXCOORD2;
                float3 ambient : TEXCOORD3;
                float3 diffuse : TEXCOORD4;
                float3 emission : TEXCOORD5;
                // UNITY_VERTEX_INPUT_INSTANCE_ID // Have this if you want to use UNITY_ACCESS_INSTANCED_PROP in fragment shader
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            TEXTURE2D(_AnimTexPos);
            SAMPLER(sampler_AnimTexPos);
            TEXTURE2D(_AnimTexNorm);
            SAMPLER(sampler_AnimTexNorm);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                float4 _CustomColors[1000];
                float4 _CustomValues[1000];
                float3 _TargetPosition;

                float _TexelSize;
                float _AnimLengthInv;
            CBUFFER_END

            float3 Unity_RotateAboutAxis_Radians_float(float3 In, float3 Axis, float Rotation)
            {
                float s = sin(Rotation);
                float c = cos(Rotation);
                float one_minus_c = 1.0 - c;

                Axis = normalize(Axis);
                float3x3 rot_mat = 
                {   one_minus_c * Axis.x * Axis.x + c, one_minus_c * Axis.x * Axis.y - Axis.z * s, one_minus_c * Axis.z * Axis.x + Axis.y * s,
                    one_minus_c * Axis.x * Axis.y + Axis.z * s, one_minus_c * Axis.y * Axis.y + c, one_minus_c * Axis.y * Axis.z - Axis.x * s,
                    one_minus_c * Axis.z * Axis.x - Axis.y * s, one_minus_c * Axis.y * Axis.z + Axis.x * s, one_minus_c * Axis.z * Axis.z + c
                };
                return mul(rot_mat,  In);
            }

            Varying vert (Attributes IN, uint instanceID : SV_InstanceID, uint vertexID : SV_VertexID)
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                Varying OUT = (Varying)0;
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT); // Have this if you want to use UNITY_ACCESS_INSTANCED_PROP in fragment shader
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                // Custom Data
                float4 customColor = _CustomColors[instanceID];
                float4 customValue = _CustomValues[instanceID];

                // Get position and normal (Animation)
                float time = _Time.y * _AnimLengthInv;
                time += customColor.a * 172.827412; // randomize
                time = fmod(time, 1.0); // saturate(time) if it's not looping

                float2 uv;
                uv.x = (float(vertexID) + 0.5) * _TexelSize;
                uv.y = time;

                float3 positionOS = SAMPLE_TEXTURE2D_LOD(_AnimTexPos, sampler_AnimTexPos, uv, 0).xyz;
                float3 normalOS = SAMPLE_TEXTURE2D_LOD(_AnimTexNorm, sampler_AnimTexNorm, uv, 0).xyz;

                // Transform
                float3 instancePosition = float3(customValue.x, 0, customValue.y);
                float3 direction = normalize((_TargetPosition - instancePosition) * float3(1,0,1));

                float angle = -atan2(direction.z, direction.x) + 3.141592 * 0.5;
                positionOS = Unity_RotateAboutAxis_Radians_float(positionOS, float3(0, 1, 0), angle);
                positionOS += instancePosition;
                positionOS.y *= customValue.z;

                // Lighting
                // VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.positionOS.xyz);
                // OUT.positionCS = mul(UNITY_MATRIX_VP, float4(positionWS, 1.0));
                OUT.positionCS = TransformObjectToHClip(positionOS);
                float3 normalWS = normalOS;

                float NoL = saturate(dot(normalWS, _MainLightPosition.xyz));
                float3 ambient = SampleSH(normalWS) * 2; // TODO: enhance ambient temporarily
                float3 diffuse = NoL * _MainLightColor.rgb;

                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.ambient = ambient;
                OUT.diffuse = diffuse;
                OUT.color = customColor;
                OUT.emission = customValue.w;
                OUT.fogCoord = ComputeFogFactor(OUT.positionCS.z);
                return OUT;
            }

            half4 frag (Varying IN) : SV_Target
            {
                // UNITY_SETUP_INSTANCE_ID(IN); // Have this if you want to use UNITY_ACCESS_INSTANCED_PROP in fragment shader
                half shadow = MainLightRealtimeShadow(IN.positionCS);
                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor * IN.color;
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
            // Cull[_Cull]

            HLSLPROGRAM
            // #pragma exclude_renderers gles gles3 glcore
            // #pragma target 4.5

            // -------------------------------------
            // Material Keywords
            // #pragma shader_feature_local_fragment _ALPHATEST_ON
            // #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            //--------------------------------------
            // GPU Instancing
            // #pragma multi_compile_instancing
            // #pragma multi_compile _ DOTS_INSTANCING_ON

            // -------------------------------------
            // Universal Pipeline keywords

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #pragma vertex ShadowPassVertexInstancer
            #pragma fragment ShadowPassFragmentInstancer

            #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"

            TEXTURE2D(_AnimTexPos);
            SAMPLER(sampler_AnimTexPos);
            TEXTURE2D(_AnimTexNorm);
            SAMPLER(sampler_AnimTexNorm);

            half4 _CustomColors[1000];
            half4 _CustomValues[1000];
            float3 _TargetPosition;

            float _TexelSize;
            float _AnimLengthInv;

            float3 Unity_RotateAboutAxis_Radians_float(float3 In, float3 Axis, float Rotation)
            {
                float s = sin(Rotation);
                float c = cos(Rotation);
                float one_minus_c = 1.0 - c;

                Axis = normalize(Axis);
                float3x3 rot_mat = 
                {   one_minus_c * Axis.x * Axis.x + c, one_minus_c * Axis.x * Axis.y - Axis.z * s, one_minus_c * Axis.z * Axis.x + Axis.y * s,
                    one_minus_c * Axis.x * Axis.y + Axis.z * s, one_minus_c * Axis.y * Axis.y + c, one_minus_c * Axis.y * Axis.z - Axis.x * s,
                    one_minus_c * Axis.z * Axis.x - Axis.y * s, one_minus_c * Axis.y * Axis.z + Axis.x * s, one_minus_c * Axis.z * Axis.z + c
                };
                return mul(rot_mat,  In);
            }

            Varyings ShadowPassVertexInstancer(Attributes input, uint instanceID : SV_InstanceID, uint vertexID : SV_VertexID)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);

                // Get position and normal (Animation)
                float time = _Time.y * _AnimLengthInv;
                time = fmod(time, 1.0); // saturate(time) if it's not looping

                float2 uv;
                uv.x = (float(vertexID) + 0.5) * _TexelSize;
                uv.y = time;

                input.positionOS.xyz = SAMPLE_TEXTURE2D_LOD(_AnimTexPos, sampler_AnimTexPos, uv, 0).xyz;

                // Custom Data
                float4 customColor = _CustomColors[instanceID];
                float4 customValue = _CustomValues[instanceID];

                float3 instancePosition = float3(customValue.x, 0, customValue.y);
                float3 direction = normalize((_TargetPosition - instancePosition) * float3(1,0,1));

                float angle = -atan2(direction.z, direction.x) + 3.141592 * 0.5;
                input.positionOS.xyz = Unity_RotateAboutAxis_Radians_float(input.positionOS.xyz, float3(0, 1, 0), angle);
                input.positionOS.xyz += instancePosition;
                input.positionOS.y *= customValue.z;

                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                output.positionCS = GetShadowPositionHClip(input);
                return output;
            }

            half4 ShadowPassFragmentInstancer(Varyings input) : SV_TARGET
            {
                Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
                return 0;
            }
            ENDHLSL
        }
    }
}
