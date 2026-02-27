

// =======================================================
// 3) 구멍 오브젝트용 - 원형/라운드 박스 자동 Feather 버전
//    Shader: Custom/UI/HoleMaskRoundFeather
//
//    - Image는 그냥 흰 사각 스프라이트여도 됨.
//    - _ShapeType 으로 모양 선택:
//        0 : 원형
//        1 : 라운드 사각형
//    - _Feather 로 가장자리 부드럽게
// =======================================================
Shader "Custom/UI/HoleMaskRoundFeather"
{
    Properties
    {
        _MainTex ("Sprite Texture (unused for shape)", 2D) = "white" {}
        _Color   ("Tint Color", Color) = (1, 1, 1, 1)

        _ShapeType   ("ShapeType (0:Circle, 1:RoundRect)", Float) = 0
        _Radius      ("Circle Radius", Range(0,0.5)) = 0.4
        _RoundRadius ("Round Corner Radius", Range(0,0.5)) = 0.15
        _Feather     ("Feather", Range(0,0.25)) = 0.05
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
        // 공통 함수: 마스크 알파 계산
        // ---------------------------
        CGINCLUDE
        #include "UnityCG.cginc"

        float  _ShapeType;
        float  _Radius;
        float  _RoundRadius;
        float  _Feather;
        fixed4 _Color;

        // uv: 0~1, 중앙 기준으로 모양 생성
        float ComputeMaskAlpha(float2 uv)
        {
            float2 p = uv * 2.0 - 1.0; // (-1 ~ 1), 중심 (0,0)

            if (_ShapeType < 0.5)
            {
                // --------------------
                // Circle
                // --------------------
                float dist = length(p);
                float inner = _Radius - _Feather;
                float outer = _Radius + _Feather;
                float m = 1.0 - smoothstep(inner, outer, dist); // 1=안, 0=밖
                return saturate(m);
            }
            else
            {
                // --------------------
                // Rounded Rect
                // --------------------
                float2 halfSize = float2(1.0, 1.0);  // 전체 Quad 적용
                float r = _RoundRadius;

                // SDF 기반 라운드 사각형
                float2 q = abs(p) - (halfSize - r);
                float2 qMax = max(q, 0.0);
                float dist = length(qMax) + min(max(q.x, q.y), 0.0) - r;

                float m = 1.0 - smoothstep(0.0, _Feather, dist);
                return saturate(m);
            }
        }

        ENDCG

        // ---------------------------
        // Pass 1 : 실제 하이라이트 렌더
        // ---------------------------
        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

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
                fixed4 color  : COLOR;
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;

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
                float mask = ComputeMaskAlpha(i.uv);

                fixed4 c = i.color;
                c.a *= mask;      // Feather 적용

                clip(c.a - 0.001);  // 거의 0인 건 버림

                return c;
            }
            ENDCG
        }

        // ---------------------------
        // Pass 2 : 모양대로 Stencil = 1
        // ---------------------------
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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float mask = ComputeMaskAlpha(i.uv);

                clip(mask - 0.001);   // 모양 안쪽만 스텐실 기록

                return 0;
            }
            ENDCG
        }
    }

    FallBack Off
}
