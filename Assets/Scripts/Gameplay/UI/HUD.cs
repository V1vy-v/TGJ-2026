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
        [SerializeField] private GameObject energyTank;
        [SerializeField] private Image energy1;
        [SerializeField] private Image energy2;
        [SerializeField] private Image energy3;
        [SerializeField] private Image energy4;
        [SerializeField] private Image energy5;
        [SerializeField] private Image energy6;
        [SerializeField] private Button buttonPrefab;

        [SerializeField] private Color normalColor;
        [SerializeField] private Color selectedColor;
        [SerializeField] private Color rejectedColor = Color.red;

        private PlayerController _player;
        private readonly List<Button> _buttons = new();
        private Coroutine _rejectRoutine;
        private bool _hasKeyboard;

        #endregion

        #region 生命周期

        protected override void OnCreate()
        {
            base.OnCreate();
            FindEnergyBindings();
            SetEnergyVisible(false);
        }

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
            _player.KeyboardAcquired += OnKeyboardAcquired;
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
                _player.KeyboardAcquired -= OnKeyboardAcquired;
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

        private void OnKeyboardAcquired()
        {
            _hasKeyboard = true;
            SetEnergyVisible(true);
            OnEnergyChanged(_player != null ? _player.Energy.Current : 0);
        }

        private void OnEnergyChanged(int current)
        {
            if (!_hasKeyboard || _player == null)
            {
                SetEnergyVisible(false);
                return;
            }

            SetEnergyVisible(true);

            // 1~6 各占一格能量。
            SetCell(energy1, current >= 1);
            SetCell(energy2, current >= 2);
            SetCell(energy3, current >= 3);
            SetCell(energy4, current >= 4);
            SetCell(energy5, current >= 5);
            SetCell(energy6, current >= 6);
        }

        private void SetEnergyVisible(bool visible)
        {
            if (energyTank != null)
                energyTank.SetActive(visible);

            if (!visible)
            {
                SetCell(energy1, false);
                SetCell(energy2, false);
                SetCell(energy3, false);
                SetCell(energy4, false);
                SetCell(energy5, false);
                SetCell(energy6, false);
            }
        }

        private static void SetCell(Image image, bool visible)
        {
            if (image != null)
                image.gameObject.SetActive(visible);
        }

        private void FindEnergyBindings()
        {
            if (energyTank == null)
            {
                foreach (var child in GetComponentsInChildren<Transform>(true))
                {
                    if (child.name == "EnergyTank")
                    {
                        energyTank = child.gameObject;
                        break;
                    }
                }
            }

            if (energy1 == null) energy1 = FindImage("1");
            if (energy2 == null) energy2 = FindImage("2");
            if (energy3 == null) energy3 = FindImage("3");
            if (energy4 == null) energy4 = FindImage("4");
            if (energy5 == null) energy5 = FindImage("5");
            if (energy6 == null) energy6 = FindImage("6");
        }

        private Image FindImage(string objectName)
        {
            foreach (var image in GetComponentsInChildren<Image>(true))
            {
                if (image.name == objectName)
                    return image;
            }

            return null;
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
