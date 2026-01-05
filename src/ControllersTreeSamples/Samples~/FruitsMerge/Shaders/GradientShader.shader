Shader "Sprites/GradientShader"
{
    Properties
    {
        [PerRendererData]
        _MainTex ("Texture", 2D) = "white" {}
        _FirstColor ("First Color", Color) = (0.0,0.0,0.0,1.0)
        _SecondColor ("Second Color", Color) = (0.0,0.0,0.0,1.0)
        _TransitionPosition ("Transition Position", Range(0.0, 1.0)) = 0.5
        _GradientWidth ("Gradient Width", Range(0.0, 1.0)) = 0.3
        [Enum(UnityEngine.Rendering.BlendOp)] _BlendOption ("Blend Option", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _BlendSrc ("Blend mode Source", Int) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _BlendDst ("Blend mode Destination", Int) = 0
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 100
        
        ZWrite Off
        BlendOp [_BlendOption]
        Blend [_BlendSrc] [_BlendDst]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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
            float4 _FirstColor;
            float4 _SecondColor;
            float _TransitionPosition;
            float _GradientWidth;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float halfGradientWidth = _GradientWidth/2;
                float4 col = lerp(_FirstColor, _SecondColor, smoothstep(_TransitionPosition - halfGradientWidth, _TransitionPosition + halfGradientWidth, i.uv.y));
    
                return col;
            }
            ENDCG
        }
    }
}
