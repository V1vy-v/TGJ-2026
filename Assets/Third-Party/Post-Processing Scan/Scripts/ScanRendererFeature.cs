using Gameplay.Player.Pulse;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

/// <summary>
/// URP 17 (Render Graph) full-screen scan effect.
/// 只负责渲染：持续时间、颜色、半径等参数由 PlayerPulse 通过 <see cref="Play"/> 传入。
/// </summary>
public sealed class ScanRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private Material scanMaterial;
    [SerializeField] private RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingTransparents;
    [SerializeField] private bool renderInSceneView;

    [Tooltip("调试用：勾上后持续播放，不看玩家触发。")]
    [SerializeField] private bool previewContinuous;

    private static readonly int OriginId = Shader.PropertyToID("_Origin");
    private static readonly int ScanTimeId = Shader.PropertyToID("_ScanTime");
    private static readonly int ColourId = Shader.PropertyToID("_Colour");
    private static readonly int PowerId = Shader.PropertyToID("_Power");
    private static readonly int TilingId = Shader.PropertyToID("_Tiling");
    private static readonly int SpeedId = Shader.PropertyToID("_Speed");
    private static readonly int RadiusId = Shader.PropertyToID("_MaskRadius");
    private static readonly int HardnessId = Shader.PropertyToID("_MaskHardness");
    private static readonly int MaskPowerId = Shader.PropertyToID("_MaskPower");
    private static readonly int MultiplyBlendId = Shader.PropertyToID("_MultiplyBlend");

    private ScanPass _scanPass;
    private float _pulseStartTime = -1f;
    private float _pulseDuration = 2f;
    private Vector3 _pulseOrigin;

    private Material _runtimeMaterial;
    private Material _runtimeSource;

    /// <summary>Last created instance, so gameplay code can trigger it without a scene reference.</summary>
    public static ScanRendererFeature Instance { get; private set; }

    /// <summary>Master switch (e.g. disable while the player does not have the glasses).</summary>
    public bool EffectEnabled { get; set; } = true;

    public bool IsPulsing => _pulseStartTime >= 0f;

    /// <summary>
    /// 播放一次脉冲。持续时间、亮度、距离等全部来自玩家侧配置。
    /// </summary>
    public void Play(Vector3 worldOrigin, PulseSettings settings)
    {
        if (settings == null) return;

        Material material = GetRuntimeMaterial();
        if (material == null) return;

        _pulseOrigin = worldOrigin;
        _pulseDuration = Mathf.Max(0.01f, settings.Duration);
        _pulseStartTime = Time.time;

        ApplySettings(material, settings);
    }

    public void StopPulse()
    {
        _pulseStartTime = -1f;

        if (_runtimeMaterial != null)
            _runtimeMaterial.SetFloat(ScanTimeId, 0f);
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
        _runtimeMaterial = null;
        _runtimeSource = null;
    }

    public override void AddRenderPasses(
        ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        Instance = this;

        if (!EffectEnabled)
            return;

        Material material = GetRuntimeMaterial();
        if (material == null)
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
            origin = renderingData.cameraData.camera.transform.position;
        }
        else
        {
            if (_pulseStartTime < 0f)
                return;

            scanTime = Time.time - _pulseStartTime;
            if (scanTime > _pulseDuration)
            {
                _pulseStartTime = -1f;
                material.SetFloat(ScanTimeId, 0f);
                return;
            }

            origin = _pulseOrigin;
        }

        material.SetVector(OriginId, origin);
        material.SetFloat(ScanTimeId, scanTime);

        _scanPass.renderPassEvent = injectionPoint;
        _scanPass.Setup(material);
        renderer.EnqueuePass(_scanPass);
    }

    private Material GetRuntimeMaterial()
    {
        if (scanMaterial == null)
            return null;

        if (_runtimeMaterial == null || _runtimeSource != scanMaterial)
        {
            _runtimeMaterial = new Material(scanMaterial);
            _runtimeSource = scanMaterial;
        }

        return _runtimeMaterial;
    }

    private static void ApplySettings(Material material, PulseSettings settings)
    {
        Color color = settings.Color;
        color.r *= settings.Intensity;
        color.g *= settings.Intensity;
        color.b *= settings.Intensity;

        material.SetColor(ColourId, color);
        material.SetFloat(PowerId, settings.Power);
        material.SetFloat(TilingId, settings.Tiling);
        material.SetFloat(SpeedId, settings.Speed);
        material.SetFloat(RadiusId, settings.Radius);
        material.SetFloat(HardnessId, settings.Hardness);

        // 这两个暂不做成玩家参数，保持扫描环的硬边和叠加模式。
        material.SetFloat(MaskPowerId, 10f);
        material.SetFloat(MultiplyBlendId, 0f);
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

        public override void RecordRenderGraph(
            RenderGraph renderGraph, ContextContainer frameData)
        {
            var resourceData = frameData.Get<UniversalResourceData>();
            var cameraData = frameData.Get<UniversalCameraData>();

            if (cameraData.cameraType == CameraType.Preview ||
                cameraData.cameraType == CameraType.Reflection)
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

            using (var builder = renderGraph.AddRasterRenderPass<BlitPassData>(
                       "Scan Effect", out var passData, profilingSampler))
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
                    Blitter.BlitTexture(
                        context.cmd,
                        data.source,
                        new Vector4(1f, 1f, 0f, 0f),
                        data.material,
                        0);
                });
            }

            renderGraph.AddBlitPass(
                temp, source, Vector2.one, Vector2.zero, passName: "Scan Copy Back");
        }
    }
}
