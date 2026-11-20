namespace Life.Application.Users;

public enum ChangePasswordOutcome
{
    Changed,
    CurrentPasswordIncorrect,
    NewPasswordSameAsCurrent,
    AccountUnavailable,
}
