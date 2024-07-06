// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "CardHighlight"
{
	Properties
	{
		_Size("Size", Range( 0 , 10)) = 1
		_RedLevel("Red Level", Float) = 0.1
		[HDR]_FXColor("FXColor", Color) = (0,2.890052,4,1)
		[HDR]_Tint("Tint", Color) = (0,0,0,0)
		_Plasma("Plasma", 2D) = "white" {}
		_FlowMap1("FlowMap1", 2D) = "white" {}
		_Mask("Mask", 2D) = "white" {}
		_Diffuse("Diffuse", 2D) = "white" {}
		_FlowSpeed("FlowSpeed", Float) = -0.07
		_FlowMap("FlowMap", 2D) = "white" {}
		_OffsetXSpeed("OffsetXSpeed", Float) = 0
		_OffsetYSpeed("OffsetYSpeed", Float) = 0
		_HighlightAlpha("HighlightAlpha", Range( 0 , 1)) = 0
		_FlowStr("FlowStr", Vector) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit alpha:fade keepalpha noshadow exclude_path:deferred noambient novertexlights nolightmap  nodynlightmap nodirlightmap nofog 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float _RedLevel;
		uniform float4 _Tint;
		uniform sampler2D _Diffuse;
		uniform float _OffsetXSpeed;
		uniform float _OffsetYSpeed;
		uniform sampler2D _Plasma;
		uniform sampler2D _FlowMap;
		uniform float4 _FlowMap_ST;
		uniform float _Size;
		uniform sampler2D _FlowMap1;
		uniform float2 _FlowStr;
		uniform float _FlowSpeed;
		uniform sampler2D _Mask;
		uniform float4 _Mask_ST;
		uniform float4 _FXColor;
		uniform float _HighlightAlpha;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float mulTime93 = _Time.y * _OffsetXSpeed;
			float mulTime98 = _Time.y * _OffsetYSpeed;
			float4 appendResult97 = (float4(mulTime93 , mulTime98 , 0.0 , 0.0));
			float2 uv_TexCoord89 = i.uv_texcoord + appendResult97.xy;
			float2 uv_FlowMap = i.uv_texcoord * _FlowMap_ST.xy + _FlowMap_ST.zw;
			float2 temp_output_4_0_g1 = (( tex2D( _FlowMap, uv_FlowMap ).rg / _Size )).xy;
			float2 uv_TexCoord17 = i.uv_texcoord * float2( 0.3,0.3 ) + float2( 0.33,0.33 );
			float2 temp_output_41_0_g1 = ( tex2D( _FlowMap1, uv_TexCoord17 ).rg + 0.5 );
			float2 temp_output_17_0_g1 = _FlowStr;
			float mulTime22_g1 = _Time.y * _FlowSpeed;
			float temp_output_27_0_g1 = frac( mulTime22_g1 );
			float2 temp_output_11_0_g1 = ( temp_output_4_0_g1 + ( temp_output_41_0_g1 * temp_output_17_0_g1 * temp_output_27_0_g1 ) );
			float2 temp_output_12_0_g1 = ( temp_output_4_0_g1 + ( temp_output_41_0_g1 * temp_output_17_0_g1 * frac( ( mulTime22_g1 + 0.5 ) ) ) );
			float4 lerpResult9_g1 = lerp( tex2D( _Plasma, temp_output_11_0_g1 ) , tex2D( _Plasma, temp_output_12_0_g1 ) , ( abs( ( temp_output_27_0_g1 - 0.5 ) ) / 0.5 ));
			float2 uv_Mask = i.uv_texcoord * _Mask_ST.xy + _Mask_ST.zw;
			float4 tex2DNode4 = tex2D( _Mask, uv_Mask );
			o.Emission = ( ( _Tint * tex2D( _Diffuse, uv_TexCoord89 ) ) + ( ( lerpResult9_g1 * tex2DNode4.g ) * _FXColor ) ).rgb;
			o.Alpha = ( tex2DNode4.a * _HighlightAlpha );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17800
788;281;1225;502;4162.439;1432.142;5.096009;True;True
Node;AmplifyShaderEditor.Vector2Node;19;-1938.134,305.7172;Inherit;False;Constant;_Vector1;Vector 1;6;0;Create;True;0;0;False;0;0.33,0.33;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;99;-1872.602,-1101.977;Inherit;False;Property;_OffsetXSpeed;OffsetXSpeed;13;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;100;-1878.363,-1014.128;Inherit;False;Property;_OffsetYSpeed;OffsetYSpeed;14;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;18;-1927.641,126.6833;Inherit;False;Constant;_Vector0;Vector 0;6;0;Create;True;0;0;False;0;0.3,0.3;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleTimeNode;93;-1719.942,-1019.889;Inherit;False;1;0;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;98;-1725.706,-942.1202;Inherit;False;1;0;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;17;-1716.472,128.1488;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;87;-1230.318,-194.8706;Inherit;False;Property;_FlowSpeed;FlowSpeed;11;0;Create;True;0;0;False;0;-0.07;-0.05;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;97;-1515.445,-982.4443;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;5;-1346.619,-117.2531;Inherit;True;Property;_FlowMap;FlowMap;12;0;Create;True;0;0;False;0;-1;6e184cd5d00de9e4d9003c8595c50b85;6e184cd5d00de9e4d9003c8595c50b85;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;88;-1365.519,-328.7705;Inherit;False;Property;_FlowStr;FlowStr;16;0;Create;True;0;0;False;0;0,0;5,5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TexturePropertyNode;9;-1201.834,-434.4572;Inherit;True;Property;_Plasma;Plasma;6;0;Create;True;0;0;False;0;8f973a247c19ec84a861cf09c3c9a044;8aba2a5c7b2ca6349b262ae9e79c41b0;False;white;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SamplerNode;13;-1315.059,128.2016;Inherit;True;Property;_FlowMap1;FlowMap1;7;0;Create;True;0;0;False;0;-1;6e184cd5d00de9e4d9003c8595c50b85;ec588d75932328c4d9ac176fc019e49c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;86;-1021.337,304.6857;Inherit;True;Property;_Mask;Mask;9;0;Create;True;0;0;False;0;None;d5d414e33fa9ecf48948be275091bb22;False;white;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;84;-1278.048,-1136.076;Inherit;True;Property;_Diffuse;Diffuse;10;0;Create;True;0;0;False;0;None;None;False;white;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;89;-1329.711,-916.0611;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;6;-912.2063,-291.9244;Inherit;True;Flow;0;;1;acad10cc8145e1f4eb8042bebe2d9a42;2,50,0,51,0;5;5;SAMPLER2D;;False;2;FLOAT2;0,0;False;18;FLOAT2;0,0;False;17;FLOAT2;1,1;False;24;FLOAT;-0.07;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;4;-864.1647,2.783636;Inherit;True;Property;_Color;Color;8;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;3;-989.8427,-911.3741;Inherit;True;Property;_BackDesign;BackDesign;1;0;Create;True;0;0;False;0;-1;bad3d50b0d663e54b844e6250c236542;bad3d50b0d663e54b844e6250c236542;True;0;False;bump;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;47;-525.6927,-214.2572;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;60;-333.9322,-1007.631;Inherit;False;Property;_Tint;Tint;5;1;[HDR];Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;52;-431.5736,74.4318;Inherit;False;Property;_FXColor;FXColor;4;1;[HDR];Create;True;0;0;False;0;0,2.890052,4,1;16,10.97382,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;102;-72.10319,-866.2796;Inherit;False;Property;_HighlightAlpha;HighlightAlpha;15;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;-198.7519,-163.4136;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;61;-145.3448,-711.7474;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector3Node;75;-2251.147,-600.1355;Inherit;False;Constant;_Scale;Scale;6;0;Create;True;0;0;True;0;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleSubtractOpNode;76;-2069.567,-972.5057;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;80;-1639.795,-784.4679;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TransformPositionNode;82;-2572.729,-396.034;Inherit;False;World;Object;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ObjSpaceViewDirHlpNode;74;-2285.352,-440.0434;Inherit;True;1;0;FLOAT4;1,1,1,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;81;-1384.045,-750.9359;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;59;-2446.61,-1021.026;Inherit;True;Property;_Height;Height;8;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;78;-1849.494,-853.4683;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;73;-2059.095,-765.968;Inherit;False;Constant;_Float1;Float 1;5;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;79;-1642.744,-554.6358;Inherit;False;0;4;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;77;-1967.747,-583.2357;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;101;79.31494,-719.7131;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;72;-2253.096,-797.8679;Inherit;False;Property;_RedLevel;Red Level;3;0;Create;True;0;0;True;0;0.1;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;71;-2665.475,-697.5596;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;22;-2.537847,-493.7848;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;58;277.8853,-748.4453;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;CardHighlight;False;False;False;False;True;True;True;True;True;True;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;True;Transparent;;Transparent;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;5;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;500;1,1,1,1;VertexOffset;False;False;Cylindrical;False;Relative;0;;4;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;93;0;99;0
WireConnection;98;0;100;0
WireConnection;17;0;18;0
WireConnection;17;1;19;0
WireConnection;97;0;93;0
WireConnection;97;1;98;0
WireConnection;13;1;17;0
WireConnection;89;1;97;0
WireConnection;6;5;9;0
WireConnection;6;2;5;0
WireConnection;6;18;13;0
WireConnection;6;17;88;0
WireConnection;6;24;87;0
WireConnection;4;0;86;0
WireConnection;3;0;84;0
WireConnection;3;1;89;0
WireConnection;47;0;6;0
WireConnection;47;1;4;2
WireConnection;51;0;47;0
WireConnection;51;1;52;0
WireConnection;61;0;60;0
WireConnection;61;1;3;0
WireConnection;76;0;59;1
WireConnection;76;1;72;0
WireConnection;80;0;78;0
WireConnection;80;1;77;0
WireConnection;81;0;80;0
WireConnection;81;1;79;0
WireConnection;78;0;76;0
WireConnection;78;1;73;0
WireConnection;77;0;75;0
WireConnection;77;1;74;0
WireConnection;101;0;4;4
WireConnection;101;1;102;0
WireConnection;22;0;61;0
WireConnection;22;1;51;0
WireConnection;58;2;22;0
WireConnection;58;9;101;0
ASEEND*/
//CHKSM=BE1340E9985C0660AEFC27A2AAF8F15A7436C292