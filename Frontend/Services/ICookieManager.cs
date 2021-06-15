using LanguageExt;

namespace Frontend.Services
{
    public interface ICookieManager
    {
        Option<string> GetCookie();
        Unit SetCookie(string value);
    }
}