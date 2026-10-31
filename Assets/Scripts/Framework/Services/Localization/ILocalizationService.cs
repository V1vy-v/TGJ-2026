namespace Framework.Services.Localization
{
    public interface ILocalizationService
    {
        string Get(string key, string fallback = null);
        bool TryGet(string key, out string value);
    }
}