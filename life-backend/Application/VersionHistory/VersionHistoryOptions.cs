namespace Life.Application.VersionHistory;

public sealed record VersionHistoryOptions(int RetentionDays = VersionHistoryOptions.DefaultRetentionDays)
{
    public const string SectionName = "VersionHistory";

    public const string RetentionDaysKey = "RetentionDays";

    public const int DefaultRetentionDays = 90;
}
