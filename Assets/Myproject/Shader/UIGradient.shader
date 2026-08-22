Shader "Custom/UIGradient"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (1,1,1,1)
        _BottomColor ("Bottom Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // UI用の透過や描画順の設定
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "PreviewType"="Plane" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            float4 _TopColor;
            float4 _BottomColor;
            sampler2D _MainTex;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                
                // UVのY座標（0.0 〜 1.0）を使って、下と上の色を混ぜ合わせる
                float4 gradColor = lerp(_BottomColor, _TopColor, v.uv.y);
                
                // 元のUIの色にグラデーション色を掛け合わせる
                o.color = v.color * gradColor;
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // テクスチャの色と計算した色を最終出力する
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                return col;
            }
            ENDCG
        }
    }
}