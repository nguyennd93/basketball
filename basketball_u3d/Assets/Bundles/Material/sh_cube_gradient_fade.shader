Shader "Basketball/URP/Cube Gradient Fade"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.93, 0.73, 0.96, 1.0)
        _BottomColor ("Bottom Color", Color) = (0.64, 0.45, 0.92, 1.0)
        _BottomAlpha ("Bottom Alpha", Range(0, 1)) = 0.0
        _HeightMin ("Height Min", Float) = -0.5
        _HeightMax ("Height Max", Float) = 0.5
        _GradientPower ("Gradient Power", Range(0.1, 4.0)) = 1.0
        _AmbientStrength ("Ambient Strength", Range(0, 1)) = 0.45
        _ShadowStrength ("Shadow Strength", Range(0, 1)) = 0.85
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "Forward"

            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float localY : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _TopColor;
                half4 _BottomColor;
                half _BottomAlpha;
                float _HeightMin;
                float _HeightMax;
                half _GradientPower;
                half _AmbientStrength;
                half _ShadowStrength;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
                output.positionHCS = positionInputs.positionCS;
                output.localY = input.positionOS.y;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float heightRange = max(_HeightMax - _HeightMin, 0.0001);
                half gradient = saturate((input.localY - _HeightMin) / heightRange);
                gradient = pow(gradient, _GradientPower);

                half3 rgb = lerp(_BottomColor.rgb, _TopColor.rgb, gradient);
                half alpha = lerp(_BottomColor.a * _BottomAlpha, _TopColor.a, gradient);

                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half3 normalWS = normalize(input.normalWS);
                half lambert = saturate(dot(normalWS, mainLight.direction));
                half lighting = lerp(_AmbientStrength, 1.0h, lambert);
                half shadowAttenuation = lerp(1.0h, mainLight.shadowAttenuation, _ShadowStrength);
                rgb *= lighting * shadowAttenuation * mainLight.color;
                return half4(rgb, alpha);
            }
            ENDHLSL
        }
    }
}
