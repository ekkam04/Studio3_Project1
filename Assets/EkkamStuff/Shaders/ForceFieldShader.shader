// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Ekkam/ForceFieldShader" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,0.5)
        _OutlineColor ("Outline Color", Color) = (0,1,1,1)
        _BaseAlpha ("Base Alpha", Float) = 0.2
        _EdgeThickness ("Edge Thickness", Float) = 1.0
        _EdgeIntensity ("Edge Intensity", Float) = 1.0
        _ScrollSpeed ("Scroll Speed", Float) = 0.5
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest LEqual
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                float4 texCoord : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _OutlineColor;
            float _BaseAlpha;
            float _EdgeIntensity;
            float _EdgeThickness;
            float _ScrollSpeed;
            
            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = mul((float3x3)unity_ObjectToWorld, v.normal);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.texCoord = _MainTex_ST;
                return o;
            }
            
            float4 frag (v2f i) : SV_Target {
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos.xyz);
                float dotResult = dot(i.normal, viewDir);
                float rim = 1 - saturate(dotResult * _EdgeThickness);
                rim = pow(rim, _EdgeIntensity);
                
                float2 uv = i.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                uv += float2(_Time.y * _ScrollSpeed, 0);
                
                float pattern = tex2D(_MainTex, uv).r;
                float alpha = lerp(_BaseAlpha, 1.0, rim * pattern);
                
                float4 col = lerp(float4(_Color.rgb, _BaseAlpha), float4(_OutlineColor.rgb, 1), rim * pattern);
                col.a = alpha;

                return col;
            }
            ENDCG
        }
    }

    Fallback "Diffuse"
}
