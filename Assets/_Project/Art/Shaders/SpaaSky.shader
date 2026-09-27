Shader "Spaa/SkyGradient"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.35, 0.55, 0.95, 1)
        _HorizonColor ("Horizon Color", Color) = (0.95, 0.85, 0.95, 1)
        _BottomColor ("Bottom Color", Color) = (0.55, 0.5, 0.65, 1)
        _Exponent ("Exponent", Range(0.1, 4)) = 0.8
    }

    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" "RenderPipeline" = "UniversalPipeline" }
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _TopColor;
                half4 _HorizonColor;
                half4 _BottomColor;
                half _Exponent;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 direction : TEXCOORD0; };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.direction = input.positionOS.xyz;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float y = normalize(input.direction).y;
                half up = pow(saturate(y), _Exponent);
                half down = pow(saturate(-y), _Exponent);
                half3 color = lerp(_HorizonColor.rgb, _TopColor.rgb, up);
                color = lerp(color, _BottomColor.rgb, down);
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
