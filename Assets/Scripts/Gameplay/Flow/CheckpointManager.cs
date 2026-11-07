using System.Collections;
using System.Collections.Generic;
using Framework.Bootstrap;
using Gameplay.Player;
using UnityEngine;

namespace Gameplay.Flow
{
    /// <summary>
    /// 复活点管理：启动时缓存 [SpawnPoint] 下的 Transform，之后只查字典。
    /// 死亡判定只看玩家根节点的 Z 轴。
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class CheckpointManager : MonoBehaviour, ICheckpointService
    {
        [SerializeField] private float deathHeight = -1.5f;
        [SerializeField] private float duration = 3f;
        [SerializeField] private float fadeDuration = 0.5f;

        private readonly Dictionary<string, Transform> _spawns = new();
        private readonly Dictionary<string, int> _order = new()
        {
            { CheckpointIds.P0_1, 0 },
            { CheckpointIds.P0_2, 1 },
            { CheckpointIds.P1_1, 2 },
            { CheckpointIds.P1_1_AfterNpc1, 3 },
            { CheckpointIds.P1_1_After1_3, 4 },
            { CheckpointIds.P1_1_After1_4, 5 },
            { CheckpointIds.P2_1, 6 },
            { CheckpointIds.P2_1_After2_2, 7 },
            { CheckpointIds.P2_1_After2_3, 8 },
            { CheckpointIds.P2_1_AfterTeleport4, 9 },
            { CheckpointIds.P2_1_AfterTeleport5, 10 },
        };

        private PlayerController _player;
        private bool _respawning;
        private string _currentCheckpoint = CheckpointIds.P0_1;

        public string CurrentCheckpoint => _currentCheckpoint;

        private void Awake()
        {
            _player = GetComponent<PlayerController>();
            CacheSpawns();

            if (_spawns.Count == 0)
                return;

            if (GameRoot.Instance != null &&
                !GameRoot.Instance.Services.TryGet<ICheckpointService>(out _))
            {
                GameRoot.Instance.Services.Register<ICheckpointService>(this);
            }
        }

        private void Update()
        {
            if (_respawning || _player == null)
                return;

            if (_player.transform.position.y < deathHeight)
                Respawn();
        }

        public void Unlock(string checkpointId)
        {
            if (string.IsNullOrEmpty(checkpointId))
                return;

            if (!_spawns.ContainsKey(checkpointId))
            {
                Debug.LogWarning($"[Checkpoint] 未找到复活点: {checkpointId}", this);
                return;
            }

            if (!_order.TryGetValue(checkpointId, out int incoming))
                return;

            if (_order.TryGetValue(_currentCheckpoint, out int current) && incoming <= current)
                return;

            _currentCheckpoint = checkpointId;
        }

        public void Respawn()
        {
            if (_respawning || _player == null)
                return;

            if (!_spawns.TryGetValue(_currentCheckpoint, out Transform spawn) || spawn == null)
                return;

            StartCoroutine(RespawnRoutine(spawn));
        }

        private IEnumerator RespawnRoutine(Transform spawn)
        {
            _respawning = true;
            _player.Interactor?.Suspend();

            ScreenFadeController fade = ScreenFadeController.Instance;
            if (fade != null)
            {
                yield return fade.FadeOut(fadeDuration);

                float hold = Mathf.Max(0f, duration - fadeDuration * 2f);
                if (hold > 0f)
                    yield return new WaitForSecondsRealtime(hold);
            }

            _player.TeleportTo(spawn.position, spawn.rotation);

            if (fade != null)
                yield return fade.FadeIn(fadeDuration);

            _player.Interactor?.Resume();
            _respawning = false;
        }

        private void CacheSpawns()
        {
            var root = GameObject.Find("/[SceneRoot]/[SpawnPoint]");

            if (root == null)
            {
                foreach (var candidate in FindObjectsByType<Transform>(
                             FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (candidate.name != "[SpawnPoint]")
                        continue;

                    root = candidate.gameObject;
                    break;
                }
            }

            if (root == null)
                return;

            foreach (Transform child in root.transform)
                _spawns[child.name] = child;
        }
    }
}
