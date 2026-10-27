using UnityEngine;

namespace Gameplay.Player
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Gameplay/PlayerConfig")]
    public class PlayerConfig : ScriptableObject
    {
        [SerializeField] [Header("移动")] private float moveSpeed = 10;
        [SerializeField] [Header("转身")] private float turnSpeed = 720;
        [SerializeField][Header("重力")] private float gravity = 20f;
        [SerializeField][Header("跳跃")] private float jumpHeight = 0.15f;
        public float MoveSpeed => moveSpeed;
        public float TurnSpeed => turnSpeed;
        public float Gravity => gravity;
        public float JumpHeight => jumpHeight;
    }
}