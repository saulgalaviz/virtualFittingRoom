Shader "Custom/DepthSegmentationCustom"
{
	Properties
	{
		_MainTexture ("Texture", 2D) = "white" {}
		_DepthTex ("Depth texture", 2D) = "white" {}
		_SegmentationTex("Segmentation texture", 2D) = "white" {}
		_RGBTex("RGB texture", 2D) = "white" {}
		_ScaleMult("Scale multiplier", Float) = 0.001
		_Greyness("Grayness", Float) = 0.5
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.2
		_SegmZeroColor("Segmentation zero color", Color) = (1,1,1,1)
		_ShowBorders ("Show borders", int) = 1
	}

	SubShader
	{
		Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" "IgnoreProjector"="True" }
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
		LOD 200

		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite On

		Pass
		{
			Tags {"LightMode" = "ForwardBase"}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

			uniform float3 _Offsets[50]; //offsets of vertexes for used point mesh

			uniform float fX;
			uniform float fY;

			struct appdata
			{
				float4 vertex : POSITION;

				float2 uv : TEXCOORD0;
				float2 depthPos : TEXCOORD1; //pos on depthmap
				float2 index : TEXCOORD2; //x is used for pixel vertex index
				float3 normal: NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1; 
				float4 vertex : SV_POSITION;
				fixed4 diff : COLOR0;
			};

			float _ScaleMult;
			float _Greyness;
			sampler2D _DepthTex;
			sampler2D _SegmentationTex;
			float4 _SegmentationTex_TexelSize;
			sampler2D _MainTexture;
			sampler2D _RGBTex;
			fixed _Cutoff;
			float4 _SegmZeroColor;
			int _ShowBorders;

			v2f vert (appdata v)
			{
				v2f o;

				float3 off = _Offsets[round(v.index.x)]; //offset for current vertex
				float4 depthCol = tex2Dlod(_DepthTex, float4(v.depthPos, 0, 0));

				//real positions of pixel:
				float z = depthCol.r * _ScaleMult * 16384; // should be enough, as depth is ushort in C#
				float x =  z * (v.depthPos.x - 0.5) / fX;
				float y = -z * (v.depthPos.y - 0.5) / fY;

				float4 newVertexPos = float4(x, y, z, 1) + float4(off, 0) * z;

				half3 worldNormal  = UnityObjectToWorldNormal(v.normal);
				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				o.diff = nl * _LightColor0;
				o.diff.rgb += ShadeSH9(half4(worldNormal,1));
				o.diff.a = 1;

				float colorModRed = max(0, 1 - newVertexPos.z / 4);
				float colorModGreen = max(0, 1 - abs(newVertexPos.z - 3) / 1.5);
				float colorModBlue = max(0, (newVertexPos.z - 3) / 7);

				o.diff.rgb *= float3(_Greyness + (1 - _Greyness) * colorModRed, _Greyness + (1 - _Greyness) * colorModGreen, _Greyness + (1 - _Greyness) * colorModBlue);

				o.vertex = UnityObjectToClipPos(newVertexPos);
				o.uv2 = v.depthPos;
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTexture, i.uv) * tex2D(_RGBTex, i.uv2) * i.diff;
				fixed4 colCenter = tex2D(_SegmentationTex, i.uv2);
				float zeroLength = length(tex2D(_SegmentationTex, i.uv2) - _SegmZeroColor);

				float colUp = length(tex2D(_SegmentationTex, i.uv2) - tex2D(_SegmentationTex, i.uv2 + float2(0, 0.016667)));
				float colDown = length(tex2D(_SegmentationTex, i.uv2) - tex2D(_SegmentationTex, i.uv2 - float2(0, 0.016667)));
				float colLeft = length(tex2D(_SegmentationTex, i.uv2) - tex2D(_SegmentationTex, i.uv2 + float2(0.0125, 0)));
				float colRight = length(tex2D(_SegmentationTex, i.uv2) - tex2D(_SegmentationTex, i.uv2 - float2(0.0125, 0)));

				if ((_ShowBorders != 0) && (zeroLength > 0.01) && ((colUp > 0.01) || (colDown > 0.01) || (colLeft > 0.01) || (colRight > 0.01))) col = colCenter;

				clip(col.a - _Cutoff);
				return col;
			}
			ENDCG
		}
	}
}
