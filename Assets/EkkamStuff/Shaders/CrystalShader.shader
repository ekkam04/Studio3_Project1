Shader "Ekkam/CrystalShader" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _FresnelPower ("Fresnel Power", Range(1.0, 10.0)) = 2.5
        _Reflectivity ("Reflectivity", Range(0.0, 1.0)) = 0.5
        _Transparency ("Transparency", Range(0.0, 1.0)) = 0.5
    }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float3 normal : NORMAL;
                float3 viewDir : TEXCOORD0;
            };

            fixed4 _Color;
            float _FresnelPower;
            float _Reflectivity;
            float _Transparency;

            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, v.vertex).xyz);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {

                float fresnel = pow(1.0 - max(dot(normalize(i.viewDir), i.normal), 0.0), _FresnelPower);
                
                fixed4 color = _Color;
                color.a *= _Transparency;
                color.rgb += fresnel * _Reflectivity;
                float bloomThreshold = 0.4;
                color.rgb = smoothstep(bloomThreshold, 1.0, color.rgb);

                return color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
