using System.Collections;
using Framework.Bootstrap;
using Framework.Services.Localization;
using Framework.Services.UI;
using Gameplay.Player;
using TMPro;
using UnityEngine;

namespace Gameplay.UI
{
    public class StoryPopup : UIBase
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private TMP_Text tips;
        [SerializeField, Min(1f)] private float charsPerSecond = 5f;

        private const string PressAnyKey = "ui.pressAnyKey";

        private PlayerController _player;
        private PlayerDialogue _dialogue;
        private ILocalizationService _localization;

        private Coroutine _typeRoutine;
        private bool _isTyping;
        private int _visibleCharacters;
        private int _totalCharacters;

        protected override void OnCreate()
        {
            base.OnCreate();

            if (text == null)
                text = GetComponentInChildren<TMP_Text>(true);

            if (tips == null)
                tips = FindTips();
        }

        private TMP_Text FindTips()
        {
            foreach (var candidate in GetComponentsInChildren<TMP_Text>(true))
            {
                if (candidate.name == "Tips")
                    return candidate;
            }

            return null;
        }

        protected override void OnSetup(object args)
        {
            Unbind();

            _player = args as PlayerController;
            if (_player == null || _player.Dialogue == null)
            {
                Debug.LogError("[StoryPopup] 参数不是 PlayerController");
                return;
            }

            _dialogue = _player.Dialogue;
            _localization = GameRoot.Instance.Services.Get<ILocalizationService>();
            UpdateTipsText();

            _player.DialogueAdvanceRequested += OnAdvanceRequested;
            _dialogue.LineChanged += ShowLine;
            _dialogue.Ended += OnDialogueEnded;
            _localization.OnLanguageChanged += OnLanguageChanged;
        }

        protected override void OnShow()
        {
            if (_dialogue == null)
                return;

            ShowLine(_dialogue.CurrentLine);
        }

        protected override void OnHide()
        {
            Unbind();
        }

        protected override void OnDestroyed()
        {
            Unbind();
        }

        private void Unbind()
        {
            if (_typeRoutine != null)
            {
                StopCoroutine(_typeRoutine);
                _typeRoutine = null;
            }

            _isTyping = false;

            if (_player != null)
                _player.DialogueAdvanceRequested -= OnAdvanceRequested;

            if (_dialogue != null)
                _dialogue.LineChanged -= ShowLine;

            if (_dialogue != null)
                _dialogue.Ended -= OnDialogueEnded;

            if (_localization != null)
                _localization.OnLanguageChanged -= OnLanguageChanged;

            _player = null;
            _dialogue = null;
            _localization = null;
        }

        private void ShowLine(PlayerDialogue.Line line)
        {
            if (text == null || _localization == null)
                return;

            text.text = _localization.GetText(line.TextKey);
            text.ForceMeshUpdate();

            _totalCharacters = text.textInfo.characterCount;

            if (line.Stream && charsPerSecond > 0f && _totalCharacters > 0)
                StartTyping();
            else
                ShowAll();
        }

        private void StartTyping()
        {
            if (_typeRoutine != null)
                StopCoroutine(_typeRoutine);

            _visibleCharacters = 0;
            _isTyping = true;
            text.maxVisibleCharacters = 0;
            _typeRoutine = StartCoroutine(TypeRoutine());
        }

        private IEnumerator TypeRoutine()
        {
            while (_visibleCharacters < _totalCharacters)
            {
                _visibleCharacters += Mathf.Max(
                    1,
                    Mathf.RoundToInt(charsPerSecond * Time.unscaledDeltaTime));

                text.maxVisibleCharacters =
                    Mathf.Min(_visibleCharacters, _totalCharacters);

                yield return null;
            }

            ShowAll();
        }

        private void ShowAll()
        {
            if (_typeRoutine != null)
            {
                StopCoroutine(_typeRoutine);
                _typeRoutine = null;
            }

            if (text != null)
                text.maxVisibleCharacters = _totalCharacters;

            _isTyping = false;
        }

        private void OnAdvanceRequested()
        {
            if (_dialogue == null)
                return;

            if (_isTyping)
            {
                ShowAll();
                return;
            }

            _dialogue.Advance();
        }

        private void OnDialogueEnded()
        {
            Hide();
        }

        private void OnLanguageChanged()
        {
            if (_dialogue != null)
                ShowLine(_dialogue.CurrentLine);

            UpdateTipsText();
        }

        private void UpdateTipsText()
        {
            if (tips == null || _localization == null)
                return;

            tips.text = _localization.GetText(PressAnyKey);
        }
    }
}
