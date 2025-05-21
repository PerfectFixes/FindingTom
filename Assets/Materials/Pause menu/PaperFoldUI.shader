Shader "UI/PaperRainbowLines"
{
    Properties
    {
        _PaperColor("Paper Color", Color) = (1, 0.992, 0.878, 1)
        _LineWidth("Line Width", Range(0.001, 0.1)) = 0.02
        _LineStrength("Line Strength", Range(0, 1)) = 0.5
        _Speed("Line Change Speed", Float) = 2.0
        _UnscaledTime("Unscaled Time", Float) = 0
        _LineCount("Number of Lines", Range(1, 10)) = 5
        _PaperNoiseScale("Paper Grain Scale", Range(10, 200)) = 80
        _PaperNoiseStrength("Paper Grain Strength", Range(0, 1)) = 0.05

        // Rainbow colors to choose from
        _RainbowColors0("Red", Color) = (1, 0, 0, 1)
        _RainbowColors1("Orange", Color) = (1, 0.5, 0, 1)
        _RainbowColors2("Yellow", Color) = (1, 1, 0, 1)
        _RainbowColors3("Green", Color) = (0, 1, 0, 1)
        _RainbowColors4("Blue", Color) = (0, 0, 1, 1)
        _RainbowColors5("Indigo", Color) = (0.29, 0, 0.51, 1)
        _RainbowColors6("Violet", Color) = (0.56, 0, 1, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "Unlit"
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _PaperColor;
            float _LineWidth;
            float _LineStrength;
            float _Speed;
            float _UnscaledTime;
            float _LineCount;
            float _PaperNoiseScale;
            float _PaperNoiseStrength;

            float4 _RainbowColors0;
            float4 _RainbowColors1;
            float4 _RainbowColors2;
            float4 _RainbowColors3;
            float4 _RainbowColors4;
            float4 _RainbowColors5;
            float4 _RainbowColors6;

            float2 rand2(float seed)
            {
                return float2(
                    frac(sin(seed * 12.9898) * 43758.5453),
                    frac(cos(seed * 78.233) * 12345.6789)
                );
            }

            float rand(float2 co)
            {
                return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
            }

            float PaperGrain(float2 uv)
            {
                float2 grainUV = uv * _PaperNoiseScale;
                float grain = sin(grainUV.x) * cos(grainUV.y);
                return grain * _PaperNoiseStrength;
            }

            float4 getRandomRainbowColor(float seed)
            {
                int index = (int)(frac(sin(seed) * 43758.5453) * 7);
                if (index == 0) return _RainbowColors0;
                if (index == 1) return _RainbowColors1;
                if (index == 2) return _RainbowColors2;
                if (index == 3) return _RainbowColors3;
                if (index == 4) return _RainbowColors4;
                if (index == 5) return _RainbowColors5;
                return _RainbowColors6;
            }

            float LineMask(float2 uv, float2 origin, float2 dir, float seed, float width)
            {
                float2 toPoint = uv - origin;

                // Project to line
                float proj = dot(toPoint, dir);
                float2 closestPoint = origin + dir * proj;

                // Wavy curve
                float wave = sin(proj * 40.0 + seed * 10.0) * 0.02; // child-like wobble
                float2 normal = float2(-dir.y, dir.x);
                closestPoint += normal * wave;

                // Add noise jitter
                float jitter = (rand(uv * 100.0 + seed * 5.0) - 0.5) * 0.01;
                closestPoint += normal * jitter;

                // Random cutout effect (skips part of the line)
                float hash = rand(float2(proj, seed));
                if (hash < 0.2 || hash > 0.8)
                    return 0.0;

                float dist = length(uv - closestPoint);
                return smoothstep(width, 0.0, dist);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.position = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 resultColor = _PaperColor.rgb;
                float t = floor(_UnscaledTime / _Speed);

                for (int n = 0; n < (int)_LineCount; n++)
                {
                    float seed = t + n * 10.0;
                    float2 origin = rand2(seed);
                    float angle = frac(sin(seed * 91.931) * 34567.123) * 6.2831853;
                    float2 dir = normalize(float2(cos(angle), sin(angle)));

                    float mask = LineMask(i.uv, origin, dir, seed, _LineWidth);
                    float4 lineColor = getRandomRainbowColor(seed);
                    resultColor = lerp(resultColor, lineColor.rgb, mask * _LineStrength);
                }

                resultColor += PaperGrain(i.uv);
                return float4(resultColor, _PaperColor.a);
            }

            ENDHLSL
        }
    }
}
