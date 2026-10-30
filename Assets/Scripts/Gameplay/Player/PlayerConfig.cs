using UnityEngine;

namespace Gameplay.Player
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Gameplay/PlayerConfig")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("物理配置")]
        [SerializeField] [Header("移动")] private float moveSpeed = 10;
        [SerializeField] [Header("转身")] private float turnSpeed = 720;
        [SerializeField] [Header("重力")] private float gravity = 20f;
        [SerializeField] [Header("跳跃")] private float jumpHeight = 0.15f;
        
        [Header("能量")]
        [SerializeField] [Header("初始值")] private int initialEnergy = 0;
        [SerializeField] [Header("上限")] private int energyCapacity = 6;
        
        public float MoveSpeed => moveSpeed;
        public float TurnSpeed => turnSpeed;
        public float Gravity => gravity;
        public float JumpHeight => jumpHeight;
        public int InitialEnergy => initialEnergy;
        public int EnergyCapacity => energyCapacity;
    }
}