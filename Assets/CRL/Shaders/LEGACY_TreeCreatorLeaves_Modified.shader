Shader "ArcticCastle/TreeCreatorLeaves_Modified" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	//_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
	_MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
	//_GlossMap ("Gloss (A)", 2D) = "black" {}
	//_TranslucencyMap ("Translucency (A)", 2D) = "black" {}
	//_ShadowOffset ("Shadow Offset (A)", 2D) = "black" {}
	
	// These are here only to provide default values
	_Cutoff ("Alpha cutoff", Range(0,1)) = 0.3
	[HideInInspector] _TreeInstanceColor ("TreeInstanceColor", Vector) = (1,1,1,1)
	[HideInInspector] _TreeInstanceScale ("TreeInstanceScale", Vector) = (1,1,1,1)
	[HideInInspector] _SquashAmount ("Squash", Float) = 1
}

   CGINCLUDE
        #define _GLOSSYENV 1
        #define UNITY_SETUP_BRDF_INPUT MetallicSetup
    ENDCG

SubShader { 
	Tags {"RenderType"="Opaque" "Queue"="Geometry"}
	LOD 200

		
CGPROGRAM
#pragma surface surf Standard alphatest:_Cutoff noforwardadd nolightmap vertex:TreeVertLeaf
#include "UnityBuiltin3xTreeLibrary.cginc"

sampler2D _MainTex;
sampler2D _BumpMap;
sampler2D _GlossMap;
sampler2D _TranslucencyMap;
half _Shininess;

struct Input {
	float2 uv_MainTex;
	fixed4 color : COLOR; // color.a = AO
};

void surf (Input IN, inout SurfaceOutputStandard o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
	o.Albedo = c.rgb * IN.color.rgb * IN.color.a;
	o.Emission = c.rgb * 0.06;
	//o.Gloss = tex2D(_GlossMap, IN.uv_MainTex).a;
	o.Alpha = c.a;

	
	o.Smoothness = tex2D(_GlossMap, IN.uv_MainTex).a;;
	o.Metallic = 0;
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
}
ENDCG
}

Dependency "OptimizedShader" = "Hidden/ArcticCastle/TreeCreatorLeaves_Optimized"
//Dependency "BillboardShader" = "Hidden/Nature/Tree Creator Leaves Rendertex"
FallBack "Diffuse"
}
