//===============================================================================
//Copyright (c) 2015-2016 PTC Inc. All Rights Reserved.
//
//Vuforia is a trademark of PTC Inc., registered in the United States and other
//countries.
//===============================================================================

Shader "Custom/GroundBlend" {
  Properties {
      _MainColor ("Main Color", Color) = (1.0, 1.0, 1.0, 1.0)
      _FogFirstColor ("Fog First Color", Color) = (1.0, 1.0, 1.0, 1.0)
      _FogSecondColor ("Fog Second Color", Color) = (1.0, 1.0, 1.0, 1.0)
      _BlendFactor ("Blend", Range(0.0, 1.0)) = 0.5
      _FogStart ("Fog Start", Float) = 10.0
      _FogEnd ("Fog End", Float) = 100.0 
      _FirstTex ("First Texture", 2D) = "white" {}
      _SecondTex ("Second Texture", 2D) = "white" {}
  }
  SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 200
    
    CGPROGRAM
    #pragma surface surf Lambert finalcolor:mycolor vertex:myvert

    float4 _MainColor;
    float4 _FogFirstColor;
    float4 _FogSecondColor;
    float _BlendFactor;
    float _FogStart;
    float _FogEnd;
    sampler2D _FirstTex;
    sampler2D _SecondTex;
    
    struct Input {
      float2 uv_FirstTex;
      half fog;
    };

    void myvert (inout appdata_full v, out Input data) {
      UNITY_INITIALIZE_OUTPUT(Input,data);
      float distance = length(UnityObjectToViewPos(v.vertex));
      float fogFactor = clamp((distance - _FogStart) / (_FogEnd - _FogStart), 0.0, 1.0);
      data.fog = clamp (fogFactor, 0.0, 0.5);
    }

    void mycolor (Input IN, SurfaceOutput o, inout fixed4 color) {
      fixed3 fogColor = lerp(_FogFirstColor.rgb, _FogSecondColor.rgb, _BlendFactor);
      #ifdef UNITY_PASS_FORWARDADD
      fogColor = 0;
      #endif
      color.rgb = lerp(color.rgb, fogColor, IN.fog);
    }

    void surf (Input IN, inout SurfaceOutput o) {
      half4 tc0 = tex2D (_FirstTex, IN.uv_FirstTex);
      half4 tc1 = tex2D (_SecondTex, IN.uv_FirstTex);
      half4 blendedTexColor = lerp(tc0, tc1, _BlendFactor);
      o.Albedo = _MainColor * blendedTexColor;
      o.Alpha = _MainColor.a;
    }
    ENDCG
  } 
  FallBack "Diffuse"
}