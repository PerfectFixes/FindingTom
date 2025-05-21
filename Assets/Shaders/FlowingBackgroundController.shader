Shader "Custom/BackgroundShader"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.1, 0.1, 0.1, 1)
        _YellowTint ("Yellow Tint", Color) = (1.0, 1.0, 0.0, 1)
        _Speed ("Animation Speed", Range(0, 1)) = 0.05
        _NoiseScale ("Noise Scale", Range(0.1, 5)) = 3.0
        _NoiseSpeed ("Noise Speed", Range(0, 2)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            float4 _BaseColor;
            float4 _YellowTint;
            float _Speed;
            float _NoiseScale;
            float _NoiseSpeed;
            
            float random(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            float noise(float2 uv)
            {
                float2 pos = uv * _NoiseScale;
                return random(floor(pos)) * 0.3 + random(ceil(pos)) * 0.3;
            }
            
            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                float t = abs(sin(_Time.y * _Speed));
                float n = noise(i.uv + _Time.y * _NoiseSpeed) * 0.05;
                float3 backgroundColor = _BaseColor.rgb + _YellowTint.rgb * (0.2 + n);
                return float4(backgroundColor, 1.0);
            }
            ENDCG
        }
    }
}
