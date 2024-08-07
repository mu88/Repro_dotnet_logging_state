namespace WebApi;

public static partial class LoggerExtensions
{
    [LoggerMessage(
        EventId = 1,
        EventName = nameof(Temperatures),
        Level = LogLevel.Information,
        Message = "---{Temperatures}---",
        SkipEnabledCheck = true)]
    public static partial void Temperatures(this ILogger logger, IEnumerable<string> temperatures);

    [LoggerMessage(
        EventId = 2,
        EventName = nameof(TemperaturesAndSummaries),
        Level = LogLevel.Information,
        Message = "---{Temperatures}---{Summaries}---",
        SkipEnabledCheck = true)]
    public static partial void TemperaturesAndSummaries(this ILogger logger, IEnumerable<string> temperatures, IEnumerable<string> summaries);
}