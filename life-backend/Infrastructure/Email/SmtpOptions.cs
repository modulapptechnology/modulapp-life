namespace Infrastructure.Email;

public sealed record SmtpOptions(
    string Host,
    int Port,
    bool EnableSsl,
    string UserName,
    string Password,
    string FromAddress)
{
    public const string SectionName = "Email:Smtp";

    public const int DefaultPort = 587;
}
