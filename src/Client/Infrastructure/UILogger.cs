namespace LTBlog.Client.Infrastructure;

internal delegate void OnLogHandler(string log);

internal class UILogger : ILogger {
    public static event OnLogHandler? OnLogHandler;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter) =>
        OnLogHandler?.Invoke(formatter(state, exception));

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NoOpDisposable.Instance;
}

file sealed class NoOpDisposable : IDisposable {
    public static readonly NoOpDisposable Instance = new();

    public void Dispose() {
    }
}

internal class NoOpLogger : ILogger {
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter) {
        // No operation
    }

    public bool IsEnabled(LogLevel logLevel) => false;

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NoOpDisposable.Instance;
}

public class UILoggerProvider : ILoggerProvider {
    private readonly ILogger _uiLogger = new UILogger();
    private readonly ILogger _noOpLogger = new NoOpLogger();

    public void Dispose() {
    }

    public ILogger CreateLogger(string categoryName) => categoryName.StartsWith("LTBlog") ? _uiLogger : _noOpLogger;
}

public static class UILoggerExtensions {
    public static ILoggingBuilder AddUILogger(this ILoggingBuilder builder) =>
        builder.AddProvider(new UILoggerProvider());
}