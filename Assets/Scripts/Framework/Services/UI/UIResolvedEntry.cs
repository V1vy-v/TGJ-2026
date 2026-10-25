namespace Framework.Services.UI
{
    public readonly struct UIResolvedEntry
    {
        public readonly UILayer Layer;
        public readonly UIBase Prefab;

        public UIResolvedEntry(UIBase prefab, UILayer layer)
        {
            Prefab = prefab;
            Layer = layer;
        }
    }
}