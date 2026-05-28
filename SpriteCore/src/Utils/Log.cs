namespace SpriteCore.Utils;

public enum LogLevel
{
    Trace = 0,
    Debug = 1,
    Info = 2,
    Warning = 3,
    Error = 4,
}

public static class Log
{
    private static readonly object _lock = new();
    private static StreamWriter? _writer;
    private static string? _currentFileDate;

    public static LogLevel MinimumLevel { get; set; } = LogLevel.Debug;
    public static bool EnableConsole { get; set; } = true;
    public static bool EnableFile { get; set; } = true;

    public static void Initialize()
    {
        if (!EnableFile) return;
        EnsureWriter();
    }

    public static void Shutdown()
    {
        lock (_lock)
        {
            _writer?.Dispose();
            _writer = null;
        }
    }

    public static void Trace(string category, string message) => Write(LogLevel.Trace, category, message);
    public static void Debug(string category, string message) => Write(LogLevel.Debug, category, message);
    public static void Info(string category, string message) => Write(LogLevel.Info, category, message);
    public static void Warning(string category, string message) => Write(LogLevel.Warning, category, message);
    public static void Error(string category, string message) => Write(LogLevel.Error, category, message);

    // Convenience overloads without category
    public static void Trace(string message) => Write(LogLevel.Trace, "General", message);
    public static void Debug(string message) => Write(LogLevel.Debug, "General", message);
    public static void Info(string message) => Write(LogLevel.Info, "General", message);
    public static void Warning(string message) => Write(LogLevel.Warning, "General", message);
    public static void Error(string message) => Write(LogLevel.Error, "General", message);

    // Lua-facing log method
    public static void LuaLog(string level, string message)
    {
        var logLevel = ParseLevel(level);
        Write(logLevel, "Lua", message);
    }

    private static void Write(LogLevel level, string category, string message)
    {
        if (level < MinimumLevel) return;

        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var levelStr = level.ToString().ToUpperInvariant();
        var line = $"[{timestamp}] [{levelStr}] [{category}] {message}";

        lock (_lock)
        {
            if (EnableConsole)
            {
                var prevColor = Console.ForegroundColor;
                Console.ForegroundColor = GetConsoleColor(level);
                Console.WriteLine(line);
                Console.ForegroundColor = prevColor;
            }

            if (EnableFile)
            {
                EnsureWriter();
                _writer?.WriteLine(line);
            }
        }
    }

    private static ConsoleColor GetConsoleColor(LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => ConsoleColor.DarkGray,
            LogLevel.Debug => ConsoleColor.Gray,
            LogLevel.Info => ConsoleColor.White,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            _ => ConsoleColor.White,
        };
    }

    private static void EnsureWriter()
    {
        var today = DateTime.Now.ToString("yyyy-MM-dd");
        if (_writer != null && _currentFileDate == today) return;

        _writer?.Dispose();
        _currentFileDate = today;

        var logDir = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(logDir);

        var filePath = Path.Combine(logDir, $"spriteforge-{today}.log");
        _writer = new StreamWriter(filePath, append: true) { AutoFlush = true };
    }

    private static LogLevel ParseLevel(string level)
    {
        return level.ToLowerInvariant() switch
        {
            "trace" => LogLevel.Trace,
            "debug" => LogLevel.Debug,
            "info" => LogLevel.Info,
            "warning" or "warn" => LogLevel.Warning,
            "error" or "err" => LogLevel.Error,
            _ => LogLevel.Info,
        };
    }
}
