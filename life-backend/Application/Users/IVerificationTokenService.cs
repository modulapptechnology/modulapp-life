namespace Application.Users;

public interface IVerificationTokenService
{
    VerificationToken Create();

    VerificationToken CreateCode();

    string Hash(string token);
}
