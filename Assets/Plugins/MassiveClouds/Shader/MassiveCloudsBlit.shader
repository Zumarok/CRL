Shader "MassiveCloudsBlit"
{
	Properties
	{
	    [HideInInspector]
		_MainTex   ("Texture", 2D) = "white" {}
        [Toggle]
        _HORIZONTAL         ("Horizontal?", Float)              = 0
        [Toggle]
        _RelativeHeight     ("RelativeHeight?", Float)          = 0
		_Thickness          ("Thickness", Range(0, 10000))      = 50
		_FromHeight         ("FromHeight", Range(0, 5000))      = 1
		_MaxDistance        ("MaxDistance", Range(0, 60000))    = 5000
	}

	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex MassiveCloudsVertHDRPView
			#pragma fragment MassiveCloudsFragment
            #pragma shader_feature _HORIZONTAL_ON

            #include "MassiveCloudsCommon.cginc"

            float4 MassiveCloudsFragment(v2f i) : SV_Target
            {
#if defined(USING_STEREO_MATRICES)
                half4 texCol = tex2Dproj(_MainTex, UnityStereoScreenSpaceUVAdjust(i.uv, _MainTex_ST));
#else 
                half4 texCol = tex2Dproj(_MainTex, i.uv);
#endif
                return texCol;
            }
			ENDCG
		}
	}
}
