//===============================================================================
//Copyright (c) 2015-2016 PTC Inc. All Rights Reserved.
//
//Vuforia is a trademark of PTC Inc., registered in the United States and other
//countries.
//===============================================================================

Shader "Custom/BlackMask" {
    Properties {
        _Alpha ("Alpha", Float) = 0.5
    }

    SubShader {
        Tags  { "Queue"="Overlay" "RenderType"="Transparent" }

        Pass {
            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float _Alpha;

            struct v2f {
                float4  pos : SV_POSITION;
            };

            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos (v.vertex);
                return o;
            }

            half4 frag(v2f i) : COLOR
            {
                half4 c = half4(0.0, 0.0, 0.0, 0.0);
                c.a = _Alpha;
                return c;
            }

            ENDCG
        }
    }
     
    FallBack "Diffuse"
}
