using System;

namespace Framework.Services.UI
{
    public interface IUIService
    {
        event Action<UIBase> Opened;
        event Action<UIBase> Closed;
        
        bool IsOpen<T>() where T : UIBase;

        void OpenScreen<T>(object args = null) where T : UIBase;
        void OpenPanel<T>(object args = null) where T : UIBase;
        void OpenPopup<T>(object args = null, Action onClosed = null) where T : UIBase;

        void Close<T>() where T : UIBase;
        void CloseTop(UILayer layer);
        void CloseAll();
    }
}
