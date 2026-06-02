using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Retro.PSXEffects.Retro_Shaders.Runtime.Scripts
{
    public class CrtRendererFeature : ScriptableRendererFeature
    {
        public CrtSettings settings = new();
        private CrtRenderPass crtPass;
        private Material material;

        public override void Create()
        {
            var shader = Shader.Find("Retro/CRTEffectURP");
            if (shader != null)
                material = CoreUtils.CreateEngineMaterial(shader);

            crtPass = new CrtRenderPass(material, settings)
            {
                renderPassEvent = settings.renderPassEvent
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (material == null || renderingData.cameraData.cameraType != CameraType.Game)
                return;

            crtPass.UpdateSettings(settings);
            renderer.EnqueuePass(crtPass);
        }

        protected override void Dispose(bool disposing)
        {
            if (material != null)
                CoreUtils.Destroy(material);
        }

        [Serializable]
        public class CrtSettings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

            [Header("Pixelation")]
            [Range(1, 20)]
            public float pixelSize = 4f;

            [Header("Scanlines")]
            [Range(0, 1)]
            public float scanlineIntensity = 0.3f;
            [Range(100, 1000)]
            public float scanlineCount = 300f;
            [Range(0, 0.05f)]
            public float scanlineRGBShift = 0.01f;

            [Header("Distortion")]
            [Range(0, 0.1f)]
            public float curvature = 0.02f;
            [Range(0, 0.02f)]
            public float chromaticAberration = 0.003f;

            [Header("Color")]
            [Range(0, 1)]
            public float vignette = 0.3f;
            [Range(0.5f, 1.5f)]
            public float brightness = 1f;

            [Header("RGB Phosphor")]
            [Range(0, 1)]
            public float phosphorIntensity;

            [Header("Flicker")]
            [Range(0, 1)]
            public float flickerIntensity;

            [Header("Rolling Scanline")]
            [Range(0, 1)]
            public float rollingScanlineIntensity;
            [Range(0.1f, 2f)]
            public float rollingScanlineSpeed = 0.5f;

            [Header("Glow")]
            [Range(0, 1)]
            public float glowIntensity;
            [Range(1, 10)]
            public float glowSpread = 3f;

            [Header("Static Noise")]
            [Range(0, 1)]
            public float noiseIntensity;

            [Header("Color Bleed")]
            [Range(0, 1)]
            public float colorBleedIntensity;

            [Header("Interlacing")]
            [Range(0, 1)]
            public float interlacingIntensity;
        }
    }

    public class CrtRenderPass : ScriptableRenderPass
    {
        private static readonly int pixelSizeID = Shader.PropertyToID("_PixelSize");
        private static readonly int scanlineIntensityID = Shader.PropertyToID("_ScanlineIntensity");
        private static readonly int scanlineCountID = Shader.PropertyToID("_ScanlineCount");
        private static readonly int curvatureID = Shader.PropertyToID("_Curvature");
        private static readonly int chromaticAberrationID = Shader.PropertyToID("_ChromaticAberration");
        private static readonly int vignetteID = Shader.PropertyToID("_Vignette");
        private static readonly int brightnessID = Shader.PropertyToID("_Brightness");
        private static readonly int phosphorIntensityID = Shader.PropertyToID("_PhosphorIntensity");
        private static readonly int flickerIntensityID = Shader.PropertyToID("_FlickerIntensity");
        private static readonly int rollingScanlineIntensityID = Shader.PropertyToID("_RollingScanlineIntensity");
        private static readonly int rollingScanlineSpeedID = Shader.PropertyToID("_RollingScanlineSpeed");
        private static readonly int glowIntensityID = Shader.PropertyToID("_GlowIntensity");
        private static readonly int glowSpreadID = Shader.PropertyToID("_GlowSpread");
        private static readonly int noiseIntensityID = Shader.PropertyToID("_NoiseIntensity");
        private static readonly int colorBleedIntensityID = Shader.PropertyToID("_ColorBleedIntensity");
        private static readonly int interlacingIntensityID = Shader.PropertyToID("_InterlacingIntensity");
        private static readonly int timeID = Shader.PropertyToID("_CRTTime");
        private static readonly int scanlineRGBShiftID = Shader.PropertyToID("_ScanlineRGBShift");

        private readonly Material material;
        private CrtRendererFeature.CrtSettings settings;

        public CrtRenderPass(Material material, CrtRendererFeature.CrtSettings settings)
        {
            this.material = material;
            this.settings = settings;
        }

        public void UpdateSettings(CrtRendererFeature.CrtSettings settings)
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
            destinationDesc.name = "_CRTTempTexture";
            destinationDesc.clearBuffer = false;

            var destination = renderGraph.CreateTexture(destinationDesc);

            material.SetFloat(pixelSizeID, settings.pixelSize);
            material.SetFloat(scanlineIntensityID, settings.scanlineIntensity);
            material.SetFloat(scanlineCountID, settings.scanlineCount);
            material.SetFloat(curvatureID, settings.curvature);
            material.SetFloat(chromaticAberrationID, settings.chromaticAberration);
            material.SetFloat(vignetteID, settings.vignette);
            material.SetFloat(brightnessID, settings.brightness);
            material.SetFloat(phosphorIntensityID, settings.phosphorIntensity);
            material.SetFloat(flickerIntensityID, settings.flickerIntensity);
            material.SetFloat(rollingScanlineIntensityID, settings.rollingScanlineIntensity);
            material.SetFloat(rollingScanlineSpeedID, settings.rollingScanlineSpeed);
            material.SetFloat(glowIntensityID, settings.glowIntensity);
            material.SetFloat(glowSpreadID, settings.glowSpread);
            material.SetFloat(noiseIntensityID, settings.noiseIntensity);
            material.SetFloat(colorBleedIntensityID, settings.colorBleedIntensity);
            material.SetFloat(interlacingIntensityID, settings.interlacingIntensity);
            material.SetFloat(timeID, Time.time);
            material.SetFloat(scanlineRGBShiftID, settings.scanlineRGBShift);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("CRT Effect", out var passData))
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

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("CRT Effect Copy Back", out var passData))
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