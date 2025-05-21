Shader "Custom/PortalDistortedGlowURP"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (0.7, 0.9, 1, 0.25)
        _GlowColor("Glow Color", Color) = (0, 1, 1, 1)
        _GlowRadius("Glow Radius", Float) = 0.0
        _GlowWidth("Glow Width", Float) = 0.1
        _TouchPosition("Touch Position", Vector) = (0.5, 0.5, 0, 0)
        _DistortionStrength("Distortion Strength", Float) = 0.03
        _GlowDistortionStrength("Glow Distortion Strength", Float) = 0.06
        _RippleFrequency("Ripple Frequency", Float) = 30.0
        _RippleSpeed("Ripple Speed", Float) = 2.0
        _MainTex("Main Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _CAMERA_OPAQUE_TEXTURE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _CameraOpaqueTexture;
            float4 _BaseColor;
            float4 _GlowColor;
            float _GlowRadius;
            float _GlowWidth;
            float2 _TouchPosition;
            float _DistortionStrength;
            float _GlowDistortionStrength;
            float _RippleFrequency;
            float _RippleSpeed;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float time = _Time.y;
                float2 uv = IN.uv;
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;

                // --- Ripple effect from center (always active)
                float2 center = float2(0.5, 0.5);
                float distCenter = distance(uv, center);
                float ripple = sin(distCenter * _RippleFrequency - time * _RippleSpeed);
                float attenuation = 1.0 - smoothstep(0.0, 0.5, distCenter);
                float2 delta = uv - center;
                float len = length(delta);
                float2 direction = len > 0.0001 ? delta / len : float2(0, 0);

                float2 rippleOffset = direction * ripple * _DistortionStrength * attenuation;

                // --- Glow ripple distortion (only during glow)
                float distToTouch = distance(uv, _TouchPosition);
                float glowRipple = exp(-pow((distToTouch - _GlowRadius) * 20, 2));
                float2 glowDelta = uv - _TouchPosition;
                float glowLen = length(glowDelta);
                float2 glowDir = glowLen > 0.0001 ? glowDelta / glowLen : float2(0, 0);

                float2 glowOffset = glowDir * glowRipple * _GlowDistortionStrength;

                float2 totalOffset = rippleOffset + glowOffset;
                float2 distortedUV = screenUV + totalOffset;

                // --- Final color
                float4 distortedBG = tex2D(_CameraOpaqueTexture, distortedUV);
                float4 baseTex = tex2D(_MainTex, uv);

                float glowMask = smoothstep(_GlowRadius, _GlowRadius - _GlowWidth, distToTouch);
                float4 glow = _GlowColor * glowMask;

                float4 finalColor = distortedBG * _BaseColor;
                finalColor.rgb += glow.rgb; // הזוהר כאילו מקרין אור

                finalColor.a = saturate(_BaseColor.a + glow.a);

                return finalColor;
            }
            ENDHLSL
        }
    }
}
