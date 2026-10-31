using Gameplay.Player.Pulse;
using UnityEngine;

namespace Gameplay.Player
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Gameplay/PlayerConfig")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("物理配置")]
        [SerializeField] private float moveSpeed = 10;
        [SerializeField] private float turnSpeed = 720;
        [SerializeField] private float gravity = 20f;
        [SerializeField] private float jumpHeight = 0.15f;
        
        [Header("能量")]
        [SerializeField] private int initialEnergy = 0;
        [SerializeField] private int energyCapacity = 6;
        
        [Header("视野")]
        [SerializeField] private VisionSettings visionSettings;

        [Header("脉冲")] 
        [SerializeField] private PulseSettings pulseSettings = new();
        
        public float MoveSpeed => moveSpeed;
        public float TurnSpeed => turnSpeed;
        public float Gravity => gravity;
        public float JumpHeight => jumpHeight;
        public int InitialEnergy => initialEnergy;
        public int EnergyCapacity => energyCapacity;
        public VisionSettings Vision => visionSettings;
        public PulseSettings Pulse => pulseSettings;
    }
}