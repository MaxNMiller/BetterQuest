Shader "Spaa/Ground"
{
    Properties
    {
        [MainColor] _BaseColor ("Base Color", Color) = (0.6, 0.6, 0.6, 1)
        _RingColor ("Ring Color", Color) = (1, 1, 1, 1)
        _RingFrequency ("Ring Frequency", Float) = 0.6
        _RingSpeed ("Ring Speed", Float) = 0.35
        _RingWidth ("Ring Width", Range(0.01, 0.5)) = 0.12
        _RingStrength ("Ring Strength", Range(0, 1)) = 0.55
        _RingFadeRadius ("Ring Fade Radius", Float) = 9
        _Center ("Center (XZ world)", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _RingColor;
                float _RingFrequency;
                float _RingSpeed;
                half _RingWidth;
                half _RingStrength;
                float _RingFadeRadius;
                float4 _Center;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float fogFactor : TEXCOORD2;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positions.positionCS;
                output.positionWS = positions.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.fogFactor = ComputeFogFactor(positions.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                Light mainLight = GetMainLight();
                float3 normalWS = normalize(input.normalWS);
                half ndotl = saturate(dot(normalWS, mainLight.direction)) * 0.6 + 0.4;
                half3 color = _BaseColor.rgb * ndotl * mainLight.color;

                float dist = length(input.positionWS.xz - _Center.xy);
                float wave = frac(dist * _RingFrequency - _Time.y * _RingSpeed);
                half ring = 1.0 - smoothstep(0.0, _RingWidth, abs(wave - 0.5));
                half fade = 1.0 - saturate(dist / _RingFadeRadius);
                color = lerp(color, _RingColor.rgb, ring * fade * _RingStrength);

                color = MixFog(color, input.fogFactor);
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
