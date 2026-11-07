using System.Collections;
using System.Collections.Generic;
using Framework.Bootstrap;
using Framework.Services.Localization;
using Gameplay.Audio;
using Gameplay.Flow;
using Gameplay.Interaction.Interface;
using Gameplay.Player;
using Gameplay.Player.Interaction;
using UnityEngine;

namespace Gameplay.Interaction.NPC
{
    public class Npc : InteractableBehaviour
    {
        [SerializeField] private TextAsset dialogueScript;

        [Tooltip("对话结束后播放的剧情，可留空")]
        [SerializeField] private TextAsset storyAfterDialogue;

        [Tooltip("对话结束后自动播放的 Tips key，可留空")]
        [SerializeField] private string tipsAfterDialogue;

        [Tooltip("对话结束后解锁的复活点 ID，可留空")]
        [SerializeField] private string checkpointOnDialogueEnd;

        [Header("问答选项（可选）")]
        [Tooltip("问答题目 key；留空则不启用问答")]
        [SerializeField] private string quizQuestionKey;
        [SerializeField] private string quizOptionAKey;
        [SerializeField] private string quizOptionBKey;
        [SerializeField] private int quizCorrectOption;
        [SerializeField] private string quizCorrectKey;
        [SerializeField] private string quizWrongKey;

        private PlayerController _watchedPlayer;
        private PlayerController _quizPlayer;
        private bool _quizActive;
        private bool _quizSolved;

        public override IReadOnlyList<InteractionOption> GetOptions(PlayerContext ctx)
        {
            if (_quizActive && !_quizSolved)
            {
                return new[]
                {
                    Option(GetLocalized(quizOptionAKey), ctx => Answer(0, ctx)),
                    Option(GetLocalized(quizOptionBKey), ctx => Answer(1, ctx)),
                };
            }

            if (dialogueScript == null || ctx.StartDialogue == null || _quizSolved)
                return System.Array.Empty<InteractionOption>();

            string prompt =
                GameRoot.Instance.Services.Get<ILocalizationService>().
                    GetText("interaction.talk");

            return SingleOption(
                prompt,
                _ => StartDialogue(ctx));
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private bool StartDialogue(PlayerContext ctx)
        {
            if (!ctx.StartDialogue(dialogueScript))
                return false;

            if (ctx.Player == null ||
                (storyAfterDialogue == null &&
                 string.IsNullOrWhiteSpace(tipsAfterDialogue) &&
                 string.IsNullOrWhiteSpace(checkpointOnDialogueEnd) &&
                 string.IsNullOrWhiteSpace(quizQuestionKey)))
                return true;

            Unsubscribe();
            _watchedPlayer = ctx.Player;
            _quizPlayer = ctx.Player;
            _watchedPlayer.Dialogue.Ended += OnDialogueEnded;
            return true;
        }

        private void OnDialogueEnded()
        {
            PlayerController player = _watchedPlayer;
            Unsubscribe();

            if (!string.IsNullOrWhiteSpace(checkpointOnDialogueEnd) &&
                GameRoot.Instance.Services.TryGet<ICheckpointService>(out var checkpoints))
            {
                checkpoints.Unlock(checkpointOnDialogueEnd);
            }

            if (!string.IsNullOrWhiteSpace(tipsAfterDialogue))
                TipsTrigger.Show(tipsAfterDialogue);

            if (player != null && storyAfterDialogue != null)
                StartCoroutine(StartStoryNextFrame(player));

            if (!string.IsNullOrWhiteSpace(quizQuestionKey))
            {
                _quizActive = true;
            }
        }

        private bool Answer(int index, PlayerContext ctx)
        {
            if (!_quizActive || _quizSolved)
                return false;

            if (index == quizCorrectOption)
            {
                _quizActive = false;
                _quizSolved = true;

                PlayerEnergy energy = _quizPlayer != null
                    ? _quizPlayer.Energy
                    : ctx.Energy;

                if (energy != null && energy.TryRecycle(1))
                {
                    if (GameRoot.Instance.Services.TryGet<IGameAudio>(out var audio))
                        audio.PlaySfx(GameSfx.EnergyInject);
                }

                TipsTrigger.Show(quizCorrectKey);
                return true;
            }

            TipsTrigger.Show(quizWrongKey);
            return true;
        }

        private string GetLocalized(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return string.Empty;

            return GameRoot.Instance.Services.Get<ILocalizationService>().GetText(key);
        }

        private IEnumerator StartStoryNextFrame(PlayerController player)
        {
            // 等当前 DialoguePopup 关闭后再开下一段。
            yield return null;

            if (player != null)
                player.StartStory(storyAfterDialogue);
        }

        private void Unsubscribe()
        {
            if (_watchedPlayer == null)
                return;

            _watchedPlayer.Dialogue.Ended -= OnDialogueEnded;
            _watchedPlayer = null;
        }
    }
}
