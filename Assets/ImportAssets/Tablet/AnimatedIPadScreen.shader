Shader "Custom/AnimatedIPadScreen"
{
    Properties
    {
        _MainColor ("Screen Color", Color) = (0.58, 0.93, 1, 1) // 94EDFF in RGB
        _EmissionIntensity ("Emission Intensity", Range(0, 3)) = 1.5
        _WaveSpeed ("Wave Speed", Range(0, 5)) = 1.0
        _WaveAmplitude ("Wave Amplitude", Range(0, 0.1)) = 0.02
        _WaveFrequency ("Wave Frequency", Range(0, 50)) = 10.0
        _RippleStrength ("Ripple Strength", Range(0, 1)) = 0.1
        _GlowEdge ("Edge Glow", Range(0, 1)) = 0.3
        _CenterX ("Center X Offset", Range(0, 1)) = 0.5
        _CenterY ("Center Y Offset", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 200
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        
        CBUFFER_START(UnityPerMaterial)
        float4 _MainColor;
        float _EmissionIntensity;
        float _WaveSpeed;
        float _WaveAmplitude;
        float _WaveFrequency;
        float _RippleStrength;
        float _GlowEdge;
        float _CenterX;
        float _CenterY;
        CBUFFER_END
        ENDHLSL
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv           : TEXCOORD0;
                float3 positionWS   : TEXCOORD1;
                float3 normalWS     : TEXCOORD2;
                float3 viewDirWS    : TEXCOORD3;
                float4 positionCS   : SV_POSITION;
            };
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.uv = input.uv;
                output.normalWS = normalInput.normalWS;
                output.positionWS = vertexInput.positionWS;
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                output.positionCS = vertexInput.positionCS;
                
                return output;
            }
            
            float4 frag(Varyings input) : SV_Target
            {
                // Create time-based animation for the waves
                float time = _Time.y * _WaveSpeed;
                
                // Generate wave pattern with adjustable center point
                float2 uv = input.uv;
                float2 center = float2(_CenterX, _CenterY);
                float dist = distance(uv, center);
                
                // Multiple concentric waves emanating from the adjustable center
                float wave1 = sin(dist * _WaveFrequency - time) * _WaveAmplitude;
                float wave2 = sin(dist * _WaveFrequency * 1.5 - time * 1.3) * _WaveAmplitude * 0.7;
                
                // Create circular ripple effect from the adjustable center
                float ripple = sin(dist * _WaveFrequency * 3.0 - time * 2.0) * _RippleStrength * (1.0 - dist);
                
                // Combine waves
                float waveCombined = wave1 + wave2 + ripple;
                
                // Edge glow effect based on view angle
                float3 viewDirWS = normalize(input.viewDirWS);
                float3 normalWS = normalize(input.normalWS);
                float edgeFactor = 1.0 - saturate(dot(viewDirWS, normalWS));
                float edgeGlow = pow(edgeFactor, 2.0) * _GlowEdge;
                
                // Set the main color
                float3 albedo = _MainColor.rgb;
                
                // Apply the wave animation to the emission
                float3 emission = albedo * (_EmissionIntensity + waveCombined + edgeGlow);
                
                // Basic lighting
                InputData lightingInput = (InputData)0;
                lightingInput.positionWS = input.positionWS;
                lightingInput.normalWS = normalWS;
                lightingInput.viewDirectionWS = viewDirWS;
                lightingInput.shadowCoord = float4(0, 0, 0, 0);
                
                #if defined(MAIN_LIGHT_CALCULATE_SHADOWS)
                    lightingInput.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                #endif
                
                lightingInput.fogCoord = 0;
                lightingInput.vertexLighting = float3(0, 0, 0);
                lightingInput.bakedGI = float3(0, 0, 0);
                lightingInput.normalizedScreenSpaceUV = float2(0, 0);
                lightingInput.shadowMask = float4(1, 1, 1, 1);
                
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo;
                surfaceData.metallic = 0;
                surfaceData.specular = float3(0, 0, 0);
                surfaceData.smoothness = 0.5;
                surfaceData.occlusion = 1.0;
                surfaceData.emission = emission;
                surfaceData.alpha = 1.0;
                surfaceData.clearCoatMask = 0;
                surfaceData.clearCoatSmoothness = 0;
                
                float4 color = UniversalFragmentPBR(lightingInput, surfaceData);
                return color;
            }
            ENDHLSL
        }
        
        // Shadow casting support
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
    FallBack "Universal Render Pipeline/Lit"
}