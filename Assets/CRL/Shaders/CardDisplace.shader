// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "CardDisplace"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_MainTex("MainTex", 2D) = "white" {}
		_EmissionStr("Emission Str", Float) = 1
		_HeightTex("Height Tex", 2D) = "white" {}
		_RedLevel("Red Level", Float) = 0.5
		_Scale("Scale", Vector) = (0,0,0,0)
		[HideInInspector] _tex4coord( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull Back
		ColorMask RGB
		CGPROGRAM
		#include "UnityCG.cginc"
		#pragma target 4.6
		#pragma surface surf Standard keepalpha noshadow exclude_path:deferred noambient novertexlights nolightmap  nodynlightmap nodirlightmap nofog nometa noforwardadd 
		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		struct Input
		{
			float2 uv_texcoord;
			float4 uv_tex4coord;
		};

		uniform sampler2D _MainTex;
		uniform sampler2D _HeightTex;
		uniform half4 _HeightTex_ST;
		uniform half _RedLevel;
		uniform half3 _Scale;
		uniform half _EmissionStr;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_HeightTex = i.uv_texcoord * _HeightTex_ST.xy + _HeightTex_ST.zw;
			half3 objectSpaceViewDir26 = ObjSpaceViewDir( float4( 0,0,0,1 ) );
			half4 tex2DNode1 = tex2D( _MainTex, ( half4( ( ( ( tex2D( _HeightTex, uv_HeightTex ).r - _RedLevel ) * 0.5 ) * ( _Scale * objectSpaceViewDir26 ) ) , 0.0 ) + i.uv_tex4coord ).xy );
			half4 temp_output_6_0 = ( tex2DNode1 * tex2DNode1.a );
			o.Albedo = temp_output_6_0.rgb;
			o.Emission = ( temp_output_6_0 / _EmissionStr ).rgb;
			o.Alpha = 1;
			clip( tex2DNode1.a - _Cutoff );
		}

		ENDCG
	}
	Fallback "Sprites/Default"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17800
277;609;2007;728;2333.568;721.4983;1.228832;True;True
Node;AmplifyShaderEditor.SamplerNode;9;-1815.875,-315.9162;Inherit;True;Property;_HeightTex;Height Tex;3;0;Create;True;0;0;True;0;-1;62ecc4b9f88cce844ae8fb21f9d0b3d5;62ecc4b9f88cce844ae8fb21f9d0b3d5;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;12;-1688.577,-112.6162;Inherit;False;Property;_RedLevel;Red Level;4;0;Create;True;0;0;True;0;0.5;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-1502.377,-139.2162;Inherit;False;Constant;_Float0;Float 0;5;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ObjSpaceViewDirHlpNode;26;-1765.035,132.1087;Inherit;False;1;0;FLOAT4;0,0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;19;-1694.43,-29.28353;Inherit;False;Property;_Scale;Scale;5;0;Create;True;0;0;True;0;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleSubtractOpNode;11;-1503.777,-251.6162;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-1511.13,-24.08358;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-1340.877,-247.5162;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;25;-1186.127,4.516274;Inherit;False;0;4;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-1183.178,-226.6158;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;20;-927.4282,-191.7838;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TexturePropertyNode;2;-1130.134,-474.9523;Inherit;True;Property;_MainTex;MainTex;1;0;Create;True;0;0;True;0;None;672c02290fb9d5d459aef9f88d5e5050;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SamplerNode;1;-707.0833,-302.7697;Inherit;True;Property;_TextureSample0;Texture Sample 0;2;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-353.5664,-307.6436;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-311.619,-70.86799;Inherit;False;Property;_EmissionStr;Emission Str;2;0;Create;True;0;0;True;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;8;-114.3823,-219.522;Inherit;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;562.588,-246.3403;Half;False;True;-1;6;ASEMaterialInspector;0;0;Standard;CardDisplace;False;False;False;False;True;True;True;True;True;True;True;True;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;TransparentCutout;;Transparent;ForwardOnly;14;all;True;True;True;False;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;5;False;-1;10;False;-1;0;5;False;-1;10;False;-1;0;False;-1;2;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;Sprites/Default;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;11;0;9;1
WireConnection;11;1;12;0
WireConnection;17;0;19;0
WireConnection;17;1;26;0
WireConnection;13;0;11;0
WireConnection;13;1;14;0
WireConnection;15;0;13;0
WireConnection;15;1;17;0
WireConnection;20;0;15;0
WireConnection;20;1;25;0
WireConnection;1;0;2;0
WireConnection;1;1;20;0
WireConnection;6;0;1;0
WireConnection;6;1;1;4
WireConnection;8;0;6;0
WireConnection;8;1;7;0
WireConnection;0;0;6;0
WireConnection;0;2;8;0
WireConnection;0;10;1;4
ASEEND*/
//CHKSM=20A3A81F4C5533553AB70444B975663F691E5AE2