Shader "Spaa/Particle"
{
    Properties
    {
        [MainColor] _BaseColor ("Tint", Color) = (1, 1, 1, 1)
        _Shape ("Shape (0 orb,1 ring,2 star,3 heart,4 square)", Float) = 0
        _CoreGlow ("Core Glow", Range(0, 1)) = 0.35
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _Shape;
                half _CoreGlow;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; half4 color : COLOR; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; half4 color : COLOR; float2 uv : TEXCOORD0; };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color * _BaseColor;
                output.uv = input.uv;
                return output;
            }

            float Dot2(float2 v) { return dot(v, v); }

            float HeartSdf(float2 p)
            {
                p.x = abs(p.x);
                if (p.y + p.x > 1.0)
                {
                    return sqrt(Dot2(p - float2(0.25, 0.75))) - sqrt(2.0) / 4.0;
                }
                return sqrt(min(Dot2(p - float2(0.0, 1.0)), Dot2(p - 0.5 * max(p.x + p.y, 0.0)))) * sign(p.x - p.y);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 p = input.uv * 2.0 - 1.0;
                float r = length(p);
                half alpha;
                half core;

                if (_Shape < 0.5)
                {
                    alpha = 1.0 - smoothstep(0.35, 1.0, r);
                    core = 1.0 - smoothstep(0.0, 0.45, r);
                }
                else if (_Shape < 1.5)
                {
                    half rim = 1.0 - smoothstep(0.04, 0.16, abs(r - 0.78));
                    half fill = (1.0 - smoothstep(0.7, 0.8, r)) * 0.18;
                    half shine = 1.0 - smoothstep(0.0, 0.18, length(p - float2(-0.35, 0.35)));
                    alpha = saturate(rim + fill + shine);
                    core = shine;
                }
                else if (_Shape < 2.5)
                {
                    float2 a = abs(p);
                    float star = sqrt(a.x) + sqrt(a.y);
                    alpha = 1.0 - smoothstep(0.75, 1.0, star);
                    core = 1.0 - smoothstep(0.0, 0.3, r);
                }
                else if (_Shape < 3.5)
                {
                    float d = HeartSdf(p * 0.75 + float2(0.0, 0.55));
                    alpha = 1.0 - smoothstep(-0.02, 0.03, d);
                    core = smoothstep(-0.25, 0.0, d) * 0.5;
                }
                else
                {
                    float2 a = abs(p);
                    float box = max(a.x, a.y);
                    alpha = 1.0 - smoothstep(0.75, 0.9, box);
                    core = (1.0 - smoothstep(0.55, 0.75, box)) * 0.2;
                }

                half3 color = lerp(input.color.rgb, half3(1, 1, 1), core * _CoreGlow);
                return half4(color, alpha * input.color.a);
            }
            ENDHLSL
        }
    }
}
