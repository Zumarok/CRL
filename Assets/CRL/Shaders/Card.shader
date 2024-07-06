// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Card"
{
	Properties
	{
		_Size("Size", Range( 0 , 10)) = 1
		_RedLevel("Red Level", Float) = 0.1
		[HDR]_MistColor("MistColor", Color) = (0,2.890052,4,1)
		_Scale("Scale", Vector) = (0,0,0,0)
		[HDR]_Tint("Tint", Color) = (0,0,0,0)
		_Plasma("Plasma", 2D) = "white" {}
		_FlowMap1("FlowMap1", 2D) = "white" {}
		_Height("Height", 2D) = "white" {}
		_Mask("Mask", 2D) = "white" {}
		_Diffuse("Diffuse", 2D) = "white" {}
		_FlowMap("FlowMap", 2D) = "white" {}
		[HideInInspector] _tex4coord( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit alpha:fade keepalpha noshadow exclude_path:deferred noambient novertexlights nolightmap  nodynlightmap nodirlightmap nofog 
		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		struct Input
		{
			float2 uv_texcoord;
			float4 uv_tex4coord;
		};

		uniform float4 _Tint;
		uniform sampler2D _Diffuse;
		uniform sampler2D _Height;
		uniform float4 _Height_ST;
		uniform float _RedLevel;
		uniform float3 _Scale;
		uniform sampler2D _Plasma;
		uniform sampler2D _FlowMap;
		uniform float4 _FlowMap_ST;
		uniform float _Size;
		uniform sampler2D _FlowMap1;
		uniform sampler2D _Mask;
		uniform float4 _MistColor;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_Height = i.uv_texcoord * _Height_ST.xy + _Height_ST.zw;
			float3 objectSpaceViewDir74 = ObjSpaceViewDir( float4( 1,1,1,1 ) );
			float4 temp_output_81_0 = ( float4( ( ( ( tex2D( _Height, uv_Height ).r - _RedLevel ) * 0.5 ) * ( _Scale * objectSpaceViewDir74 ) ) , 0.0 ) + i.uv_tex4coord );
			float4 tex2DNode3 = tex2D( _Diffuse, temp_output_81_0.xy );
			float2 uv_FlowMap = i.uv_texcoord * _FlowMap_ST.xy + _FlowMap_ST.zw;
			float2 temp_output_4_0_g1 = (( tex2D( _FlowMap, uv_FlowMap ).rg / _Size )).xy;
			float2 uv_TexCoord17 = i.uv_texcoord * float2( 0.3,0.3 ) + float2( 0.33,0.33 );
			float2 temp_output_41_0_g1 = ( tex2D( _FlowMap1, uv_TexCoord17 ).rg + 0.5 );
			float2 temp_output_17_0_g1 = float2( 1,1 );
			float mulTime22_g1 = _Time.y * -0.07;
			float temp_output_27_0_g1 = frac( mulTime22_g1 );
			float2 temp_output_11_0_g1 = ( temp_output_4_0_g1 + ( temp_output_41_0_g1 * temp_output_17_0_g1 * temp_output_27_0_g1 ) );
			float2 temp_output_12_0_g1 = ( temp_output_4_0_g1 + ( temp_output_41_0_g1 * temp_output_17_0_g1 * frac( ( mulTime22_g1 + 0.5 ) ) ) );
			float4 lerpResult9_g1 = lerp( tex2D( _Plasma, temp_output_11_0_g1 ) , tex2D( _Plasma, temp_output_12_0_g1 ) , ( abs( ( temp_output_27_0_g1 - 0.5 ) ) / 0.5 ));
			o.Emission = ( ( _Tint * tex2DNode3 ) + ( ( lerpResult9_g1 * tex2D( _Mask, temp_output_81_0.xy ).g ) * _MistColor ) ).rgb;
			o.Alpha = tex2DNode3.a;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17800
873;331;1225;526;1440.18;1106.884;1.95707;True;True
Node;AmplifyShaderEditor.SamplerNode;59;-2446.61,-1021.026;Inherit;True;Property;_Height;Height;9;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;72;-2253.096,-797.8679;Inherit;False;Property;_RedLevel;Red Level;3;0;Create;True;0;0;True;0;0.1;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ObjSpaceViewDirHlpNode;74;-2285.352,-440.0434;Inherit;True;1;0;FLOAT4;1,1,1,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleSubtractOpNode;76;-2069.567,-972.5057;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;75;-2251.147,-600.1355;Inherit;False;Property;_Scale;Scale;5;0;Create;True;0;0;True;0;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;73;-2059.095,-765.968;Inherit;False;Constant;_Float1;Float 1;5;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;77;-1967.747,-583.2357;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;78;-1849.494,-853.4683;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;19;-1938.134,305.7172;Inherit;False;Constant;_Vector1;Vector 1;6;0;Create;True;0;0;False;0;0.33,0.33;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;18;-1927.641,126.6833;Inherit;False;Constant;_Vector0;Vector 0;6;0;Create;True;0;0;False;0;0.3,0.3;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;17;-1716.472,128.1488;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;80;-1639.795,-784.4679;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;79;-1642.744,-554.6358;Inherit;False;0;4;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;13;-1315.059,128.2016;Inherit;True;Property;_FlowMap1;FlowMap1;8;0;Create;True;0;0;False;0;-1;6e184cd5d00de9e4d9003c8595c50b85;6e184cd5d00de9e4d9003c8595c50b85;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;81;-1384.045,-750.9359;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;5;-1346.619,-117.2531;Inherit;True;Property;_FlowMap;FlowMap;12;0;Create;True;0;0;False;0;-1;6e184cd5d00de9e4d9003c8595c50b85;6e184cd5d00de9e4d9003c8595c50b85;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;86;-1021.337,304.6857;Inherit;True;Property;_Mask;Mask;10;0;Create;True;0;0;False;0;None;None;False;white;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;9;-1203.134,-338.2573;Inherit;True;Property;_Plasma;Plasma;7;0;Create;True;0;0;False;0;8f973a247c19ec84a861cf09c3c9a044;8f973a247c19ec84a861cf09c3c9a044;False;white;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;84;-1370.218,-1001.833;Inherit;True;Property;_Diffuse;Diffuse;11;0;Create;True;0;0;False;0;None;None;False;white;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SamplerNode;4;-864.1647,2.783636;Inherit;True;Property;_Color;Color;8;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;6;-912.2063,-291.9244;Inherit;True;Flow;0;;1;acad10cc8145e1f4eb8042bebe2d9a42;2,50,0,51,0;5;5;SAMPLER2D;;False;2;FLOAT2;0,0;False;18;FLOAT2;0,0;False;17;FLOAT2;1,1;False;24;FLOAT;-0.07;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;52;-431.5736,74.4318;Inherit;False;Property;_MistColor;MistColor;4;1;[HDR];Create;True;0;0;False;0;0,2.890052,4,1;0,0.7221026,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;3;-989.8427,-911.3741;Inherit;True;Property;_BackDesign;BackDesign;1;0;Create;True;0;0;False;0;-1;bad3d50b0d663e54b844e6250c236542;bad3d50b0d663e54b844e6250c236542;True;0;False;bump;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;60;-333.9322,-1007.631;Inherit;False;Property;_Tint;Tint;6;1;[HDR];Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;47;-525.6927,-214.2572;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;-198.7519,-163.4136;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;61;-145.3448,-711.7474;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;22;-2.537847,-493.7848;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;71;-2665.475,-697.5596;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TransformPositionNode;82;-2572.729,-396.034;Inherit;False;World;Object;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;58;277.8853,-748.4453;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;Card;False;False;False;False;True;True;True;True;True;True;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;True;Transparent;;Transparent;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;5;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;500;1,1,1,1;VertexOffset;False;False;Cylindrical;False;Relative;0;;3;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;76;0;59;1
WireConnection;76;1;72;0
WireConnection;77;0;75;0
WireConnection;77;1;74;0
WireConnection;78;0;76;0
WireConnection;78;1;73;0
WireConnection;17;0;18;0
WireConnection;17;1;19;0
WireConnection;80;0;78;0
WireConnection;80;1;77;0
WireConnection;13;1;17;0
WireConnection;81;0;80;0
WireConnection;81;1;79;0
WireConnection;4;0;86;0
WireConnection;4;1;81;0
WireConnection;6;5;9;0
WireConnection;6;2;5;0
WireConnection;6;18;13;0
WireConnection;3;0;84;0
WireConnection;3;1;81;0
WireConnection;47;0;6;0
WireConnection;47;1;4;2
WireConnection;51;0;47;0
WireConnection;51;1;52;0
WireConnection;61;0;60;0
WireConnection;61;1;3;0
WireConnection;22;0;61;0
WireConnection;22;1;51;0
WireConnection;58;2;22;0
WireConnection;58;9;3;4
WireConnection;58;10;3;4
ASEEND*/
//CHKSM=F36A2BFAE2C06578F419A834582A8E15A3327A0D