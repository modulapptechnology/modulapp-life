namespace Domain.Users;

public enum LoginOutcome
{
    Succeeded = 0,
    InvalidCredentials = 1,
    AccountLocked = 2,
    AccountInactive = 3,
}
