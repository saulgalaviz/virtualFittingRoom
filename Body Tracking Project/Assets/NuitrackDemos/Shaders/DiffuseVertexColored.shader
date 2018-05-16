Shader "Custom/DiffuseVertexColored" 
{
	Properties {
		_Color ("Main Color", Color) = (1,1,1,0.5)
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader 
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert alpha:blend

		sampler2D _MainTex;
		fixed4 _Color;
		
		struct Input 
		{
			float2 uv_MainTex;
			half3 color : COLOR;
		};

		void surf (Input IN, inout SurfaceOutput o) 
		{
			half4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb * IN.color.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
