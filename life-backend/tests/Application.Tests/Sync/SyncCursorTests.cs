using Life.Application.Sync;

namespace Life.Application.Tests.Sync;

public class SyncCursorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryParse_WithoutCursor_StartsFromTheBeginning(string? cursor)
    {
        var parsed = SyncCursor.TryParse(cursor, out var version);

        Assert.True(parsed);
        Assert.Equal(new byte[SyncCursor.Length], version);
    }

    [Fact]
    public void TryParse_WithAFormattedVersion_ReturnsTheSameVersion()
    {
        byte[] original = [0, 0, 0, 0, 0, 0, 1, 255];
        var cursor = SyncCursor.Format(original);

        var parsed = SyncCursor.TryParse(cursor, out var version);

        Assert.True(parsed);
        Assert.Equal(original, version);
    }

    [Theory]
    [InlineData("not-base64!")]
    [InlineData("AQID")]
    public void TryParse_WithAnInvalidCursor_ReturnsFalse(string cursor)
    {
        var parsed = SyncCursor.TryParse(cursor, out _);

        Assert.False(parsed);
    }

    [Fact]
    public void Comparer_WithTwoVersions_OrdersThemByTheirBytes()
    {
        byte[] lower = [0, 0, 0, 0, 0, 0, 0, 9];
        byte[] higher = [0, 0, 0, 0, 0, 0, 1, 0];

        var result = SyncCursor.Comparer.Compare(lower, higher);

        Assert.True(result < 0);
    }
}
