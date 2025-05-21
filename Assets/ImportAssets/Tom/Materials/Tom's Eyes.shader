Shader "Custom/MovingEyeWithBlink"
{
    Properties
    {
        _GreenColor ("Green Color", Color) = (0,1,0,1)
        _BlackColor ("Black Color", Color) = (0,0,0,1)
        _WhiteColor ("White Color", Color) = (1,1,1,1)
        _BlinkColor ("Blink Color", Color) = (0,0,0,1)
        _MoveSpeed ("Move Speed", Float) = 0.5
        _MoveAmount ("Move Amount", Float) = 0.1
        _Blink ("Enable Blink", Range(0,1)) = 1
        _BlinkInterval ("Blink Interval (Seconds)", Float) = 6.0
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

            float4 _GreenColor;
            float4 _BlackColor;
            float4 _WhiteColor;
            float4 _BlinkColor;
            float _MoveSpeed;
            float _MoveAmount;
            float _Blink;
            float _BlinkInterval;

            float random(float seed)
            {
                return frac(sin(seed * 43758.5453) * 10000.0);
            }

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * 2 - 1;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float time = _Time.y;

                // === תנועת האישון הרנדומלית ===
                float timeStep = floor(time * _MoveSpeed);
                float timeFrac = frac(time * 0.095);
                float movementTrigger = step(0.499, timeFrac);

                float offsetX = (random(timeStep) - 0.5) * _MoveAmount * 2 * movementTrigger;
                float offsetY = (random(timeStep + 10.0) - 0.5) * _MoveAmount * 2 * movementTrigger;

                // === אפקט מצמוץ ===
                float blinkCycle = fmod(time, _BlinkInterval); // שליטה בתדירות מצמוץ
                float blinkPhase = smoothstep(0.0, 0.25, blinkCycle) - smoothstep(0.25, 0.5, blinkCycle);
                blinkPhase -= (smoothstep(0.5, 0.75, blinkCycle) - smoothstep(0.75, 1.0, blinkCycle));
                blinkPhase *= _Blink; // מאפשר להדליק/לכבות את המצמוץ

                float blinkY = lerp(1.0, -1.0, blinkPhase);

                // === ציור העין ===
                float greenRadius = 0.8;
                float blackRadius = greenRadius * 0.35;
                float whiteRadius = blackRadius * 0.3;

                float3 color = float3(1,1,1); // רקע לבן

                if (length(uv) < greenRadius)
                    color = _GreenColor.rgb;

                if (length(uv) < blackRadius)
                    color = _BlackColor.rgb;

                if (length(uv - float2(offsetX, offsetY)) < whiteRadius)
                    color = _WhiteColor.rgb;

                if (uv.x > blinkY)
                    color = _BlinkColor.rgb;

                return fixed4(color, 1.0);
            }
            ENDCG
        }
    }
}
