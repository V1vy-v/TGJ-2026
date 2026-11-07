using Framework.Bootstrap;
using Framework.Services.Audio;
using Framework.Services.Localization;
using Framework.Services.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Gameplay.UI
{
    public class SettingPanel : UIBase
    {
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Button languageButton;
        [SerializeField] private Button backButton;
        [SerializeField] private TMP_Text languageLabel;
        
        private IAudioService _audio;
        private ILocalizationService _localization;

        protected override void OnCreate()
        {
            base.OnCreate();

            if (GameRoot.Instance != null)
            {
                GameRoot.Instance.Services.TryGet(out _audio);
                GameRoot.Instance.Services.TryGet(out _localization);
            }

            FindBindings();

            if (sfxSlider != null)
            {
                sfxSlider.value = 0.5f;
                sfxSlider.onValueChanged.AddListener(OnSfxChanged);
            }

            if (musicSlider != null)
            {
                musicSlider.value = 0.5f;
                musicSlider.onValueChanged.AddListener(OnMusicChanged);
            }

            _audio?.SetChannelVolume(AudioChannel.Sfx, 0.5f);
            _audio?.SetChannelVolume(AudioChannel.Bgm, 0.5f);

            if (languageButton != null)
                languageButton.onClick.AddListener(ToggleLanguage);

            if (backButton != null)
                backButton.onClick.AddListener(CloseSelf);

            if (_localization != null)
                _localization.OnLanguageChanged += OnLanguageChanged;

            UpdateLanguageLabel();
        }

        protected override void OnDestroyed()
        {
            if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
            if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
            if (languageButton != null) languageButton.onClick.RemoveListener(ToggleLanguage);
            if (backButton != null) backButton.onClick.RemoveListener(CloseSelf);

            if (_localization != null)
                _localization.OnLanguageChanged -= OnLanguageChanged;
        }

        protected override void OnShow()
        {
            base.OnShow();
            UpdateLanguageLabel();
        }

        protected override void OnHide()
        {
            base.OnHide();
        }

        private void Update()
        {
            if (Keyboard.current != null &&
                Keyboard.current.escapeKey.wasPressedThisFrame &&
                GameRoot.Instance != null)
            {
                GameRoot.Instance.Services.Get<IUIService>().Close<SettingPanel>();
            }
        }

        private void FindBindings()
        {
            if (sfxSlider == null)
                sfxSlider = FindSlider("SliderSFX");
            if (musicSlider == null)
                musicSlider = FindSlider("SliderMusic");
            if (languageButton == null)
                languageButton = FindButton("ButtonLanguage");
            if (backButton == null)
                backButton = FindButton("ButtonBack");

            if (languageButton != null && languageLabel == null)
                languageLabel = languageButton.GetComponentInChildren<TMP_Text>(true);
        }

        private Slider FindSlider(string objectName)
        {
            foreach (var slider in GetComponentsInChildren<Slider>(true))
            {
                if (slider.name == objectName)
                    return slider;
            }

            return null;
        }

        private Button FindButton(string objectName)
        {
            foreach (var button in GetComponentsInChildren<Button>(true))
            {
                if (button.name == objectName)
                    return button;
            }

            return null;
        }

        private void OnSfxChanged(float value) => _audio?.SetChannelVolume(AudioChannel.Sfx, value);

        private void OnMusicChanged(float value) => _audio?.SetChannelVolume(AudioChannel.Bgm, value);

        private void ToggleLanguage()
        {
            if (_localization == null)
                return;

            _localization.SetLanguage(
                _localization.Language == Language.ChineseSimple
                    ? Language.English
                    : Language.ChineseSimple);

            UpdateLanguageLabel();
        }

        private void UpdateLanguageLabel()
        {
            if (languageLabel == null || _localization == null)
                return;

            languageLabel.text = _localization.Language == Language.English
                ? "English"
                : "中文";
        }

        private void OnLanguageChanged()
        {
            UpdateLanguageLabel();
        }

        private void CloseSelf()
        {
            GameRoot.Instance.Services.Get<IUIService>().Close<SettingPanel>();
        }
    }
}
