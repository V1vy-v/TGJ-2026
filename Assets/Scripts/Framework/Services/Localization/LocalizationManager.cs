using System;
using System.Collections.Generic;
using Framework.Core;
using UnityEngine;

namespace Framework.Services.Localization
{
    public class LocalizationManager : IManager, ILocalizationService
    {
        #region 状态

        public int Order => 15;
        private readonly List<LocalizationTable> _tables;
        private readonly Dictionary<string, string> _currentTexts = new();

        #endregion

        #region 生命周期

        public LocalizationManager(List<LocalizationTable> tables)
        {
            _tables = tables;
        }

        public void Initialize(ServiceRegistry services, MonoBehaviour host)
        {
            if (services == null || host == null)
            {
                Debug.LogError("[UIManager] services 或 host 为空");
                return;
            }

            if (_tables == null)
            {
                Debug.LogError("[UIManager] LocalizationTable 未配置");
                return;
            }

            SetLanguage(Language.English);
            RefreshTexts();

            services.Register<ILocalizationService>(this);
        }

        public void Shutdown()
        {
            _currentTexts.Clear();
        }

        #endregion

        #region 对外接口

        public Language Language { get; private set; }
        public event Action OnLanguageChanged;

        public string GetText(string key)
        {
            if (!_currentTexts.TryGetValue(key, out var text))
            {
                Debug.LogError($"[LocalizationManager] 未找到 {key} 对应文本");
                return string.Empty;
            }

            return text;
        }

        public void SetLanguage(Language language)
        {
            Language = language;
            RefreshTexts();
            OnLanguageChanged?.Invoke();
        }

        #endregion

        #region 内部实现

        private void RefreshTexts()
        {
            if (Language == Language.English)
            {
                foreach (var table in _tables)
                    foreach (var group in table.LocalizationGroups)
                        _currentTexts[group.Key] =
                            string.IsNullOrEmpty(group.TextEn) ? group.TextCh : group.TextEn;
            }
            else
            {
                foreach (var table in _tables)
                    foreach (var group in table.LocalizationGroups)
                        _currentTexts[group.Key] =
                            string.IsNullOrEmpty(group.TextCh) ? group.TextEn : group.TextCh;
            }
        }

        #endregion

    }
}