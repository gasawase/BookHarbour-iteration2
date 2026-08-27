Shader "Custom/WallWithWindowCutout"
{
    Properties
    {
        _BaseMap ("Base Texture", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1,1,1,1)

        _WindowCenter ("Window Center (World)", Vector) = (0,0,0,0)
        _WindowSize ("Window Size", Vector) = (1,1,0,0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="AlphaTest" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            float4 _Color;
            float3 _WindowCenter;
            float2 _WindowSize;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);

                OUT.positionHCS = posInputs.positionCS;
                OUT.worldPos = posInputs.positionWS;
                OUT.uv = IN.uv;

                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float3 worldPos = IN.worldPos;

                float2 halfSize = _WindowSize * 0.5;

                float2 localPos = worldPos.xy - _WindowCenter.xy;

                // Check if inside rectangle
                if (abs(localPos.x) < halfSize.x && abs(localPos.y) < halfSize.y)
                {
                    discard; // this creates the "hole"
                }

                float4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);

                return tex * _Color;
            }

            ENDHLSL
        }
    }
}