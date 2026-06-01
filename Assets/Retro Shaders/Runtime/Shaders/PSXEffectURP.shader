Shader "Retro/PSXEffectURP"
{
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"
        }
        LOD 100

        ZWrite Off
        Cull Off
        ZTest Always

        Pass
        {
            Name "PSXPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // ============================================
            // PSX SETTINGS
            // ============================================
            float _PSXColorDepth;
            float _PSXDitherIntensity;
            float _PSXPosterization;
            float _PSXResolutionScale;
            float _PSXSaturationBoost;
            float _PSXDarkening;
            float _CRTTime;

            // ============================================
            // NEW: ENHANCED SETTINGS
            // ============================================
            float _ColorBleedIntensity;
            float _ChromaticShift;
            float _VerticalBlur;
            float _VHSTracking;
            float _SignalNoise;
            float _ColorTemperature;
            float _PixelPerfectSnapping;
            float _PaletteReduction;
            float _LCDGhosting;
            float _PixelGridIntensity;
            float _PixelGridSize;
            float _GameBoyMode;

            // ============================================
            // UTILITY FUNCTIONS (Original)
            // ============================================

            // Ordered dithering (Bayer matrix 4x4)
            static const float bayerMatrix[16] = {
                0.0 / 16.0, 8.0 / 16.0, 2.0 / 16.0, 10.0 / 16.0,
                12.0 / 16.0, 4.0 / 16.0, 14.0 / 16.0, 6.0 / 16.0,
                3.0 / 16.0, 11.0 / 16.0, 1.0 / 16.0, 9.0 / 16.0,
                15.0 / 16.0, 7.0 / 16.0, 13.0 / 16.0, 5.0 / 16.0
            };

            float getBayerValue(float2 pixelPos)
            {
                int x = int(fmod(pixelPos.x, 4.0));
                int y = int(fmod(pixelPos.y, 4.0));
                return bayerMatrix[y * 4 + x];
            }

            // Color depth reduction with dithering (PS1 style)
            float3 psxColorReduce(float3 color, float2 pixelPos)
            {
                float dither = getBayerValue(pixelPos);
                dither = (dither - 0.5) * _PSXDitherIntensity;

                float levels = pow(2.0, _PSXColorDepth);

                float3 ditheredColor = color + dither / levels;

                // Quantize to limited color depth
                float3 quantized = floor(ditheredColor * levels) / (levels - 1.0);

                return saturate(quantized);
            }

            // Posterization for additional color banding
            float3 posterize(float3 color, float levels)
            {
                return floor(color * levels) / levels;
            }

            // PS1-style resolution reduction (pixelation at PSX resolution)
            float2 psxPixelate(float2 uv, float2 screenSize)
            {
                float2 psxRes = screenSize * _PSXResolutionScale;
                psxRes = max(psxRes, float2(160, 120));

                return floor(uv * psxRes) / psxRes;
            }

            // Saturation adjustment
            float3 adjustSaturation(float3 color, float saturation)
            {
                float luma = dot(color, float3(0.299, 0.587, 0.114));
                return lerp(float3(luma, luma, luma), color, saturation);
            }

            // ============================================
            // NEW: ENHANCED FUNCTIONS
            // ============================================

            // Color temperature adjustment
            float3 applyColorTemperature(float3 color, float temperature)
            {
                float3 warm = float3(1.0, 0.95, 0.8);
                float3 cool = float3(0.8, 0.9, 1.0);
                float3 tint = lerp(cool, warm, temperature * 0.5 + 0.5);
                return color * tint;
            }

            // NTSC color bleeding
            float3 colorBleed(float2 uv, float2 texelSize)
            {
                float bleed = _ColorBleedIntensity * 2.0;

                float3 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv).rgb;
                col += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + float2(texelSize.x * bleed, 0)).rgb *
                    0.3;
                col += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv - float2(texelSize.x * bleed, 0)).rgb *
                    0.2;

                return col / 1.5;
            }

            // Chromatic aberration
            float3 chromaticAberration(float2 uv, float2 texelSize)
            {
                float2 offset = float2(_ChromaticShift, 0) * texelSize;

                float r = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + offset).r;
                float g = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv).g;
                float b = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv - offset).b;

                return float3(r, g, b);
            }

            // Vertical blur
            float3 verticalBlur(float2 uv, float2 texelSize)
            {
                float blurAmount = _VerticalBlur * texelSize.y;

                float3 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv).rgb * 0.4;
                col += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + float2(0, blurAmount)).rgb * 0.3;
                col += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv - float2(0, blurAmount)).rgb * 0.3;

                return col;
            }

            // VHS tracking
            float2 vhsTracking(float2 uv)
            {
                float jitter = frac(sin(_CRTTime * 50.0) * 43758.5453); 
                float trackingNoise = sin(uv.y * 100.0 + _CRTTime * 30.0 + jitter) * _VHSTracking * 0.01;
                uv.x += trackingNoise;
                return uv;
            }

            // Signal noise
            float signalNoise(float2 uv)
            {
                float noise = frac(sin(dot(uv + _CRTTime * 0.1, float2(12.9898, 78.233))) * 43758.5453);
                return (noise - 0.5) * _SignalNoise;
            }

            // Pixel Perfect Snapping
            float2 pixelPerfectSnap(float2 uv, float2 screenSize)
            {
                float2 pixelSize = 1.0 / screenSize;
                float2 snappedUV = floor(uv / pixelSize) * pixelSize + pixelSize * 0.5;
                return lerp(uv, snappedUV, _PixelPerfectSnapping);
            }

            // Palette Reduction
            float3 reduceToPalette(float3 color)
            {
                float paletteSize = lerp(256.0, 16.0, _PaletteReduction);
                float3 quantized;
                quantized.r = floor(color.r * paletteSize) / paletteSize;
                quantized.g = floor(color.g * paletteSize) / paletteSize;
                quantized.b = floor(color.b * paletteSize) / paletteSize;
                return quantized;
            }

            // LCD Ghosting
            float3 lcdGhosting(float3 color, float2 uv, float2 texelSize)
            {
                float3 ghost = color;
                ghost += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + float2(texelSize.x, 0) * 2.0).rgb *
                    0.3;
                ghost += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + float2(texelSize.x, 0) * 4.0).rgb *
                    0.15;
                return lerp(color, ghost / 1.45, _LCDGhosting);
            }

            // Pixel Grid
            float3 applyPixelGrid(float3 color, float2 uv, float2 screenSize)
            {
                float2 pixelPos = frac(uv * screenSize / _PixelGridSize);
                float grid = 1.0 - (step(0.95, pixelPos.x) + step(0.95, pixelPos.y)) * _PixelGridIntensity * 0.3;
                return color * grid;
            }

            // Game Boy Palette
            float3 applyGameBoyPalette(float3 color)
            {
                float luma = dot(color, float3(0.299, 0.587, 0.114));
                int index = int(floor(luma * 3.99));

                if (index == 0) return float3(0.06, 0.22, 0.06);
                else if (index == 1) return float3(0.19, 0.38, 0.19);
                else if (index == 2) return float3(0.54, 0.67, 0.06);
                else return float3(0.61, 0.73, 0.06);
            }

            // ============================================
            // MAIN FRAGMENT SHADER
            // ============================================

            half4 frag(Varyings input) : SV_Target
            {
                float2 screenSize = _ScreenParams.xy;
                float2 texelSize = _ScreenParams.zw - 1.0;
                float2 uv = input.texcoord;

                // Pixel Perfect Snapping
                if (_PixelPerfectSnapping > 0.001)
                    uv = pixelPerfectSnap(uv, screenSize);

                // VHS Tracking
                if (_VHSTracking > 0.001)
                    uv = vhsTracking(uv);

                // Resolution reduction
                float2 psxUV = uv;
                if (_PSXResolutionScale < 0.99)
                    psxUV = psxPixelate(uv, screenSize);

                // Sample with effects
                float3 col;
                if (_ChromaticShift > 0.001)
                    col = chromaticAberration(psxUV, texelSize);
                else if (_ColorBleedIntensity > 0.001)
                    col = colorBleed(psxUV, texelSize);
                else if (_VerticalBlur > 0.001)
                    col = verticalBlur(psxUV, texelSize);
                else
                    col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, psxUV).rgb;

                // Signal noise
                if (_SignalNoise > 0.001)
                    col += signalNoise(uv);

                // LCD Ghosting
                if (_LCDGhosting > 0.001)
                    col = lcdGhosting(col, psxUV, texelSize);

                // PSX darkening (subtle)
                if (_PSXDarkening > 0.0001)
                    col *= (1.0 - _PSXDarkening * 0.3);

                // PSX saturation boost
                if (_PSXSaturationBoost > 1.0001)
                    col = adjustSaturation(col, _PSXSaturationBoost);

                // Color temperature
                if (abs(_ColorTemperature) > 0.001)
                    col = applyColorTemperature(col, _ColorTemperature);

                // Palette reduction
                if (_PaletteReduction > 0.001)
                    col = reduceToPalette(col);

                // Game Boy palette
                if (_GameBoyMode > 0.5)
                    col = applyGameBoyPalette(col);

                // PSX posterization (before dithering)
                if (_PSXPosterization > 0.0001)
                {
                    float posterLevels = lerp(256.0, 8.0, _PSXPosterization);
                    col = posterize(col, posterLevels);
                }

                // PSX color depth reduction with dithering
                if (_PSXColorDepth < 7.99)
                {
                    float2 pixelPos = uv * screenSize;
                    col = psxColorReduce(col, pixelPos);
                }

                // Pixel grid
                if (_PixelGridIntensity > 0.001)
                    col = applyPixelGrid(col, uv, screenSize);

                return half4(saturate(col), 1);
            }
            ENDHLSL
        }
    }
}