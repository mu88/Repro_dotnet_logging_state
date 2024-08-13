using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace WebApi;

internal sealed class CustomServiceRuntimeContractFormatter : ConsoleFormatter, IDisposable
{
    public const string FormatterName = "CustomServiceRuntimeContractFormatter";

    private static readonly Action<object?, TextWriter> ProcessScope = (Action<object, TextWriter>)((scope, state) =>
    {
        if (scope is IEnumerable<KeyValuePair<string, object>> keyValuePairs2)
        {
            WriteStructuredValues(state, keyValuePairs2);
        }
        else
        {
            state.Write(' ');
            state.Write(scope);
        }
    });

    private readonly IDisposable? _optionsReloadToken;
    private CustomServiceRuntimeContractFormatterOptions _formatterOptions;

    public CustomServiceRuntimeContractFormatter(
        IOptionsMonitor<CustomServiceRuntimeContractFormatterOptions> options)
        : base(nameof(CustomServiceRuntimeContractFormatter))
    {
        ReloadLoggerOptions(options.CurrentValue);
        _optionsReloadToken = options.OnChange(ReloadLoggerOptions);
    }

    public void Dispose() => _optionsReloadToken?.Dispose();

    /// <inheritdoc />
    public override void Write<TState>(
        in LogEntry<TState> logEntry,
        IExternalScopeProvider? scopeProvider,
        TextWriter textWriter)
    {
        Func<TState, Exception, string> formatter = logEntry.Formatter;
        var message = formatter != null ? formatter(logEntry.State, logEntry.Exception) : null;
        if (logEntry.Exception == null && message == null)
            return;
        WriteTimestamp(textWriter);
        WriteLogLevel(textWriter, logEntry);
        WriteEventId(textWriter, logEntry);
        WriteEventName(textWriter, logEntry);
        WriteCategory(textWriter, logEntry);
        WriteMessage(textWriter, message);
        if (_formatterOptions.IncludeParameters && logEntry.State is IEnumerable<KeyValuePair<string, object>> state)
            WriteStructuredValues(textWriter, state);
        if (_formatterOptions.IncludeScopes)
            WriteScopeInformation(textWriter, scopeProvider);
        if (_formatterOptions.IncludeBaggage)
            WriteBaggage(textWriter);
        if (_formatterOptions.IncludeTags)
            WriteTags(textWriter);
        if (logEntry.Exception != null)
            WriteException(textWriter, logEntry.Exception);
        textWriter.WriteLine();
    }

    private void ReloadLoggerOptions(CustomServiceRuntimeContractFormatterOptions options)
    {
        _formatterOptions = options;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteTimestamp(TextWriter textWriter)
    {
        var timestampFormat = _formatterOptions.TimestampFormat;
        var str = _formatterOptions.UseUtcTimestamp
            ? DateTimeOffset.UtcNow.ToString(timestampFormat, CultureInfo.InvariantCulture)
            : DateTimeOffset.Now.ToString(timestampFormat, CultureInfo.InvariantCulture);
        textWriter.Write(str);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteStructuredValues(
        TextWriter textWriter,
        IEnumerable<KeyValuePair<string, object>> keyValuePairs)
    {
        foreach (var keyValuePair in keyValuePairs)
            if (!keyValuePair.Key.Equals("{OriginalFormat}", StringComparison.Ordinal))
            {
                if (keyValuePair.Value is IEnumerable<string> values)
                    WriteKeyValuePair(textWriter, keyValuePair.Key, string.Join(", ", values));
                else
                    WriteKeyValuePair(textWriter, keyValuePair.Key, keyValuePair.Value);
            }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteCategory<TState>(TextWriter textWriter, LogEntry<TState> logEntry)
    {
        textWriter.Write(" sourceContext=\"");
        textWriter.Write(logEntry.Category);
        textWriter.Write('"');
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteEventId<TState>(TextWriter textWriter, LogEntry<TState> logEntry)
    {
        if (logEntry.EventId.Id == 0 && string.IsNullOrEmpty(logEntry.EventId.Name))
            return;
        textWriter.Write(" eventId=");
        Span<char> destination = stackalloc char[10];
        int charsWritten;
        logEntry.EventId.Id.TryFormat(destination, out charsWritten, provider: CultureInfo.InvariantCulture);
        textWriter.Write(destination.Slice(0, charsWritten));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteEventName<TState>(TextWriter textWriter, LogEntry<TState> logEntry)
    {
        if (string.IsNullOrEmpty(logEntry.EventId.Name))
            return;
        textWriter.Write(" eventName=");
        textWriter.Write(logEntry.EventId.Name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteLogLevel<TState>(TextWriter textWriter, LogEntry<TState> logEntry)
    {
        var logLevelString = GetLogLevelString(logEntry.LogLevel);
        textWriter.Write(' ');
        textWriter.Write(logLevelString);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteMessage(TextWriter textWriter, string? message)
    {
        if (string.IsNullOrEmpty(message))
            return;
        textWriter.Write(" message=\"");
        textWriter.Write(message);
        textWriter.Write('"');
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetLogLevelString(LogLevel logLevel)
    {
        switch (logLevel)
        {
            case LogLevel.Trace:
                return "level=TRACE";
            case LogLevel.Debug:
                return "level=DEBUG";
            case LogLevel.Information:
                return "level=INFO";
            case LogLevel.Warning:
                return "level=WARN";
            case LogLevel.Error:
                return "level=ERROR";
            case LogLevel.Critical:
                return "level=FATAL";
            default:
                throw new ArgumentOutOfRangeException(nameof(logLevel));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteScopeInformation(
        TextWriter textWriter,
        IExternalScopeProvider? scopeProvider)
    {
        scopeProvider?.ForEachScope(ProcessScope, textWriter);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteTags(TextWriter textWriter)
    {
        if (Activity.Current == null)
            return;
        WriteKeyValuePairs(textWriter, Activity.Current.TagObjects);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteBaggage(TextWriter textWriter)
    {
        if (Activity.Current == null)
            return;
        WriteKeyValuePairs(textWriter, Activity.Current.Baggage);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteKeyValuePairs(
        TextWriter textWriter,
        IEnumerable<KeyValuePair<string, string?>> pairs)
    {
        foreach (KeyValuePair<string, string> pair in pairs)
            WriteKeyValuePair(textWriter, pair.Key, pair.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteKeyValuePairs(
        TextWriter textWriter,
        IEnumerable<KeyValuePair<string, object?>> pairs)
    {
        foreach (KeyValuePair<string, object> pair in pairs)
            WriteKeyValuePair(textWriter, pair.Key, pair.Value);
    }

    private static void WriteKeyValuePair(TextWriter textWriter, string key, object? value)
    {
        if (string.IsNullOrEmpty(key))
            return;
        textWriter.Write(' ');
        textWriter.Write(char.ToLower(key[0], CultureInfo.InvariantCulture));
        textWriter.Write(key.AsSpan().Slice(1, key.Length - 1));
        textWriter.Write("=\"");
        if (value == null)
        {
            textWriter.Write(string.Empty);
        }
        else
        {
            var str = Convert.ToString(value, CultureInfo.InvariantCulture);
            textWriter.Write(str ?? string.Empty);
        }

        textWriter.Write('"');
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteException(TextWriter textWriter, Exception exception)
    {
        textWriter.Write(" exception=\"");
        textWriter.Write(exception.ToString());
        textWriter.Write('"');
    }
}