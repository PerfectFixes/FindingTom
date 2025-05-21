Shader "Custom/DiscoBall"
{
    Properties
    {
        _Color ("Tile Color", Color) = (1,1,1,1)
        _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        _TileSize ("Tile Size", Float) = 20
        _GlintStrength ("Glint Strength", Float) = 1
        _GlintSpeed ("Glint Speed", Float) = 2
        _MainTex ("Base Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            float4 _Color;
            float4 _EmissionColor;
            float _TileSize;
            float _GlintStrength;
            float _GlintSpeed;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            float rand(float2 co)
            {
                return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 tileUV = floor(IN.worldPos.xy * _TileSize);
                float noise = rand(tileUV + _Time.x * _GlintSpeed);

                float glint = saturate(sin(_Time.x * _GlintSpeed + noise * 6.28) * _GlintStrength);

                float3 baseColor = _Color.rgb;
                float3 emission = _EmissionColor.rgb * glint;

                return float4(baseColor + emission, 1);
            }

            ENDHLSL
        }
    }

    FallBack "Diffuse"
}
