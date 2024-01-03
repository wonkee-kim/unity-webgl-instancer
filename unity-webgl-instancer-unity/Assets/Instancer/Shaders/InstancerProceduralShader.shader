Shader "Instancer/InstancerProceduralShader"
{
    Properties
    {
        _BaseColor ("Base color", Color) = (1,1,1,1)
        _BaseMap ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                uint instanceID : SV_InstanceID;
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

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _CustomColors[512];
                half4 _CustomValues[512];
            CBUFFER_END

            // uint instanceID : SV_InstanceID;

            // Varying vert (Attributes IN, uint instanceID : SV_InstanceID)
            Varying vert (Attributes IN)
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                Varying OUT = (Varying)0;
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT); // Have this if you want to use UNITY_ACCESS_INSTANCED_PROP in fragment shader
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float4 customColor = _CustomColors[IN.instanceID];
                float4 customValue = _CustomValues[IN.instanceID];
                // float4 customColor = _CustomColors[instanceID];
                // float4 customValue = _CustomValues[instanceID];

                float3 positionOS = IN.positionOS.xyz + float3(customValue.x, 0, customValue.y);
                positionOS.y *= customValue.z;

                // VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.positionOS.xyz);
                // OUT.positionCS = mul(UNITY_MATRIX_VP, float4(positionWS, 1.0));
                OUT.positionCS = TransformObjectToHClip(positionOS);
                float3 normalWS = IN.normalOS;

                float NoL = saturate(dot(normalWS, _MainLightPosition.xyz));
                float3 ambient = SampleSH(normalWS);
                float3 diffuse = NoL * _MainLightColor.rgb;

                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.ambient = ambient;
                OUT.diffuse = diffuse;
                OUT.color = customColor;
                // OUT.emission = customValue.w;
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
    }
}
