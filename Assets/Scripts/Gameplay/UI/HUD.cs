using System.Collections;
using System.Collections.Generic;
using Framework.Services.UI;
using Gameplay.Interaction.Interface;
using Gameplay.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI
{
    public class HUD : UIBase
    {
        #region 组件

        [SerializeField] private VerticalLayoutGroup interactionArea;
        [SerializeField] private Image energyBar;
        [SerializeField] private Button buttonPrefab;

        [SerializeField] private Color normalColor;
        [SerializeField] private Color selectedColor;
        [SerializeField] private Color rejectedColor = Color.red;

        private PlayerController _player;
        private readonly List<Button> _buttons = new();
        private Coroutine _rejectRoutine;

        #endregion

        #region 生命周期

        protected override void OnSetup(object args)
        {
            Unbind();

            _player = args as PlayerController;
            if (_player == null)
            {
                Debug.LogError("[HUD] OpenPanel 的参数不是 PlayerController", this);
                return;
            }

            Bind();
        }

        protected override void OnShow()
        {
            RefreshAll();
        }

        protected override void OnHide()
        {
            Unbind();
        }

        protected override void OnDestroyed()
        {
            Unbind();
        }

        #endregion

        #region 内部实现

        private void Bind()
        {
            _player.Interactor.OptionsChanged += OnOptionsChanged;
            _player.Interactor.HighlightChanged += OnHighlightChanged;
            _player.Interactor.InteractionRejected += OnRejected;
            _player.Energy.EnergyChanged += OnEnergyChanged;
        }

        private void Unbind()
        {
            if (_rejectRoutine != null)
            {
                StopCoroutine(_rejectRoutine);
                _rejectRoutine = null;
            }

            if (_player != null)
            {
                _player.Interactor.OptionsChanged -= OnOptionsChanged;
                _player.Interactor.HighlightChanged -= OnHighlightChanged;
                _player.Interactor.InteractionRejected -= OnRejected;
                _player.Energy.EnergyChanged -= OnEnergyChanged;
            }

            _player = null;
            ClearButtons();
        }

        private void RefreshAll()
        {
            if (_player == null) return;

            OnOptionsChanged(
                _player.Interactor.CurrentOptions,
                _player.Interactor.HighlightedIndex);

            OnEnergyChanged(_player.Energy.Current);
        }

        private void OnEnergyChanged(int current)
        {
            if (energyBar == null || _player == null) return;

            int capacity = _player.Energy.Capacity;
            energyBar.fillAmount = capacity > 0
                ? Mathf.Clamp01((float)current / capacity)
                : 0f;
        }

        private void OnOptionsChanged(
            IReadOnlyList<InteractionOption> options, int highlightedIndex)
        {
            ClearButtons();

            if (interactionArea == null) return;

            bool hasOptions = options != null && options.Count > 0;
            interactionArea.gameObject.SetActive(hasOptions);
            if (!hasOptions) return;

            for (int i = 0; i < options.Count; i++)
            {
                Button button = Instantiate(buttonPrefab, interactionArea.transform);
                button.transition = Selectable.Transition.None;

                var label = button.GetComponentInChildren<TMP_Text>();
                if (label != null)
                    label.text = options[i].Prompt;

                EnsureLayout(button);
                _buttons.Add(button);
            }

            ApplyHighlight(highlightedIndex);

            var areaRect = interactionArea.transform as RectTransform;
            if (areaRect != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(areaRect);
        }

        private void OnHighlightChanged(int highlightedIndex)
        {
            ApplyHighlight(highlightedIndex);
        }

        private void OnRejected(InteractionOption option)
        {
            if (_buttons.Count == 0) return;

            if (_rejectRoutine != null)
                StopCoroutine(_rejectRoutine);

            _rejectRoutine = StartCoroutine(RejectRoutine());
        }

        private IEnumerator RejectRoutine()
        {
            int index = _player != null ? _player.Interactor.HighlightedIndex : 0;
            index = Mathf.Clamp(index, 0, _buttons.Count - 1);

            var image = _buttons[index].targetGraphic as Image;
            if (image == null)
            {
                _rejectRoutine = null;
                yield break;
            }

            image.color = rejectedColor;
            yield return new WaitForSeconds(0.15f);

            if (index < _buttons.Count)
            {
                var current = _buttons[index].targetGraphic as Image;
                if (current != null)
                    current.color = selectedColor;
            }

            _rejectRoutine = null;
        }

        private void ApplyHighlight(int highlightedIndex)
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                var image = _buttons[i].targetGraphic as Image;
                if (image != null)
                    image.color = i == highlightedIndex ? selectedColor : normalColor;
            }
        }

        private static void EnsureLayout(Button button)
        {
            if (button.GetComponent<LayoutElement>() != null) return;

            var element = button.gameObject.AddComponent<LayoutElement>();
            element.preferredHeight = 100f;
        }

        private void ClearButtons()
        {
            if (interactionArea == null) return;

            for (int i = interactionArea.transform.childCount - 1; i >= 0; i--)
                Destroy(interactionArea.transform.GetChild(i).gameObject);

            _buttons.Clear();
        }

        #endregion
    }
}
