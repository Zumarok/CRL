Shader "ArcticCastle/SURFBPR_Water" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_Parallax ("Height", Range (0.005, 0.08)) = 0.02
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
	_ParallaxMap ("Heightmap", 2D) = "black" {}
	_ScrollSpeed("Scroll Speed", float) = 0.2
	_WaveFreq("Wave Frequency", float) = 20
	_Smoothness("Smoothbess", Range(0,1)) = 0.5
}

    CGINCLUDE
        #define _GLOSSYENV 1
        #define UNITY_SETUP_BRDF_INPUT MetallicSetup
    ENDCG

SubShader { 
	Tags { "RenderType"="Opaque" "Queue"="Geometry" }
	LOD 200
	
	CGPROGRAM
	#include "UnityPBSLighting.cginc"
	#pragma surface surf Standard vertex:vert
	#pragma target 3.0

       


sampler2D _MainTex, _BumpMap, _ParallaxMap;
fixed4 _Color;
half _ScrollSpeed, _WaveFreq, _Smoothness;
float _Parallax;

struct Input {
	half2 uv_MainTex;
	half2 uv_BumpMap;
	half2 uv_ParallaxMap;
	half3 viewDir;
};

void vert (inout appdata_full v) {
    float phase = _Time * _WaveFreq;
    float offset = (v.vertex.x + (v.vertex.z * 2)) * 8;
    v.vertex.y = sin(phase + offset) * 0.3;
}


void surf (Input IN, inout SurfaceOutputStandard o) {


	half scrollX = _ScrollSpeed * _Time;
	half scrollY = (_ScrollSpeed * _Time) * 0.5;

	half h = tex2D (_ParallaxMap, IN.uv_ParallaxMap).w;
	half2 offset = ParallaxOffset (h, _Parallax, IN.viewDir);


	IN.uv_MainTex = IN.uv_MainTex + offset + half2(scrollX, scrollY);
	IN.uv_BumpMap = IN.uv_BumpMap + offset + half2(scrollX, scrollY);

	half3 nrml = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));


	half4 tex = tex2D(_MainTex, IN.uv_MainTex);


	o.Albedo = saturate(tex.rgb * _Color.rgb);
	o.Smoothness = _Smoothness;
	o.Metallic = 0;
	o.Normal = nrml.rgb;
}
ENDCG
}

FallBack "Bumped Specular"
}
