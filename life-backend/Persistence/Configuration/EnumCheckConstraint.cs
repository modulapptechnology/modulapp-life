namespace Life.Persistence.Configuration;

public static class EnumCheckConstraint
{
    public static string Name(string table, string column)
    {
        return $"CK_{table}_{column}";
    }

    public static string Sql<TEnum>(string column)
        where TEnum : struct, Enum
    {
        var values = Enum.GetValues<TEnum>().Select(value => Convert.ToInt32(value));

        return $"[{column}] IN ({string.Join(", ", values)})";
    }
}
