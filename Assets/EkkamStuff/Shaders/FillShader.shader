Shader "Ekkam/FillShader"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,1,1,1)
        _EmissiveColor ("Emissive Color", Color) = (0,1,0,1)
        _FillAmount ("Fill Amount", Range(0,1)) = 0
        _MinHeight ("Minimum Height", Float) = 17
        _MaxHeight ("Maximum Height", Float) = 20
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            CGPROGRAM
            uniform float _MinHeight;
            uniform float _MaxHeight;
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float3 worldPos : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            uniform float4 _Color;
            uniform float4 _EmissiveColor;
            uniform float _FillAmount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float normalizedHeight = (i.worldPos.y - _MinHeight) / (_MaxHeight - _MinHeight);
                float emissiveIntensity = step(normalizedHeight, _FillAmount);
                float4 color = lerp(_Color, _EmissiveColor, emissiveIntensity);
                return color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
