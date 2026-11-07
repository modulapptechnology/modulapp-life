namespace Application.Users;

public interface IVerificationTokenService
{
    VerificationToken Create();

    string Hash(string token);
}
