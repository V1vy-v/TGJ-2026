using System;
using UnityEngine;

namespace Gameplay.Player.Pulse
{
    [Serializable]
    public class PulseSettings
    {
        [Header("时间")]
        [Tooltip("一次脉冲从开始到结束的持续时间（秒）。")]
        [SerializeField] private float duration = 2f;

        [Tooltip("两次脉冲之间的最小间隔（秒）。")]
        [SerializeField] private float cooldown = 2f;

        [Header("亮度")]
        [Tooltip("脉冲颜色；最终亮度还会乘以 Intensity。")]
        [SerializeField] private Color color = Color.white;

        [Tooltip("亮度倍率；越高脉冲越亮，过高会变成一片白。")]
        [SerializeField] private float intensity = 1f;

        [Header("距离")]
        [Tooltip("脉冲可见的最大世界距离（米）。")]
        [SerializeField] private float radius = 10f;

        [Tooltip("球壳厚度：0 表示从玩家附近开始渐隐，1 表示只在最外圈可见；越大实际可见范围越窄。")]
        [SerializeField, Range(0f, 1f)] private float hardness = 0.5f;

        [Tooltip("环的锐度：越高环越细、对比越强。")]
        [SerializeField] private float power = 2f;

        [Tooltip("环的密度：越高环越多、间距越小。")]
        [SerializeField] private float tiling = 0.5f;

        [Tooltip("环的扩散速度：越大扩散越快，但受 Duration 限制。")]
        [SerializeField] private float speed = 1f;

        public float Duration => duration;
        public float Cooldown => cooldown;
        public Color Color => color;
        public float Intensity => intensity;
        public float Radius => radius;
        public float Hardness => hardness;
        public float Power => power;
        public float Tiling => tiling;
        public float Speed => speed;
    }
}
