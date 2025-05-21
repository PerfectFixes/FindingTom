Shader "UI/BorderRunnerWithTexture"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,1,1,1)
        _BorderColor ("Tint Color", Color) = (1,1,1,1)
        _Thickness ("Border Thickness", Float) = 0.02
        _LineLength ("Line Length", Float) = 0.15
        _Speed ("Speed", Float) = 1
        _MainTex ("Main Texture", 2D) = "white" {}
        _LineTex ("Line Texture", 2D) = "white" {} // NEW!
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _LineTex;
            float4 _MainTex_ST;

            fixed4 _Color;
            fixed4 _BorderColor;
            float _Thickness;
            float _LineLength;
            float _Speed;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float borderPath(float2 uv)
            {
                if (uv.y <= _Thickness) // Bottom
                    return uv.x * 0.25;
                else if (uv.x >= 1.0 - _Thickness) // Right
                    return 0.25 + uv.y * 0.25;
                else if (uv.y >= 1.0 - _Thickness) // Top
                    return 0.75 - uv.x * 0.25;
                else if (uv.x <= _Thickness) // Left
                    return 0.75 + (1.0 - uv.y) * 0.25;
                else
                    return -1.0;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float distToBorder = min(min(uv.x, 1.0 - uv.x), min(uv.y, 1.0 - uv.y));
                float path = borderPath(uv);
                float time = frac(_Time.y * _Speed);

                float border = smoothstep(0.0, _Thickness, _Thickness - distToBorder);

                float glow = 0.0;
                float textureSample = 0.0;

                if (path >= 0.0)
                {
                    float d = abs(path - time);
                    d = min(d, 1.0 - d); // wrap around

                    if (d < _LineLength)
                    {
                        float pathUV = d / _LineLength;
                        textureSample = tex2D(_LineTex, float2(pathUV, 0.5)).r;
                        glow = textureSample;

                        // Boost tip if near start
                        float tipFade = smoothstep(0.0, 0.02, _LineLength - d);
                        glow += tipFade * 1.2;
                    }
                }

                fixed4 baseCol = tex2D(_MainTex, uv) * _Color;
                fixed4 lineCol = _BorderColor * glow;

                return lerp(baseCol, lineCol, glow * border);
            }
            ENDCG
        }
    }
}
