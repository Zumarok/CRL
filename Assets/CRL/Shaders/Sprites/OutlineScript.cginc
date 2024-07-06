// Search surrounding pixels up to search width and return largest alpha value found.

float GetNeigbourWithLargestAlpha(sampler2D baseTexture, float2 baseTextureUV, float2 baseTextureTexelSize, float currentAlpha, int searchWidth, float outlineWidth)
{
    float alpha = currentAlpha;
    float2 texelSize = (outlineWidth / searchWidth) * baseTextureTexelSize.xy;

    for (int x = -searchWidth; x <= searchWidth; x++)
    {
        for (int y = -searchWidth; y <= searchWidth; y++)
        {
            if (x == 0 && y == 0)
                continue; // Ignore this pixel.

            float2 offset = float2(x, y) * texelSize;
            float4 neighbour = tex2D(baseTexture, baseTextureUV + offset);

            alpha = max(alpha, neighbour.a);
        }
    }

    return alpha;
}

// Based on current pixel and largest neighbour alpha, should current pixel be an outline pixel?

bool IsOutline(float currentAlpha, float largestNeighbourAlpha)
{
    if (currentAlpha < 0.9 && largestNeighbourAlpha >= 0.5)
    {
        return true;
    }

    return false;
}