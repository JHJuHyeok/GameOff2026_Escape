Shader "SH/UI/Result Noise Fill"
{
    Properties
    {
        _Progress ("Fill Progress", Range(0, 1)) = 0
        _NoiseAmount ("Edge Noise Amount", Range(0, 0.3)) = 0.12
        _NoiseScale ("Noise Cells Per Half Screen", Range(8, 160)) = 70
        _EdgeSoftness ("Edge Softness", Range(0.001, 0.06)) = 0.012
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "False"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"

            float _Progress;
            float _NoiseAmount;
            float _NoiseScale;
            float _EdgeSoftness;

            struct Attributes
            {
                float4 position : POSITION;
                fixed4 color : COLOR;
            };

            struct Varyings
            {
                float4 position : SV_POSITION;
                float4 screenPosition : TEXCOORD0;
                fixed4 color : COLOR;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.position = UnityObjectToClipPos(input.position);
                output.screenPosition = ComputeScreenPos(output.position);
                output.color = input.color;
                return output;
            }

            float CellNoise(float2 cell)
            {
                float3 hash = frac(float3(cell.x, cell.y, cell.x) * 0.1031);
                hash += dot(hash, hash.yzx + 33.33);
                return frac((hash.x + hash.y) * hash.z);
            }

            fixed4 Frag(Varyings input) : SV_Target
            {
                fixed4 color = input.color;
                if (_Progress <= 0.0) return fixed4(color.rgb, 0.0);
                if (_Progress >= 1.0) return color;

                float2 uv = input.screenPosition.xy / input.screenPosition.w;
                float distanceFromSide = 1.0 - abs(2.0 * uv.x - 1.0);
                float2 cell = floor(uv * float2(2.0, 1.0) * _NoiseScale);
                float noise = (CellNoise(cell) - 0.5) * _NoiseAmount;
                float softness = max(_EdgeSoftness, 0.001);
                float edgeStrength = saturate(min(_Progress, 1.0 - _Progress) * 8.0);
                float edge = _Progress + noise * edgeStrength;
                color.a *= 1.0 - smoothstep(edge - softness, edge + softness, distanceFromSide);
                return color;
            }
            ENDCG
        }
    }
}



