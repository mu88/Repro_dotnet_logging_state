using Microsoft.Extensions.Logging.Console;

namespace WebApi;

public sealed class CustomServiceRuntimeContractFormatterOptions : SimpleConsoleFormatterOptions
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CustomServiceRuntimeContractFormatterOptions" /> class.
    /// </summary>
    public CustomServiceRuntimeContractFormatterOptions()
    {
        IncludeScopes = true;
        SingleLine = true;
        TimestampFormat = "yyyy-MM-dd HH:mm:ss.fffzzz";
        UseUtcTimestamp = true;
        base.ColorBehavior = LoggerColorBehavior.Disabled;
    }

    /// <inheritdoc cref="T:Microsoft.Extensions.Logging.Console.SimpleConsoleFormatterOptions" />
    public new LoggerColorBehavior ColorBehavior
    {
        get => LoggerColorBehavior.Disabled;
        set => base.ColorBehavior = LoggerColorBehavior.Disabled; // changed from throw new NotSupportedException("This formatter does not support ColorBehavior.");
    }

    /// <summary>
    ///     Specify if the parameter values are only logged as part of the message
    ///     or appended to the log line as key-value-pair. Default is <c>true</c>.
    /// </summary>
    public bool IncludeParameters { get; set; } = true;

    /// <summary>
    ///     Specify if <see cref="P:System.Diagnostics.Activity.Tags" /> are logged as key-value-pairs.
    ///     Default is <c>true</c>.
    /// </summary>
    public bool IncludeTags { get; set; } = true;

    /// <summary>
    ///     Specify if <see cref="P:System.Diagnostics.Activity.Baggage" /> are logged as key-value-pairs.
    ///     Default is <c>true</c>.
    /// </summary>
    public bool IncludeBaggage { get; set; } = true;
}