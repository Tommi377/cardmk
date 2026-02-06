using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using FileAccess = Godot.FileAccess;

namespace RealMK;

/// <summary>
///     Custom logger with file/line tracking and log levels.
///     Usage: Log.Info("Message"), Log.Warning("Warning"), Log.Error("Error"), Log.Debug("Debug info")
///     Works both inside Godot and in unit tests (prints to Console when Godot is unavailable).
/// </summary>
public static class Log
{
    public enum Level
    {
        Trace,
        Debug,
        Info,
        Warning,
        Error
    }

    private static readonly Color _traceColor = new(0.4f, 0.4f, 0.4f); // Dark 
    private static readonly Color _debugColor = new(0.5f, 0.8f, 1.0f); // Light blue
    private static readonly Color _infoColor = new(0.8f, 0.8f, 0.8f); // Light gray
    private static readonly Color _warningColor = new(1.0f, 0.8f, 0.0f); // Yellow
    private static readonly Color _errorColor = new(1.0f, 0.3f, 0.3f); // Red

    private static Level _minLogLevel = Level.Debug;
    private static bool _logToFile;
    private static string? _logFilePath;
    private static Mutex? _fileMutex;
    private static readonly object _consoleLock = new();
    private static readonly bool _isGodotAvailable;

    static Log()
    {
        // Check if running in test context by looking for test framework assemblies
        // This avoids calling any Godot types in the static constructor
        _isGodotAvailable = !IsRunningInTestContext();
        if (_isGodotAvailable)
        {
            _fileMutex = new Mutex();
        }
    }

    private static bool IsRunningInTestContext()
    {
        // Check if xunit or other test assemblies are loaded
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        return assemblies.Any(a =>
            a.FullName != null && (
                a.FullName.StartsWith("xunit", StringComparison.OrdinalIgnoreCase) ||
                a.FullName.StartsWith("Microsoft.TestPlatform", StringComparison.OrdinalIgnoreCase) ||
                a.FullName.StartsWith("testhost", StringComparison.OrdinalIgnoreCase)
            ));
    }

    /// <summary>
    ///     Set the minimum log level. Messages below this level will not be logged.
    /// </summary>
    public static void SetMinLevel(Level level)
    {
        _minLogLevel = level;
    }

    /// <summary>
    ///     Enable logging to file. Creates a log file in user:// directory.
    /// </summary>
    public static void EnableFileLogging(string? customPath = null)
    {
        _logToFile = true;
        if (customPath != null)
        {
            _logFilePath = customPath;
        }
        else
        {
            string timestamp = Time.GetDatetimeStringFromSystem().Replace(":", "-");
            _logFilePath = $"user://log_{timestamp}.log";
        }

        // Write header to log file
        WriteToFile($"=== Log started at {Time.GetDatetimeStringFromSystem()} ===");
        WriteToFile($"Godot Version: {Engine.GetVersionInfo()["string"]}");
        WriteToFile($"OS: {OS.GetName()} {OS.GetVersion()}");
        WriteToFile("");
    }

    /// <summary>
    ///     Log a trace message. Use for debugging traces.
    /// </summary>
    public static void Trace(
        string message,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = ""
    )
    {
        LogMessage(Level.Trace, message, filePath, lineNumber, memberName);
    }

    /// <summary>
    ///     Log a debug message. Use for detailed diagnostic information.
    /// </summary>
    public static void Debug(
        string message,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = ""
    )
    {
        LogMessage(Level.Debug, message, filePath, lineNumber, memberName);
    }

    /// <summary>
    ///     Log an info message. Use for general informational messages.
    /// </summary>
    public static void Info(
        string message,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = ""
    )
    {
        LogMessage(Level.Info, message, filePath, lineNumber, memberName);
    }

    /// <summary>
    ///     Log a warning message. Use for potentially harmful situations.
    /// </summary>
    public static void Warning(
        string message,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = ""
    )
    {
        LogMessage(Level.Warning, message, filePath, lineNumber, memberName);
    }

    /// <summary>
    ///     Log an error message. Use for error events that might still allow the application to continue.
    /// </summary>
    public static void Error(
        string message,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = ""
    )
    {
        LogMessage(Level.Error, message, filePath, lineNumber, memberName);
    }

    private static void LogMessage(
        Level level,
        string message,
        string filePath,
        int lineNumber,
        string memberName
    )
    {
        if (level < _minLogLevel) return;

        string fileName = Path.GetFileName(filePath);
        string timestamp = _isGodotAvailable
            ? Time.GetTimeStringFromSystem()
            : DateTime.Now.ToString("HH:mm:ss");
        string levelStr = level.ToString().ToUpper();
        string location = $"{fileName}:{lineNumber}";
        string method = string.IsNullOrEmpty(memberName) ? "" : $" @ {memberName}()";

        // Format: [HH:MM:SS] LEVEL [File.cs:123 @ Method()] Message
        string formattedMessage = $"[{timestamp}] {levelStr} [{location}{method}] {message}";

        if (_isGodotAvailable)
        {
            // Get color for this log level
            Color color = level switch
            {
                Level.Trace => _traceColor,
                Level.Debug => _debugColor,
                Level.Info => _infoColor,
                Level.Warning => _warningColor,
                Level.Error => _errorColor,
                _ => _infoColor
            };

            // Print to console with color
            string colorHex = color.ToHtml();
            string richMessage = $"[color=#{colorHex}]{formattedMessage}[/color]";
            GD.PrintRich(richMessage);
        }
        else
        {
            // Fallback to Console for unit tests
            lock (_consoleLock)
            {
                var originalColor = Console.ForegroundColor;
                Console.ForegroundColor = level switch
                {
                    Level.Trace => ConsoleColor.DarkGray,
                    Level.Debug => ConsoleColor.Cyan,
                    Level.Info => ConsoleColor.Gray,
                    Level.Warning => ConsoleColor.Yellow,
                    Level.Error => ConsoleColor.Red,
                    _ => ConsoleColor.Gray
                };
                Console.WriteLine(formattedMessage);
                Console.ForegroundColor = originalColor;
            }
        }

        // Write to file if enabled (only in Godot)
        if (_logToFile && _isGodotAvailable)
        {
            WriteToFile(formattedMessage);
        }
    }

    private static void WriteToFile(string message)
    {
        if (_logFilePath == null || _fileMutex == null || !_isGodotAvailable) return;

        _fileMutex.Lock();
        try
        {
            FileAccess file = FileAccess.Open(_logFilePath, FileAccess.ModeFlags.ReadWrite);
            if (file != null)
            {
                file.SeekEnd();
                file.StoreLine(message);
                file.Flush();
            }
        }
        catch (Exception ex)
        {
            GD.PushError($"Failed to write to log file: {ex.Message}");
        }
        finally
        {
            _fileMutex.Unlock();
        }
    }

    /// <summary>
    ///     Force flush the log file buffer (if file logging is enabled).
    /// </summary>
    public static void Flush()
    {
        if (!_logToFile || _logFilePath == null || _fileMutex == null || !_isGodotAvailable) return;

        _fileMutex.Lock();
        try
        {
            FileAccess file = FileAccess.Open(_logFilePath, FileAccess.ModeFlags.ReadWrite);
            file?.Flush();
        }
        finally
        {
            _fileMutex.Unlock();
        }
    }
}
