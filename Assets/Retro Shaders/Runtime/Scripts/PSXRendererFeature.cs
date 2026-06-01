using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Retro.PSXEffects.Retro_Shaders.Runtime.Scripts
{
    public class PsxRendererFeature : ScriptableRendererFeature
    {
        public PsxSettings settings = new();
        private Material material;
        private PsxRenderPass renderPass;

        public override void Create()
        {
            var shader = Shader.Find("Retro/PSXEffectURP");
            if (shader != null)
                material = CoreUtils.CreateEngineMaterial(shader);

            renderPass = new PsxRenderPass(material, settings)
            {
                renderPassEvent = settings.renderPassEvent
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (material == null || renderingData.cameraData.cameraType != CameraType.Game)
                return;

            renderPass.UpdateSettings(settings);
            renderer.EnqueuePass(renderPass);
        }

        protected override void Dispose(bool disposing)
        {
            if (material != null)
                CoreUtils.Destroy(material);
        }

        [Serializable]
        public class PsxSettings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

            [Header("Core PSX Settings")]
            [Tooltip("Color bit depth per channel. PS1 used ~5 bits (32 levels).")]
            [Range(2, 8)]
            public float psxColorDepth = 5f;

            [Tooltip("Ordered dithering intensity.")]
            [Range(0, 1)]
            public float psxDitherIntensity = 0.5f;

            [Tooltip("Additional color posterization.")]
            [Range(0, 1)]
            public float psxPosterization = 0.2f;

            [Tooltip("Resolution scale. 0.5 = PS1-like pixelation.")]
            [Range(0.1f, 1f)]
            public float psxResolutionScale = 0.5f;

            [Tooltip("Saturation boost.")]
            [Range(1f, 2f)]
            public float psxSaturationBoost = 1.2f;

            [Tooltip("Subtle darkening.")]
            [Range(0, 1)]
            public float psxDarkening = 0.1f;

            [Header("Analog/CRT Effects")]
            [Tooltip("NTSC color bleeding")]
            [Range(0, 1)]
            public float colorBleedIntensity = 0.1f;

            [Tooltip("Chromatic aberration")]
            [Range(0, 2)]
            public float chromaticShift;

            [Tooltip("Vertical blur")]
            [Range(0, 3)]
            public float verticalBlur = 0.5f;

            [Tooltip("VHS tracking errors")]
            [Range(0, 1)]
            public float vhsTracking;

            [Tooltip("Analog signal noise")]
            [Range(0, 0.5f)]
            public float signalNoise = 0.05f;

            [Tooltip("Color temperature")]
            [Range(-1, 1)]
            public float colorTemperature = 0.15f;

            [Header("2D Pixel Art")]
            [Tooltip("Snap to pixel grid")]
            [Range(0, 1)]
            public float pixelPerfectSnapping = 0.5f;

            [Tooltip("Reduce colors to palette")]
            [Range(0, 1)]
            public float paletteReduction;

            [Tooltip("LCD ghosting effect")]
            [Range(0, 1)]
            public float lcdGhosting;

            [Tooltip("Pixel grid intensity")]
            [Range(0, 1)]
            public float pixelGridIntensity = 0.1f;

            [Tooltip("Pixel size for grid")]
            [Range(1, 8)]
            public float pixelGridSize = 2f;

            [Tooltip("Game Boy mode")]
            [Range(0, 1)]
            public float gameBoyMode;
        }
    }

    public class PsxRenderPass : ScriptableRenderPass
    {
        private static readonly int psxColorDepthID = Shader.PropertyToID("_PSXColorDepth");
        private static readonly int psxDitherIntensityID = Shader.PropertyToID("_PSXDitherIntensity");
        private static readonly int psxPosterizationID = Shader.PropertyToID("_PSXPosterization");
        private static readonly int psxResolutionScaleID = Shader.PropertyToID("_PSXResolutionScale");
        private static readonly int psxSaturationBoostID = Shader.PropertyToID("_PSXSaturationBoost");
        private static readonly int psxDarkeningID = Shader.PropertyToID("_PSXDarkening");
        private static readonly int timeID = Shader.PropertyToID("_CRTTime");

        private static readonly int colorBleedIntensityID = Shader.PropertyToID("_ColorBleedIntensity");
        private static readonly int chromaticShiftID = Shader.PropertyToID("_ChromaticShift");
        private static readonly int verticalBlurID = Shader.PropertyToID("_VerticalBlur");
        private static readonly int vhsTrackingID = Shader.PropertyToID("_VHSTracking");
        private static readonly int signalNoiseID = Shader.PropertyToID("_SignalNoise");
        private static readonly int colorTemperatureID = Shader.PropertyToID("_ColorTemperature");

        private static readonly int pixelPerfectSnappingID = Shader.PropertyToID("_PixelPerfectSnapping");
        private static readonly int paletteReductionID = Shader.PropertyToID("_PaletteReduction");
        private static readonly int lcdGhostingID = Shader.PropertyToID("_LCDGhosting");
        private static readonly int pixelGridIntensityID = Shader.PropertyToID("_PixelGridIntensity");
        private static readonly int pixelGridSizeID = Shader.PropertyToID("_PixelGridSize");
        private static readonly int gameBoyModeID = Shader.PropertyToID("_GameBoyMode");

        private readonly Material material;
        private PsxRendererFeature.PsxSettings settings;

        public PsxRenderPass(Material material, PsxRendererFeature.PsxSettings settings)
        {
            this.material = material;
            this.settings = settings;
        }

        public void UpdateSettings(PsxRendererFeature.PsxSettings settings)
        {
            this.settings = settings;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (material == null)
                return;

            var resourceData = frameData.Get<UniversalResourceData>();

            if (resourceData.isActiveTargetBackBuffer)
                return;

            var source = resourceData.activeColorTexture;

            var destinationDesc = renderGraph.GetTextureDesc(source);
            destinationDesc.name = "_PSXTempTexture";
            destinationDesc.clearBuffer = false;

            var destination = renderGraph.CreateTexture(destinationDesc);

            material.SetFloat(psxColorDepthID, settings.psxColorDepth);
            material.SetFloat(psxDitherIntensityID, settings.psxDitherIntensity);
            material.SetFloat(psxPosterizationID, settings.psxPosterization);
            material.SetFloat(psxResolutionScaleID, settings.psxResolutionScale);
            material.SetFloat(psxSaturationBoostID, settings.psxSaturationBoost);
            material.SetFloat(psxDarkeningID, settings.psxDarkening);
            material.SetFloat(timeID, Time.realtimeSinceStartup);

            material.SetFloat(colorBleedIntensityID, settings.colorBleedIntensity);
            material.SetFloat(chromaticShiftID, settings.chromaticShift);
            material.SetFloat(verticalBlurID, settings.verticalBlur);
            material.SetFloat(vhsTrackingID, settings.vhsTracking);
            material.SetFloat(signalNoiseID, settings.signalNoise);
            material.SetFloat(colorTemperatureID, settings.colorTemperature);

            material.SetFloat(pixelPerfectSnappingID, settings.pixelPerfectSnapping);
            material.SetFloat(paletteReductionID, settings.paletteReduction);
            material.SetFloat(lcdGhostingID, settings.lcdGhosting);
            material.SetFloat(pixelGridIntensityID, settings.pixelGridIntensity);
            material.SetFloat(pixelGridSizeID, settings.pixelGridSize);
            material.SetFloat(gameBoyModeID, settings.gameBoyMode);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("PSX Effect", out var passData))
            {
                passData.Source = source;
                passData.Destination = destination;
                passData.Material = material;

                builder.UseTexture(source);
                builder.SetRenderAttachment(destination, 0);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.Source, new Vector4(1, 1, 0, 0), data.Material, 0);
                });
            }

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("PSX Effect Copy Back", out var passData))
            {
                passData.Source = destination;
                passData.Destination = source;

                builder.UseTexture(destination);
                builder.SetRenderAttachment(source, 0);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.Source, new Vector4(1, 1, 0, 0), 0, false);
                });
            }
        }

        private class PassData
        {
            public TextureHandle Destination;
            public Material Material;
            public TextureHandle Source;
        }
    }
}