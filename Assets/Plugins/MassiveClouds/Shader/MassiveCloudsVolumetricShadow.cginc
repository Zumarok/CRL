#ifndef MASSIVE_CLOUDS_VOLUMETRIC_SHADOW_INCLUDED
#define MASSIVE_CLOUDS_VOLUMETRIC_SHADOW_INCLUDED

#include "MassiveCloudsRaymarch.cginc"

sampler2D _ScreenTexture;
half4     _ScreenTexture_ST;
sampler2D _CloudTexture;

Ray CalculateVolumetricShadowRayRange(
          ScreenSpace      ss,
          HorizontalRegion region)
{
    const float3 up = float3(0, 1, 0);
    float maxDist = min(_MaxDistance, max(ss.maxDist, ss.isMaxPlane * _MaxDistance));
    float  cameraY          = (1 - _RelativeHeight) * ss.cameraPos.y;
    float  dbottom          = (region.height                       - cameraY);
    float  dtop             = ((region.height  + region.thickness) - cameraY);
    float  horizontalFactor = dot(ss.rayDir, up);
    float  bottomDist       = max(0, dbottom / horizontalFactor);
    float  topDist          = max(0, dtop / horizontalFactor);
    
    float  fromDist         = min(bottomDist, topDist);
    float  toDist           = max(bottomDist, topDist);

    Ray ray;
#ifdef MASSIVE_CLOUDS_MATERIAL_ON
    ray.from   = min(maxDist, fromDist);
    ray.to     = min(maxDist, toDist);
#else
    if (cameraY >= region.height)
    {
        ray.from   = bottomDist;
        if (ss.rayDir.y >= 0)
            ray.to     = 0;
        else
            ray.to     = maxDist;
    }
    else
    {
        ray.from   = 0;
        if (toDist <= 0)
            ray.to     = maxDist;
        else
            ray.to     = min(maxDist, fromDist);
    }
#endif
    ray.max    = maxDist;

    ray.length = max(0, ray.to - ray.from);
    ray.length = min(_MaxDistance / 4, ray.length);
    return ray;
}

fixed4 MassiveCloudsVolumetricShadowFragment(v2f i) : SV_Target
{
    PrepareSampler();
    PrepareLighting();

    ScreenSpace ss = CreateScreenSpace(i.uv);

#ifdef _HORIZONTAL_ON
    HorizontalRegion region = CreateRegion();
#endif

#if defined(USING_STEREO_MATRICES)
    half4 screenCol = tex2Dproj(_ScreenTexture, UnityStereoScreenSpaceUVAdjust(i.uv, _MainTex_ST));
    half4 texCol = tex2Dproj(_MainTex, UnityStereoScreenSpaceUVAdjust(i.uv, _MainTex_ST));
    half4 cloudCol = tex2Dproj(_CloudTexture, UnityStereoScreenSpaceUVAdjust(i.uv, _MainTex_ST));
#else 
    half4 screenCol = tex2Dproj(_ScreenTexture, i.uv);
    half4 texCol = tex2Dproj(_MainTex, i.uv);
    half4 cloudCol = tex2Dproj(_CloudTexture, i.uv);
#endif

#ifdef _HORIZONTAL_ON
    Ray shadowRay = CalculateVolumetricShadowRayRange(ss, region);
    VolumetricShadow(texCol, cloudCol, screenCol, shadowRay, ss, region);
#endif

    return texCol;
}

#endif