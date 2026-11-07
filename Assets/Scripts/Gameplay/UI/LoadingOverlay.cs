using System.Collections.Generic;
using Framework.Services.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI
{
    public class LoadingOverlay : UIBase
    {
        #region 控件

        [SerializeField] private float framesPerSecond = 2f;

        private readonly List<Image> _frames = new();
        private int _frameIndex;
        private float _frameTimer;

        #endregion

        #region 生命周期

        protected override void OnCreate()
        {
            base.OnCreate();
            CacheFrames();
            SetFrame(0);
        }

        protected override void OnShow()
        {
            base.OnShow();
            _frameIndex = 0;
            _frameTimer = 0f;
            SetFrame(0);
        }

        private void Update()
        {
            if (_frames.Count <= 1)
                return;

            _frameTimer += Time.unscaledDeltaTime;
            float interval = 1f / Mathf.Max(framesPerSecond, 0.01f);

            while (_frameTimer >= interval)
            {
                _frameTimer -= interval;
                _frameIndex = (_frameIndex + 1) % _frames.Count;
                SetFrame(_frameIndex);
            }
        }

        #endregion

        #region 内部实现

        private void CacheFrames()
        {
            _frames.Clear();

            foreach (var image in GetComponentsInChildren<Image>(true))
            {
                if (image.sprite != null && image.sprite.name.StartsWith("加载_"))
                    _frames.Add(image);
            }

            _frames.Sort((a, b) => string.CompareOrdinal(a.sprite.name, b.sprite.name));
        }

        private void SetFrame(int index)
        {
            for (int i = 0; i < _frames.Count; i++)
                _frames[i].gameObject.SetActive(i == index);
        }

        #endregion
    }
}
