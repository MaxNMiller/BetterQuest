Shader "Spaa/Toon"
{
    Properties
    {
        [MainColor] _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" {}
        _TextureTint ("Texture Tint (0 = raw, 1 = luminance x Base Color)", Range(0, 1)) = 0
        _ShadowTint ("Shadow Tint", Color) = (0.55, 0.5, 0.75, 1)
        _RimColor ("Rim Color", Color) = (0.106, 0.106, 0.227, 1)
        _RimPower ("Rim Power", Range(0.5, 8)) = 3
        _RimStrength ("Rim Strength", Range(0, 2)) = 0.9
        _FlashColor ("Flash Color", Color) = (1, 1, 1, 1)
        _Flash ("Flash", Range(0, 1)) = 0
        _Dissolve ("Dissolve", Range(0, 1)) = 0
        _DissolveEdgeColor ("Dissolve Edge Color", Color) = (1, 0.9, 0.5, 1)
        _DissolveEdgeWidth ("Dissolve Edge Width", Range(0, 0.3)) = 0.08
        _NoiseScale ("Noise Scale", Float) = 4
        [HideInInspector] _SrcBlend ("Src Blend", Float) = 1
        [HideInInspector] _DstBlend ("Dst Blend", Float) = 0
        [HideInInspector] _ZWrite ("ZWrite", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            float4 _BaseMap_ST;
            half _TextureTint;
            half4 _ShadowTint;
            half4 _RimColor;
            half _RimPower;
            half _RimStrength;
            half4 _FlashColor;
            half _Flash;
            half _Dissolve;
            half4 _DissolveEdgeColor;
            half _DissolveEdgeWidth;
            float _NoiseScale;
        CBUFFER_END

        float Hash31(float3 p)
        {
            p = frac(p * 0.1031);
            p += dot(p, p.yzx + 33.33);
            return frac((p.x + p.y) * p.z);
        }

        float ValueNoise(float3 p)
        {
            float3 i = floor(p);
            float3 f = frac(p);
            f = f * f * (3.0 - 2.0 * f);
            float n000 = Hash31(i);
            float n100 = Hash31(i + float3(1, 0, 0));
            float n010 = Hash31(i + float3(0, 1, 0));
            float n110 = Hash31(i + float3(1, 1, 0));
            float n001 = Hash31(i + float3(0, 0, 1));
            float n101 = Hash31(i + float3(1, 0, 1));
            float n011 = Hash31(i + float3(0, 1, 1));
            float n111 = Hash31(i + float3(1, 1, 1));
            float x00 = lerp(n000, n100, f.x);
            float x10 = lerp(n010, n110, f.x);
            float x01 = lerp(n001, n101, f.x);
            float x11 = lerp(n011, n111, f.x);
            return lerp(lerp(x00, x10, f.y), lerp(x01, x11, f.y), f.z);
        }

        float DissolveClip(float3 positionOS)
        {
            float noise = ValueNoise(positionOS * _NoiseScale) * 0.7 + ValueNoise(positionOS * _NoiseScale * 2.3) * 0.3;
            float d = noise - (_Dissolve * 1.1 - 0.05);
            clip(d);
            return d;
        }
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        ENDHLSL

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionOS : TEXCOORD2;
                float fogFactor : TEXCOORD3;
                float2 uv : TEXCOORD4;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positions.positionCS;
                output.positionWS = positions.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionOS = input.positionOS.xyz;
                output.fogFactor = ComputeFogFactor(positions.positionCS.z);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float dissolveDistance = DissolveClip(input.positionOS);

                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(GetWorldSpaceViewDir(input.positionWS));
                Light mainLight = GetMainLight();

                half4 texel = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half luminance = dot(texel.rgb, half3(0.299, 0.587, 0.114));
                half3 albedo = lerp(texel.rgb, (luminance * 1.5).xxx, _TextureTint) * _BaseColor.rgb;

                half ndotl = dot(normalWS, mainLight.direction);
                half band = smoothstep(-0.05, 0.05, ndotl);
                half3 lit = lerp(albedo * _ShadowTint.rgb, albedo, band) * mainLight.color;
                lit += albedo * SampleSH(normalWS) * 0.35;

                half rim = saturate(pow(1.0 - saturate(dot(normalWS, viewDirWS)), _RimPower) * _RimStrength);
                lit = lerp(lit, _RimColor.rgb, rim);

                lit = lerp(lit, _FlashColor.rgb, _Flash);

                half edge = (_Dissolve > 0.001) ? 1.0 - smoothstep(0.0, _DissolveEdgeWidth, dissolveDistance) : 0.0;
                lit = lerp(lit, _DissolveEdgeColor.rgb * 2.0, edge);

                lit = MixFog(lit, input.fogFactor);
                return half4(lit, texel.a * _BaseColor.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionOS : TEXCOORD0; };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionOS = input.positionOS.xyz;
                return output;
            }

            half Frag(Varyings input) : SV_Target
            {
                DissolveClip(input.positionOS);
                return input.positionCS.z;
            }
            ENDHLSL
        }
    }
}
