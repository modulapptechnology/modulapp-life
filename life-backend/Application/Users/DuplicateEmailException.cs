namespace Application.Users;

public sealed class DuplicateEmailException(Exception innerException)
    : Exception("The email is already registered.", innerException);
