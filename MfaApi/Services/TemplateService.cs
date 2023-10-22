using MfaApi.Entities;

namespace MfaApi.Services;

public interface ITemplateService
{
    string GetForgotPassword(Account account, string url);
}

public class TemplateService : ITemplateService
{
    public string GetForgotPassword(Account account, string url)
    {
        using StreamReader reader = new StreamReader("");
    }
}
