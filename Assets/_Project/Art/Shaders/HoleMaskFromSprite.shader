Shader "Custom/UI/HoleMaskFromSprite"
{
    Properties
    {
        _MainTex     ("Sprite Texture", 2D) = "white" {}
        _Color       ("Tint Color", Color)  = (1, 1, 1, 1)
        _AlphaCutoff ("Alpha Cutoff (Render)", Range(0,1)) = 0.1
        _StencilCutoff ("Alpha Cutoff (Stencil)", Range(0,1)) = 0.5
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

        // ---------------------------
        // Pass 1 : 실제 이미지 렌더
        // ---------------------------
        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;
            fixed4    _Color;
            float     _AlphaCutoff;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = TRANSFORM_TEX(v.uv, _MainTex);
                o.color  = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv) * i.color;

                // 렌더용 컷오프 (필요 없으면 이 clip 제거해도 됨)
                clip(c.a - _AlphaCutoff);

                return c;
            }
            ENDCG
        }

        // ---------------------------------------------
        // Pass 2 : 스프라이트 알파 모양대로 Stencil = 1
        //          (스텐실 전용 컷오프 사용)
        // ---------------------------------------------
        Pass
        {
            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }

            ColorMask 0
            ZWrite Off

            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;
            float     _StencilCutoff;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv);

                // 스텐실용 컷오프 (그림자 알파보다 크게 설정)
                clip(c.a - _StencilCutoff);

                return 0;
            }
            ENDCG
        }
    }

    FallBack Off
}
