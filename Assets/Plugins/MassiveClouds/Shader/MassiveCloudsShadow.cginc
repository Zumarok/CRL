#ifndef MASSIVE_CLOUDS_SHADOW_INCLUDED
#define MASSIVE_CLOUDS_SHADOW_INCLUDED

#include "Includes/MassiveCloudsMath.cginc"
#include "Includes/MassiveCloudsScreenSpace.cginc"
#include "MassiveCloudsShadowRaymarch.cginc"

float ShadowAttn(float3 worldPos, ScreenSpace ss)
{
    HorizontalRegion horizontalRegion = CreateRegion();
    float3 cameraPos = _WorldSpaceCameraPos;
    if (worldPos.y > (horizontalRegion.height + horizontalRegion.thickness/ 2)) return 0;
    float3 dCameraPos = float3(0, -(_RelativeHeight) * cameraPos.y, 0);
    float dist = length(worldPos - _WorldSpaceCameraPos);
    float worldDotLight = saturate(dot(float3(0, 1, 0), -_MassiveCloudsSunLightDirection));
    float bottomDist = max(0, horizontalRegion.height - worldPos.y - dCameraPos.y) / worldDotLight;
    float topDist = max(0, horizontalRegion.height + horizontalRegion.thickness - worldPos.y - dCameraPos.y) / worldDotLight;
    float thickness = topDist - bottomDist;
    float mid = bottomDist + 0.1 * thickness;
    bottomDist = mid - 0.1 * thickness * _ShadowQuality;
    topDist = mid + 0.9 * thickness * _ShadowQuality;
    float shadowIter = _ShadowQuality * 49 + 1;
    float shadow = ShadowRaymarch(float4(0,0,0,0),
                                    dCameraPos + worldPos + bottomDist * MassiveCloudsLightDirection,
                                    MassiveCloudsLightDirection,
                                    topDist,
                                    (topDist - bottomDist) / shadowIter,
                                    shadowIter);
    shadow *= pow(worldDotLight, 1);
    float att = shadow / ((0.01 + 2 * _ShadowSoftness)) * (1 - ss.isMaxPlane);
//    float att = pow(shadow, 2 * _ShadowSoftness) * (1 - ss.isMaxPlane);
    return att * (2 * _ShadowStrength);
}

float VolumetricShadowAttn(float3 worldPos, ScreenSpace ss)
{
    HorizontalRegion horizontalRegion = CreateRegion();
    float3 cameraPos = _WorldSpaceCameraPos;
    float3 lightDir = normalize(MassiveCloudsLightDirection);
    float3 forward = worldPos - _WorldSpaceCameraPos;
    if (worldPos.y > (horizontalRegion.height + horizontalRegion.thickness/ 2)) return 0;
    float3 dCameraPos = float3(0, -(_RelativeHeight) * cameraPos.y, 0);
    float upDotLight = saturate(dot(float3(0,1,0), lightDir));
    float bottomDist = max(0, horizontalRegion.height - worldPos.y - dCameraPos.y) / upDotLight;
    float topDist = max(0, horizontalRegion.height + horizontalRegion.thickness - worldPos.y - dCameraPos.y) / upDotLight;
    float thickness = topDist - bottomDist;
    float mid = bottomDist + (0.05 + 0.2 * horizontalRegion.softness[0]) * thickness;
    bottomDist = mid - 0.2 * thickness * 0;
    topDist = mid + 0.8 * thickness * 1 + 1;
    float shadowIter = 1;
    float shadow = ShadowRaymarch(float4(0,0,0,0),
                                    dCameraPos + worldPos + bottomDist * lightDir,
                                    lightDir,
                                    topDist,
                                    (topDist - bottomDist) / shadowIter,
                                    shadowIter);
    shadow *= 1;
    shadow = pow(shadow, 1);
    float att = shadow;
//    float att = pow(shadow, 2 * _ShadowSoftness) * (1 - ss.isMaxPlane);
    return att;
}

#if _HDRP_SHADOW_AMBIENT
float4 ShadowColor()
{
    float4 col = _ShadowColor;
    col.rgb *= ExposureMultiplier();
    return col;
}
#else
float4 ShadowColor()
{
    return _ShadowColor;
}
#endif

void ScreenSpaceShadow(inout half4 screenCol, float3 worldPos, ScreenSpace ss)
{
    float4 shadowColor = ShadowColor();
    float attn = ShadowAttn(worldPos, ss);
    float l = saturate(luminance(screenCol));
    float luminanceFactor = smoothstep(_ShadowThreshold/2, _ShadowThreshold, l);
    float3 col = half3(1,1,1) - lerp(normalize(screenCol.rgb) * shadowColor.rgb, shadowColor.rgb, shadowColor.a);
    attn = luminanceFactor * saturate(attn);
    screenCol.rgb *= saturate(1 - col * attn);
}

#endif