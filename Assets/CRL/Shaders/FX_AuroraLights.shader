Shader "ArcticCastle/FX_AuroraLights" {
	Properties {
		_TintColor ("Color1", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Texture1", 2D) = "white" {}
		_BreakUp("Texture2", 2D) = "white" {}
		_Intens("Intensity", float) = 1.5
		_ScrollSpeed("Scroll_Speed", float) = 0.5	

		_Ignore_ViewDir("_Ignore_ViewDir", Range(0,1))	= 1
			
	}

SubShader {
	Tags { "Queue"="Transparent-10" "RenderType"="Transparent" }
	Blend One One
	Fog { Mode Off }
	Cull Off
	

	
		CGPROGRAM
		#pragma surface surf SimpleUnlit alpha nofog
		#pragma target 3.0

		sampler2D _MainTex, _BreakUp;
		half4 _TintColor;
		half _ScrollSpeed, _Intens, _Ignore_ViewDir;


		half4 LightingSimpleUnlit (SurfaceOutput s, half3 lightDir, half atten) {
			half4 c;
              c.rgb = s.Albedo;
              c.a = s.Alpha;
              return c;
          }

		struct Input {
			half2 uv_MainTex;
			half2 uv_BreakUp;		
			half3 viewDir;
		};

		void surf (Input IN, inout SurfaceOutput o) {

			//Scrolling uvs
			half xvalue = _ScrollSpeed * _Time;
			half xvalue2 = (_ScrollSpeed - 1) * _Time * _ScrollSpeed;	
			half2 scrollresult = IN.uv_BreakUp + half2(xvalue, 1);
			half2 scrollresult2 = IN.uv_BreakUp + half2(xvalue2, 1);


			//Textures
			half3 c = tex2D (_MainTex, IN.uv_MainTex);
			half3 br = tex2D(_BreakUp, scrollresult);
			half3 br2 = tex2D(_BreakUp, scrollresult2);	
		

			half3 combined = c.rgb * (br.rgb * br2.rgb);

		
			combined = combined * _Intens * _TintColor;		

			combined = pow(combined, 0.5);

			half3 dotCalc =dot(normalize(IN.viewDir), o.Normal);
			dotCalc = saturate(pow(dotCalc, 2));
			dotCalc = lerp(dotCalc, 2, _Ignore_ViewDir);

			combined = combined * (dotCalc);
         	half3 alph = saturate(combined.r + combined.g + combined.b);

			o.Albedo = combined.rgb;
			o.Emission = combined.rgb;
			o.Alpha = alph.r;


		
		}
			ENDCG 
		}
		Fallback "Diffuse"	

}
