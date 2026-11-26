// UI(Canvas) 전용 암전 오버레이 셰이더.
// 화면 전체를 어둡게 덮고, 최대 8개의 원형 '구멍'(광원)을 뚫는다. 실제 광원이 아니라 알파 마스크 연출.
Shader "Ju/UI/BlackoutOverlay"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Overlay Color", Color) = (0,0,0,1)
        _Darkness ("Darkness", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            #define MAX_LIGHTS 8

            fixed4 _Color;
            float _Darkness;
            float _Aspect;                  // Screen.width / Screen.height
            float _LightCount;
            float4 _Lights[MAX_LIGHTS];     // xy: 뷰포트 좌표(0~1), z: 반지름(화면 높이 대비), w: 경계 부드러움(0~1)

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;                // 화면 전체를 덮는 Image이면 uv == 뷰포트 좌표
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float visible = 0;
                int count = (int)_LightCount;
                for (int k = 0; k < MAX_LIGHTS; k++)
                {
                    if (k >= count) break;
                    float4 L = _Lights[k];
                    float2 d = (i.uv - L.xy) * float2(_Aspect, 1.0);
                    float inner = L.z * (1.0 - L.w);
                    visible = max(visible, 1.0 - smoothstep(inner, L.z, length(d)));
                }
                fixed4 c = fixed4(_Color.rgb, _Darkness * (1.0 - visible) * i.color.a);
                return c;
            }
            ENDCG
        }
    }
}
