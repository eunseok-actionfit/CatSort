Shader "Custom/UI/DimOverlay_MaskScale"
{
    Properties
    {
        // 원하는 모양의 마스크 텍스처 (A 채널 사용)
        // 흰색(1) = 딤 유지, 검정(0) = 구멍
        _MaskTex ("Hole Mask (A)", 2D) = "white" {}
        _MaskTex_ST ("Mask Tiling/Offset", Vector) = (1,1,0,0)

        // 마스크 반전 (0 = 기본, 1 = 반전)
        _InvertMask ("Invert Mask", Float) = 0

        // 1 = 원래 사이즈, 0.1 = 아주 작게, 3 = 크게
        _MaskScale ("Mask Scale", Range(0, 3)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGBA

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;   // 마스크 텍스처용 UV
            };

            sampler2D _MaskTex;
            float4    _MaskTex_ST;
            float     _InvertMask;
            float     _MaskScale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = TRANSFORM_TEX(v.uv, _MaskTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 가운데(0.5, 0.5)를 기준으로 스케일
                float2 centered = i.uv - 0.5;
                centered /= _MaskScale;
                float2 scaledUV = centered + 0.5;

                // 마스크 알파 사용 (0 = 구멍, 1 = 딤 유지)
                float m = tex2D(_MaskTex, scaledUV).a;

                if (_InvertMask > 0.5)
                    m = 1.0 - m;

                float holeMask = saturate(m);

                // 검은색 딤 + 구멍
                return fixed4(0, 0, 0, holeMask);
            }
            ENDCG
        }
    }

    FallBack Off
}
