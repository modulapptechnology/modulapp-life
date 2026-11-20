namespace Life.Infrastructure.Email;

public static class EmailAddressMask
{
    public const string Hidden = "***";

    public static string Mask(string? address)
    {
        var separator = address?.IndexOf('@') ?? -1;

        if (address is null || separator < 1)
        {
            return Hidden;
        }

        return $"{address[0]}{Hidden}{address[separator..]}";
    }
}
