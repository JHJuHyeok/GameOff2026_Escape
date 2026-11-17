Shader "GameOff2026/SpriteOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [HideInInspector] _Color ("Tint", Color) = (1, 1, 1, 1)

        [Toggle] _OutlineEnabled ("Outline Enabled", Float) = 0
        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineWidth ("Outline Width (Texture Pixels)", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "CanUseSpriteAtlas" = "False"
        }

        Cull Off
        ZWrite Off

        // 프래그먼트에서 RGB에 알파를 곱해서 출력합니다.
        Blend One OneMinusSrcAlpha

        Pass
        {
            Name "SpriteOutline"

            Tags
            {
                "LightMode" = "Universal2D"
            }

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma target 3.5

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _OutlineColor;
                float _OutlineEnabled;
                float _OutlineWidth;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                SetUpSpriteInstanceProperties();

                float3 positionOS = UnityFlipSprite(input.positionOS,unity_SpriteProps.xy);

                output.positionCS = TransformObjectToHClip(positionOS);
                output.uv = input.uv;
                output.color = input.color * _Color * unity_SpriteColor;

                return output;
            }

            half SampleAlpha(float2 uv)
            {
                // 텍스처 바깥을 투명하게 취급해 가장자리 반복을 방지합니다.
                if (any(uv < 0.0) || any(uv > 1.0))
                {
                    return 0.0h;
                }

                return SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv).a;
            }

            half GetNeighbourAlpha(float2 uv, float2 offset)
            {
                half alpha = 0.0h;

                // 상하좌우
                alpha = max(alpha, SampleAlpha(uv + float2( offset.x, 0)));
                alpha = max(alpha, SampleAlpha(uv + float2(-offset.x, 0)));
                alpha = max(alpha, SampleAlpha(uv + float2(0,  offset.y)));
                alpha = max(alpha, SampleAlpha(uv + float2(0, -offset.y)));

                // 대각선
                alpha = max(alpha, SampleAlpha(uv + float2( offset.x,  offset.y)));
                alpha = max(alpha, SampleAlpha(uv + float2(-offset.x,  offset.y)));
                alpha = max(alpha, SampleAlpha(uv + float2( offset.x, -offset.y)));
                alpha = max(alpha, SampleAlpha(uv + float2(-offset.x, -offset.y)));

                return alpha;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 sprite = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv) * input.color;
                half4 result;
                result.rgb = sprite.rgb * sprite.a;
                result.a = sprite.a;

                if (_OutlineEnabled <= 0.0 || _OutlineWidth <= 0.0)
                {
                    return result;
                }

                uint textureWidth;
                uint textureHeight;

                _MainTex.GetDimensions(textureWidth, textureHeight);

                float2 texelSize = 1.0 / float2(textureWidth, textureHeight);
                float2 offset = texelSize * _OutlineWidth;

                half neighbourAlpha = GetNeighbourAlpha(input.uv, offset);
                half outlineAlpha = neighbourAlpha * _OutlineColor.a * input.color.a * saturate(_OutlineEnabled);

                // 원본 스프라이트 뒤에 외곽선을 합성합니다.
                half visibleOutlineAlpha = outlineAlpha * (1.0h - sprite.a);
                result.rgb += _OutlineColor.rgb * visibleOutlineAlpha;
                result.a += visibleOutlineAlpha;

                return result;
            }

            ENDHLSL
        }
    }

    Fallback Off
}