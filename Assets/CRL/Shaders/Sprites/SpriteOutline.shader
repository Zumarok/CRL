// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SpriteOutline"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		_OutlineWidth("OutlineWidth", Range( 0 , 8)) = 5
		[HDR]_OutlineColor("OutlineColor", Color) = (0,0,0,0)
		[HDR]_Tint("Tint", Color) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#include "OutlineScript.cginc"
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float4 _Tint;
		uniform float4 _OutlineColor;
		uniform float4 _MainTex_TexelSize;
		uniform float _OutlineWidth;


		float GetNeighborWithLargestAlpha1( sampler2D baseTexture , float2 baseTextureUV , float2 baseTextureTexelSize , float currentAlpha , int searchWidth , float outlineWidth )
		{
			return GetNeigbourWithLargestAlpha( baseTexture, baseTextureUV, baseTextureTexelSize, currentAlpha, searchWidth, outlineWidth);
		}


		float IsOutline10( float currentAlpha , float largestNeighbourAlpha )
		{
			return IsOutline(currentAlpha, largestNeighbourAlpha);
		}


		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float4 tex2DNode5 = tex2D( _MainTex, uv_MainTex );
			float4 TextureColor6 = ( tex2DNode5 * _Tint );
			float currentAlpha10 = (TextureColor6).a;
			sampler2D baseTexture1 = _MainTex;
			float2 uv0_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float2 baseTextureUV1 = uv0_MainTex;
			float2 baseTextureTexelSize1 = _MainTex_TexelSize.xy;
			float currentAlpha1 = tex2DNode5.a;
			int searchWidth1 = 1;
			float outlineWidth1 = _OutlineWidth;
			float localGetNeighborWithLargestAlpha1 = GetNeighborWithLargestAlpha1( baseTexture1 , baseTextureUV1 , baseTextureTexelSize1 , currentAlpha1 , searchWidth1 , outlineWidth1 );
			float GetNeighborWithLargestAlpha9 = localGetNeighborWithLargestAlpha1;
			float largestNeighbourAlpha10 = GetNeighborWithLargestAlpha9;
			float localIsOutline10 = IsOutline10( currentAlpha10 , largestNeighbourAlpha10 );
			float IsOutline14 = localIsOutline10;
			float4 lerpResult15 = lerp( TextureColor6 , _OutlineColor , IsOutline14);
			float4 FinalColor19 = lerpResult15;
			float4 temp_output_20_0 = FinalColor19;
			o.Emission = temp_output_20_0.rgb;
			float temp_output_21_0 = (FinalColor19).a;
			o.Alpha = temp_output_21_0;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit alpha:fade keepalpha fullforwardshadows exclude_path:deferred 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17800
-951;271;1376;632;443.7532;-156.8158;1.368991;True;True
Node;AmplifyShaderEditor.TexturePropertyNode;2;-996.1332,-15.97168;Inherit;True;Property;_MainTex;MainTex;0;0;Create;True;0;0;False;0;None;f5405476d72d9c84f84647c0266586cc;False;white;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SamplerNode;5;-721.1745,382.0359;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;22;-985.0863,529.2;Inherit;False;Property;_Tint;Tint;3;1;[HDR];Create;True;0;0;False;0;0,0,0,0;0.2830189,0.2185474,0.2122642,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexelSizeNode;4;-664.9663,199.7427;Inherit;False;-1;1;0;SAMPLER2D;;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;3;-680.1572,44.793;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;8;-710.541,725.3559;Inherit;False;Property;_OutlineWidth;OutlineWidth;1;0;Create;True;0;0;False;0;5;3.7;0;8;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-317.0438,506.2688;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.IntNode;7;-605.7232,614.4601;Inherit;False;Constant;_SearchWidth;SearchWidth;1;0;Create;True;0;0;False;0;1;0;0;1;INT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;6;-151.9063,503.2268;Inherit;False;TextureColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CustomExpressionNode;1;-394.0047,-12.73577;Inherit;False;return GetNeigbourWithLargestAlpha( baseTexture, baseTextureUV, baseTextureTexelSize, currentAlpha, searchWidth, outlineWidth)@;1;False;6;False;baseTexture;SAMPLER2D;;In;;Float;False;False;baseTextureUV;FLOAT2;0,0;In;;Float;False;False;baseTextureTexelSize;FLOAT2;0,0;In;;Float;False;False;currentAlpha;FLOAT;0;In;;Float;False;False;searchWidth;INT;0;In;;Float;False;False;outlineWidth;FLOAT;0;In;;Float;False;GetNeighborWithLargestAlpha;True;False;0;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT;0;False;4;INT;0;False;5;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;9;-77.07098,-15.97227;Inherit;False;GetNeighborWithLargestAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;11;-1234.108,869.4349;Inherit;False;6;TextureColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;12;-1016.292,867.0448;Inherit;False;False;False;False;True;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;13;-1129.991,1002.027;Inherit;False;9;GetNeighborWithLargestAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;10;-694.0808,869.5239;Inherit;False;return IsOutline(currentAlpha, largestNeighbourAlpha)@;1;False;2;False;currentAlpha;FLOAT;0;In;;Float;False;False;largestNeighbourAlpha;FLOAT;0;In;;Float;False;IsOutline;True;False;0;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;14;-299.4317,868.2526;Inherit;False;IsOutline;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;16;-1044.861,1113.426;Inherit;False;6;TextureColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;18;-1026.351,1398.552;Inherit;False;14;IsOutline;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;17;-1055.151,1206.554;Inherit;False;Property;_OutlineColor;OutlineColor;2;1;[HDR];Create;True;0;0;False;0;0,0,0,0;0,0,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;15;-696.762,1188.97;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;19;-458.3517,1185.752;Inherit;False;FinalColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;20;108.6204,278.493;Inherit;False;19;FinalColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;21;315.2573,420.8433;Inherit;False;False;False;False;True;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;546.1624,230.9206;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;SpriteOutline;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;3;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;1;Include;OutlineScript.cginc;False;;Custom;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;5;0;2;0
WireConnection;4;0;2;0
WireConnection;3;2;2;0
WireConnection;23;0;5;0
WireConnection;23;1;22;0
WireConnection;6;0;23;0
WireConnection;1;0;2;0
WireConnection;1;1;3;0
WireConnection;1;2;4;0
WireConnection;1;3;5;4
WireConnection;1;4;7;0
WireConnection;1;5;8;0
WireConnection;9;0;1;0
WireConnection;12;0;11;0
WireConnection;10;0;12;0
WireConnection;10;1;13;0
WireConnection;14;0;10;0
WireConnection;15;0;16;0
WireConnection;15;1;17;0
WireConnection;15;2;18;0
WireConnection;19;0;15;0
WireConnection;21;0;20;0
WireConnection;0;2;20;0
WireConnection;0;9;21;0
WireConnection;0;10;21;0
ASEEND*/
//CHKSM=7870CCA3A84A9C7BB9724B290A1EE6B3F39FEF5D