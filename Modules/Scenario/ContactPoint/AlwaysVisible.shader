Shader "ContactPoint/AlwaysVisible"
{
    Properties
    {
        _Color1("Always visible color1", Color) = (0,0,0,0)
        _Color2("Always visible color2", Color) = (0,0,0,0)
         _Transparency("Transparency", Range(0.0,0.5)) = 0.25
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        Pass 
        {            
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            ZTest Always
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag          
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            float4 _Color1;
            float _Transparency;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                _Color1.a = _Transparency;
                return _Color1;
            }
            
            ENDCG
        }
        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag          
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            float4 _Color2;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _Color2;
            }
            
            ENDCG
        }
    }
}
