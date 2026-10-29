using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

/// <summary>
/// URP 17 (Render Graph) full-screen scan effect.
/// Wrap of "Mirza Beig/Post-Processing Scan" material: reads camera colour + depth,
/// renders the scan overlay into a temporary texture, then copies it back.
/// The effect does not run continuously: gameplay code calls <see cref="TriggerPulse(Vector3)"/>.
/// </summary>
public sealed class ScanRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private Material scanMaterial;
    [SerializeField] private RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingTransparents;
    [SerializeField] private bool renderInSceneView;

    [Tooltip("扫描原点。留空时使用触发瞬间的相机位置。")]
    [SerializeField] private Transform scanOrigin;

    [Tooltip("一次脉冲持续多少秒。")]
    [SerializeField] private float pulseDuration = 1.2f;

    [Tooltip("调试用：勾上后持续播放，不看玩家触发。")]
    [SerializeField] private bool previewContinuous;

    private static readonly int OriginId = Shader.PropertyToID("_Origin");
    private static readonly int ScanTimeId = Shader.PropertyToID("_ScanTime");

    private ScanPass _scanPass;
    private float _pulseStartTime = -1f;
    private Vector3 _pulseOrigin;

    /// <summary>Last created instance, so gameplay code can trigger it without a scene reference.</summary>
    public static ScanRendererFeature Instance { get; private set; }

    /// <summary>Master switch (e.g. disable while the player does not have the glasses).</summary>
    public bool EffectEnabled { get; set; } = true;

    public bool IsPulsing => _pulseStartTime >= 0f;

    public float PulseDuration
    {
        get => pulseDuration;
        set => pulseDuration = Mathf.Max(0.01f, value);
    }

    /// <summary>Fire one pulse from a world position.</summary>
    public void TriggerPulse(Vector3 worldOrigin)
    {
        _pulseOrigin = worldOrigin;
        _pulseStartTime = Time.time;
    }

    /// <summary>Fire one pulse from the Scan Origin transform, or from the camera if none is set.</summary>
    public void TriggerPulse()
    {
        TriggerPulse(scanOrigin != null ? scanOrigin.position : CameraFallback());
    }

    public void StopPulse()
    {
        _pulseStartTime = -1f;
        if (scanMaterial != null)
            scanMaterial.SetFloat(ScanTimeId, 0f);
    }

    private static Vector3 CameraFallback()
    {
        var camera = Camera.main;
        return camera != null ? camera.transform.position : Vector3.zero;
    }

    public override void Create()
    {
        _scanPass = new ScanPass
        {
            renderPassEvent = injectionPoint,
            requiresIntermediateTexture = true,
        };

        // 编辑器里重新创建 Feature 时清掉上次留下的脉冲状态。
        _pulseStartTime = -1f;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        Instance = this;

        if (scanMaterial == null || !EffectEnabled)
            return;

        var cameraType = renderingData.cameraData.cameraType;
        bool isGameCamera = cameraType == CameraType.Game;
        bool isSceneView = renderInSceneView && cameraType == CameraType.SceneView;

        if (!isGameCamera && !isSceneView)
            return;

        // 编辑模式下不自动播放脉冲：只有 Preview Continuous（调试）或 Play 模式才渲染。
        if (!previewContinuous && !Application.isPlaying)
            return;

        float scanTime;
        Vector3 origin;

        if (previewContinuous)
        {
            scanTime = Time.time;
            origin = scanOrigin != null ? scanOrigin.position : renderingData.cameraData.camera.transform.position;
        }
        else
        {
            if (_pulseStartTime < 0f)
                return;

            scanTime = Time.time - _pulseStartTime;
            if (scanTime > pulseDuration)
            {
                _pulseStartTime = -1f;
                scanMaterial.SetFloat(ScanTimeId, 0f);
                return;
            }

            origin = _pulseOrigin;
        }

        scanMaterial.SetVector(OriginId, origin);
        scanMaterial.SetFloat(ScanTimeId, scanTime);

        _scanPass.renderPassEvent = injectionPoint;
        _scanPass.Setup(scanMaterial);
        renderer.EnqueuePass(_scanPass);
    }

    private sealed class ScanPass : ScriptableRenderPass
    {
        private Material _material;

        public void Setup(Material material)
        {
            _material = material;
        }

        private sealed class BlitPassData
        {
            public TextureHandle source;
            public Material material;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resourceData = frameData.Get<UniversalResourceData>();
            var cameraData = frameData.Get<UniversalCameraData>();

            if (cameraData.cameraType == CameraType.Preview || cameraData.cameraType == CameraType.Reflection)
                return;

            if (resourceData.isActiveTargetBackBuffer)
                return;

            TextureHandle source = resourceData.activeColorTexture;
            if (!source.IsValid())
                return;

            var tempDesc = renderGraph.GetTextureDesc(source);
            tempDesc.name = "_ScanTemp";
            tempDesc.clearBuffer = false;

            TextureHandle temp = renderGraph.CreateTexture(tempDesc);

            using (var builder = renderGraph.AddRasterRenderPass<BlitPassData>("Scan Effect", out var passData, profilingSampler))
            {
                passData.source = source;
                passData.material = _material;

                builder.UseTexture(source, AccessFlags.Read);
                if (resourceData.cameraDepthTexture.IsValid())
                    builder.UseTexture(resourceData.cameraDepthTexture, AccessFlags.Read);

                builder.UseAllGlobalTextures(true);
                builder.AllowPassCulling(false);
                builder.SetRenderAttachment(temp, 0, AccessFlags.Write);

                builder.SetRenderFunc((BlitPassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.source, new Vector4(1f, 1f, 0f, 0f), data.material, 0);
                });
            }

            renderGraph.AddBlitPass(temp, source, Vector2.one, Vector2.zero, passName: "Scan Copy Back");
        }
    }
}
