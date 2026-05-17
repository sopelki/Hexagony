using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace Retro.PSXEffects
{
    public class PSXRendererFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class PSXSettings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

            [Header("Core PSX Settings")]
            [Tooltip("Color bit depth per channel. PS1 used ~5 bits (32 levels).")]
            [Range(2, 8)] public float psxColorDepth = 5f;

            [Tooltip("Ordered dithering intensity.")]
            [Range(0, 1)] public float psxDitherIntensity = 0.5f;

            [Tooltip("Additional color posterization.")]
            [Range(0, 1)] public float psxPosterization = 0.2f;

            [Tooltip("Resolution scale. 0.5 = PS1-like pixelation.")]
            [Range(0.1f, 1f)] public float psxResolutionScale = 0.5f;

            [Tooltip("Saturation boost.")]
            [Range(1f, 2f)] public float psxSaturationBoost = 1.2f;

            [Tooltip("Subtle darkening.")]
            [Range(0, 1)] public float psxDarkening = 0.1f;

            [Header("Analog/CRT Effects")]
            [Tooltip("NTSC color bleeding")]
            [Range(0, 1)] public float colorBleedIntensity = 0.1f;

            [Tooltip("Chromatic aberration")]
            [Range(0, 2)] public float chromaticShift = 0f;

            [Tooltip("Vertical blur")]
            [Range(0, 3)] public float verticalBlur = 0.5f;

            [Tooltip("VHS tracking errors")]
            [Range(0, 1)] public float vhsTracking = 0f;

            [Tooltip("Analog signal noise")]
            [Range(0, 0.5f)] public float signalNoise = 0.05f;

            [Tooltip("Color temperature")]
            [Range(-1, 1)] public float colorTemperature = 0.15f;

            [Header("2D Pixel Art")]
            [Tooltip("Snap to pixel grid")]
            [Range(0, 1)] public float pixelPerfectSnapping = 0.5f;

            [Tooltip("Reduce colors to palette")]
            [Range(0, 1)] public float paletteReduction = 0f;

            [Tooltip("LCD ghosting effect")]
            [Range(0, 1)] public float lcdGhosting = 0f;

            [Tooltip("Pixel grid intensity")]
            [Range(0, 1)] public float pixelGridIntensity = 0.1f;

            [Tooltip("Pixel size for grid")]
            [Range(1, 8)] public float pixelGridSize = 2f;

            [Tooltip("Game Boy mode")]
            [Range(0, 1)] public float gameBoyMode = 0f;
        }

        public PSXSettings settings = new PSXSettings();
        private PSXRenderPass renderPass;
        private Material material;

        public override void Create()
        {
            var shader = Shader.Find("Retro/PSXEffectURP");
            if (shader != null)
                material = CoreUtils.CreateEngineMaterial(shader);

            renderPass = new PSXRenderPass(material, settings);
            renderPass.renderPassEvent = settings.renderPassEvent;
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
    }

    public class PSXRenderPass : ScriptableRenderPass
    {
        private class PassData
        {
            public TextureHandle source;
            public TextureHandle destination;
            public Material material;
        }

        private Material material;
        private PSXRendererFeature.PSXSettings settings;

        private static readonly int PSXColorDepthID = Shader.PropertyToID("_PSXColorDepth");
        private static readonly int PSXDitherIntensityID = Shader.PropertyToID("_PSXDitherIntensity");
        private static readonly int PSXPosterizationID = Shader.PropertyToID("_PSXPosterization");
        private static readonly int PSXResolutionScaleID = Shader.PropertyToID("_PSXResolutionScale");
        private static readonly int PSXSaturationBoostID = Shader.PropertyToID("_PSXSaturationBoost");
        private static readonly int PSXDarkeningID = Shader.PropertyToID("_PSXDarkening");
        private static readonly int TimeID = Shader.PropertyToID("_CRTTime");

        private static readonly int ColorBleedIntensityID = Shader.PropertyToID("_ColorBleedIntensity");
        private static readonly int ChromaticShiftID = Shader.PropertyToID("_ChromaticShift");
        private static readonly int VerticalBlurID = Shader.PropertyToID("_VerticalBlur");
        private static readonly int VHSTrackingID = Shader.PropertyToID("_VHSTracking");
        private static readonly int SignalNoiseID = Shader.PropertyToID("_SignalNoise");
        private static readonly int ColorTemperatureID = Shader.PropertyToID("_ColorTemperature");

        private static readonly int PixelPerfectSnappingID = Shader.PropertyToID("_PixelPerfectSnapping");
        private static readonly int PaletteReductionID = Shader.PropertyToID("_PaletteReduction");
        private static readonly int LCDGhostingID = Shader.PropertyToID("_LCDGhosting");
        private static readonly int PixelGridIntensityID = Shader.PropertyToID("_PixelGridIntensity");
        private static readonly int PixelGridSizeID = Shader.PropertyToID("_PixelGridSize");
        private static readonly int GameBoyModeID = Shader.PropertyToID("_GameBoyMode");

        public PSXRenderPass(Material material, PSXRendererFeature.PSXSettings settings)
        {
            this.material = material;
            this.settings = settings;
        }

        public void UpdateSettings(PSXRendererFeature.PSXSettings settings)
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

            TextureHandle destination = renderGraph.CreateTexture(destinationDesc);

            // Set properties
            material.SetFloat(PSXColorDepthID, settings.psxColorDepth);
            material.SetFloat(PSXDitherIntensityID, settings.psxDitherIntensity);
            material.SetFloat(PSXPosterizationID, settings.psxPosterization);
            material.SetFloat(PSXResolutionScaleID, settings.psxResolutionScale);
            material.SetFloat(PSXSaturationBoostID, settings.psxSaturationBoost);
            material.SetFloat(PSXDarkeningID, settings.psxDarkening);
            material.SetFloat(TimeID, Time.realtimeSinceStartup);

            material.SetFloat(ColorBleedIntensityID, settings.colorBleedIntensity);
            material.SetFloat(ChromaticShiftID, settings.chromaticShift);
            material.SetFloat(VerticalBlurID, settings.verticalBlur);
            material.SetFloat(VHSTrackingID, settings.vhsTracking);
            material.SetFloat(SignalNoiseID, settings.signalNoise);
            material.SetFloat(ColorTemperatureID, settings.colorTemperature);

            material.SetFloat(PixelPerfectSnappingID, settings.pixelPerfectSnapping);
            material.SetFloat(PaletteReductionID, settings.paletteReduction);
            material.SetFloat(LCDGhostingID, settings.lcdGhosting);
            material.SetFloat(PixelGridIntensityID, settings.pixelGridIntensity);
            material.SetFloat(PixelGridSizeID, settings.pixelGridSize);
            material.SetFloat(GameBoyModeID, settings.gameBoyMode);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("PSX Effect", out var passData))
            {
                passData.source = source;
                passData.destination = destination;
                passData.material = material;

                builder.UseTexture(source);
                builder.SetRenderAttachment(destination, 0);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0);
                });
            }

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("PSX Effect Copy Back", out var passData))
            {
                passData.source = destination;
                passData.destination = source;

                builder.UseTexture(destination);
                builder.SetRenderAttachment(source, 0);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), 0, false);
                });
            }
        }
    }
}