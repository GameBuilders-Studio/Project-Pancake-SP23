Shader "Unlit/PartialStar"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _BGColor ("BGColor", Color) = (1,1,1,1)
        _Fraction ("Fraction", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Fraction;
            float4 _Color;
            float4 _BGColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 sample = tex2D(_MainTex, i.uv);
                fixed4 col = float4(0,0,0,0);
                if (sample.w != 0)
                {
                    col = sample;
                }
                if (i.uv.x > _Fraction)
                {
                    col = float4(0,0,0,0);
                }
                return col;
            }
            ENDCG
        }
    }
}
