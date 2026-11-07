using Application.VersionHistory;

namespace Application.Tests.VersionHistory;

public class VersionHistoryOptionsTests
{
    [Fact]
    public void New_WithoutArguments_UsesNinetyDaysOfRetention()
    {
        var options = new VersionHistoryOptions();

        Assert.Equal(90, options.RetentionDays);
    }

    [Fact]
    public void New_WithRetentionDays_UsesTheGivenValue()
    {
        var options = new VersionHistoryOptions(30);

        Assert.Equal(30, options.RetentionDays);
    }
}
