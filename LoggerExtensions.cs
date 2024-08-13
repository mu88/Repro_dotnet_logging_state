namespace WebApi;

public static partial class LoggerExtensions
{
    [LoggerMessage(
        EventId = 1001,
        EventName = nameof(Pets),
        Level = LogLevel.Information,
        Message = "---{Pets}---",
        SkipEnabledCheck = true)]
    public static partial void Pets(this ILogger logger, IEnumerable<string> pets);

    [LoggerMessage(
        EventId = 1002,
        EventName = nameof(PetsAndNationalitiesInSameOrder),
        Level = LogLevel.Information,
        Message = "---{Pets}---{Nationalities}---",
        SkipEnabledCheck = true)]
    public static partial void PetsAndNationalitiesInSameOrder(this ILogger logger, IEnumerable<string> pets, IEnumerable<string> nationalities);

    [LoggerMessage(
        EventId = 1003,
        EventName = nameof(NationalitiesAndPetsInDifferentOrder),
        Level = LogLevel.Information,
        Message = "---{Pets}---{Nationalities}---",
        SkipEnabledCheck = true)]
    public static partial void NationalitiesAndPetsInDifferentOrder(this ILogger logger, IEnumerable<string> nationalities, IEnumerable<string> pets);
}