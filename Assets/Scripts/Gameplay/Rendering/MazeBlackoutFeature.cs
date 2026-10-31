using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Gameplay.Rendering
{
    public sealed class MazeBlackoutFeature : ScriptableRendererFeature
    {
        [SerializeField] private Material blackoutMaterial;
        [SerializeField] private RenderPassEvent injectionPoint =
            RenderPassEvent.AfterRenderingTransparents;

        private static bool _enabled;

        public static void SetEnabled(bool enabled)
        {
            _enabled = enabled;
        }

        private BlackoutPass _pass;

        public override void Create()
        {
            _pass = new BlackoutPass
            {
                renderPassEvent = injectionPoint,
                requiresIntermediateTexture = true,
            };
        }

        public override void AddRenderPasses(
            ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!_enabled || blackoutMaterial == null)
                return;

            if (renderingData.cameraData.cameraType != CameraType.Game)
                return;

            _pass.renderPassEvent = injectionPoint;
            _pass.Setup(blackoutMaterial);
            renderer.EnqueuePass(_pass);
        }

        private sealed class BlackoutPass : ScriptableRenderPass
        {
            private Material _material;

            public void Setup(Material material)
            {
                _material = material;
            }

            private sealed class PassData
            {
                public TextureHandle source;
                public Material material;
            }

            public override void RecordRenderGraph(
                RenderGraph renderGraph, ContextContainer frameData)
            {
                var resourceData = frameData.Get<UniversalResourceData>();
                var cameraData = frameData.Get<UniversalCameraData>();

                if (cameraData.cameraType != CameraType.Game)
                    return;

                if (resourceData.isActiveTargetBackBuffer)
                    return;

                TextureHandle source = resourceData.activeColorTexture;
                if (!source.IsValid())
                    return;

                var desc = renderGraph.GetTextureDesc(source);
                desc.name = "_MazeBlackoutTemp";
                desc.clearBuffer = false;
                TextureHandle temp = renderGraph.CreateTexture(desc);

                using (var builder = renderGraph.AddRasterRenderPass<PassData>(
                           "Maze Blackout", out var passData, profilingSampler))
                {
                    passData.source = source;
                    passData.material = _material;

                    builder.UseTexture(source, AccessFlags.Read);
                    builder.AllowPassCulling(false);
                    builder.SetRenderAttachment(temp, 0, AccessFlags.Write);

                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    {
                        Blitter.BlitTexture(
                            context.cmd,
                            data.source,
                            new Vector4(1f, 1f, 0f, 0f),
                            data.material,
                            0);
                    });
                }

                renderGraph.AddBlitPass(
                    temp, source, Vector2.one, Vector2.zero,
                    passName: "Maze Blackout Copy Back");
            }
        }
    }
}