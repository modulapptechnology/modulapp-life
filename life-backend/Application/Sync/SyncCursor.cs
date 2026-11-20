namespace Life.Application.Sync;

public static class SyncCursor
{
    public const int Length = 8;

    public static IComparer<byte[]> Comparer { get; } = new VersionComparer();

    public static bool TryParse(string? cursor, out byte[] version)
    {
        if (string.IsNullOrWhiteSpace(cursor))
        {
            version = new byte[Length];

            return true;
        }

        var buffer = new byte[Length];

        if (Convert.TryFromBase64String(cursor, buffer, out var written) && written == Length)
        {
            version = buffer;

            return true;
        }

        version = [];

        return false;
    }

    public static string Format(byte[] version)
    {
        ArgumentNullException.ThrowIfNull(version);

        return Convert.ToBase64String(version);
    }

    private sealed class VersionComparer : IComparer<byte[]>
    {
        public int Compare(byte[]? x, byte[]? y)
        {
            return x.AsSpan().SequenceCompareTo(y.AsSpan());
        }
    }
}
