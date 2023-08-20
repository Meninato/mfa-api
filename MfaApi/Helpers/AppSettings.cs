namespace MfaApi.Helpers;

public class AppSettings
{
    public string Secret { get; set; } = default!;

    // refresh token time to live (in days), inactive tokens are
    // automatically deleted from the database after this time
    public int RefreshTokenTTL { get; set; }

    public string EmailFrom { get; set; } = default!;
    public string SmtpHost { get; set; } = default!;
    public int SmtpPort { get; set; }
    public string SmtpUser { get; set; } = default!;
    public string SmtpPass { get; set; } = default!;
}
