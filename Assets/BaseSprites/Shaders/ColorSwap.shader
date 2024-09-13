Shader "Images/Shader/ColorSwap" {/*For Toad color swap*/
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_ShadowColor("Shadow", Color) = (0,0,0,1)
		_ShadowOffset("ShadowOffset", Vector) = (0,-0.1,0,0)
		
		_Mask_StandartCol1("Mask Standart Color 1", Color) = (0.0, 0.0, 1.0, 1.0)
		_NewCol_Standart1("New Standart Color 1", Color) = (0.0, 0.0, 1.0, 1.0)
		_Mask_StandartCol2("Mask Standart Color 2", Color) = (0.0, 0.0, 1.0, 1.0)
		_NewCol_Standart2("New Standart Color 2", Color) = (0.0, 0.0, 1.0, 1.0)
		_Mask_StandartCol3("Mask Standart Color 3", Color) = (0.0, 0.0, 1.0, 1.0)
		_NewCol_Standart3("New Standart Color 3", Color) = (0.0, 0.0, 1.0, 1.0)
		_Mask_StandartCol4("Mask Standart Color 4", Color) = (0.0, 0.0, 1.0, 1.0)
		_NewCol_Standart4("New Standart Color 4", Color) = (0.0, 0.0, 1.0, 1.0)
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Cull Off
			Lighting Off
			ZWrite Off
			Blend One OneMinusSrcAlpha

			// draw shadow
			Pass
			{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile _ PIXELSNAP_ON
				#include "UnityCG.cginc"

				struct appdata_t {
					float4 vertex   : POSITION;
					float4 color    : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f {
					float4 vertex   : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord  : TEXCOORD0;
				};

				fixed4 _Color;
				fixed4 _ShadowColor;
				float4 _ShadowOffset;

				v2f vert(appdata_t IN) {
					v2f OUT;
					OUT.vertex = mul(unity_ObjectToWorld, IN.vertex);
					OUT.vertex += _ShadowOffset;
					OUT.vertex = mul(unity_WorldToObject, OUT.vertex);

					OUT.texcoord = IN.texcoord;
					OUT.color = IN.color * _ShadowColor;
	#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex);
	#endif
					OUT.vertex = UnityObjectToClipPos(OUT.vertex);
					return OUT;
				}

				sampler2D _MainTex;
				sampler2D _AlphaTex;
				float _AlphaSplitEnabled;

				fixed4 SampleSpriteTexture(float2 uv) {
					fixed4 color = tex2D(_MainTex, uv);
					color.rgb = _ShadowColor.rgb;

					#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
					if (_AlphaSplitEnabled)
						color.a = tex2D(_AlphaTex, uv).r;
					#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

					return color;
				}

				fixed4 frag(v2f IN) : SV_Target{
					fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
					c.rgb *= c.a;
					return c;
				}
			ENDCG
			}

		
			// draw real sprite
			Pass
			{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile _ PIXELSNAP_ON
				#include "UnityCG.cginc"

				struct appdata_t {
					float4 vertex   : POSITION;
					float4 color    : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f {
					float4 vertex   : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord  : TEXCOORD0;
				};

				fixed4 _Color;

				v2f vert(appdata_t IN) {
					v2f OUT;
					OUT.vertex = UnityObjectToClipPos(IN.vertex);
					OUT.texcoord = IN.texcoord;
					OUT.color = IN.color * _Color;
					#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex);
					#endif

					return OUT;
				}

				sampler2D _MainTex;
				sampler2D _AlphaTex;
				float _AlphaSplitEnabled;
				float4 _Mask_StandartCol1;
				float4 _NewCol_Standart1;
				float4 _Mask_StandartCol2;
				float4 _NewCol_Standart2;
				float4 _Mask_StandartCol3;
				float4 _NewCol_Standart3;
				float4 _Mask_StandartCol4;
				float4 _NewCol_Standart4;
				float _Sensitivity;
				float _Smooth;

				fixed4 SampleSpriteTexture(float2 uv) {
					fixed4 color = tex2D(_MainTex, uv);

					#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
					if (_AlphaSplitEnabled)
						color.a = tex2D(_AlphaTex, uv).r;
					#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

					if (color.a != 1)
						return color;

					if (color.r == _Mask_StandartCol1.r && color.g == _Mask_StandartCol1.g && color.b == _Mask_StandartCol1.b)
						return _NewCol_Standart1;
					else if (color.r == _Mask_StandartCol2.r && color.g == _Mask_StandartCol2.g && color.b == _Mask_StandartCol2.b)
						return _NewCol_Standart2;
					else if (color.r == _Mask_StandartCol3.r && color.g == _Mask_StandartCol3.g && color.b == _Mask_StandartCol3.b)
						return _NewCol_Standart3;
					else if (color.r == _Mask_StandartCol4.r && color.g == _Mask_StandartCol4.g && color.b == _Mask_StandartCol4.b)
						return _NewCol_Standart4;
					else
						return color;
				}

				fixed4 frag(v2f IN) : SV_Target{
					fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
					c.rgb *= c.a;
					return c;
				}
			ENDCG
			}
		}
}