using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Interaction.Puzzle.SpeakerChain
{
    /// <summary>
    /// 移动平台：被 Puzzle 一次性激活后，在起点和终点之间匀速折返，可携带乘客。
    /// 两个端点都使用本地坐标。
    /// </summary>
    public class SpeakerPlatform : MonoBehaviour
    {
        [Header("移动端点（本地坐标）")]
        [SerializeField] private Vector3 startPoint;
        [SerializeField] private Vector3 endPoint;

        [SerializeField] private float speed = 1f;

        [Tooltip("到端点后停留多久再反向")]
        [SerializeField] private float stopDuration = 2f;

        [Header("乘客检测")]
        [SerializeField] private Vector3 passengerCheckSize = new(3f, 2f, 3f);
        [SerializeField] private Vector3 passengerCheckOffset = new(0f, 1f, 0f);
        [SerializeField] private LayerMask passengerMask = ~0;

        private readonly Collider[] _hits = new Collider[8];
        private readonly List<CharacterController> _passengers = new();

        private bool _unlocked;
        private bool _movingToEnd = true;
        private bool _waiting;
        private float _waitTimer;

        public void Unlock()
        {
            _unlocked = true;
        }

        private void Update()
        {
            if (!_unlocked)
                return;

            if (_waiting)
            {
                _waitTimer -= Time.deltaTime;
                if (_waitTimer > 0f)
                    return;

                _waiting = false;
                _movingToEnd = !_movingToEnd;
            }

            // 先记录当前站在平台上的乘客，再移动平台。
            // 这样水平移动时，不会因为平台先移走而检测不到玩家。
            CollectPassengers();

            Vector3 beforeWorld = transform.position;
            Vector3 beforeLocal = transform.localPosition;
            Vector3 targetLocal = _movingToEnd ? endPoint : startPoint;

            Vector3 nextLocal = Vector3.MoveTowards(
                beforeLocal,
                targetLocal,
                speed * Time.deltaTime);

            transform.localPosition = nextLocal;

            if ((nextLocal - targetLocal).sqrMagnitude < 0.000001f)
            {
                // 到端点后先停住，等待 stopDuration，再反向。
                _waiting = true;
                _waitTimer = stopDuration;
            }

            Vector3 delta = transform.position - beforeWorld;
            if (delta.sqrMagnitude > 0f)
                MovePassengers(delta);
        }

        private void CollectPassengers()
        {
            int count = Physics.OverlapBoxNonAlloc(
                transform.TransformPoint(passengerCheckOffset),
                passengerCheckSize * 0.5f,
                _hits,
                transform.rotation,
                passengerMask,
                QueryTriggerInteraction.Ignore);

            _passengers.Clear();

            for (int i = 0; i < count; i++)
            {
                var controller = _hits[i].GetComponentInParent<CharacterController>();
                if (controller == null) continue;
                if (_passengers.Contains(controller)) continue;

                _passengers.Add(controller);
            }
        }

        private void MovePassengers(Vector3 delta)
        {
            for (int i = 0; i < _passengers.Count; i++)
            {
                CharacterController controller = _passengers[i];
                if (controller == null)
                    continue;

                Vector3 before = controller.transform.position;
                controller.Move(delta);

                // CharacterController 在平台边缘可能被平台自身 Collider 挡住，
                // 导致水平方向只被平台推着走。把剩余位移补上。
                Vector3 moved = controller.transform.position - before;
                Vector3 remaining = delta - moved;
                if (remaining.sqrMagnitude < 0.000001f)
                    continue;

                controller.enabled = false;
                controller.transform.position += remaining;
                controller.enabled = true;
            }
        }
    }
}
