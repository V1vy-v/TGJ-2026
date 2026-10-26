using UnityEngine;

namespace Gameplay.Player
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Gameplay/PlayerConfig")]
    public class PlayerConfig : ScriptableObject
    {
        [SerializeField] private float moveSpeed = 10;
        public float MoveSpeed => moveSpeed;
    }
}