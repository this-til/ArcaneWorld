using CommonUtil.Log;
using Godot;
using Environment = System.Environment;

namespace CakeToolset.Log;

internal class Log : ILog {

    private LogLevel level;

    private string name;

    public Log(LogLevel level, string name) {
        this.level = level;
        this.name = name;
    }

    private void WriteLog(LogLevel logLevel, object? message, Exception? exception = null) {
        if ((int)logLevel < (int)level) {
            return;
        }

        var dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var thread = Environment.CurrentManagedThreadId;
        var levelStr = logLevel.ToString().ToUpper().PadRight(5);
        var logger = name;
        var msg = message?.ToString() ?? "null";

        if (exception != null) {
            msg += $"\n{exception}";
        }

        var logMessage = $"{dateTime} | [{thread}] | {levelStr} | {logger} : {msg}";

        // 根据日志级别使用不同的 Godot 输出方法
        switch (logLevel) {
            case LogLevel.Debug:
            case LogLevel.Info:
                GD.Print(logMessage);
                break;
            case LogLevel.Warning:
                GD.PushWarning(logMessage);
                break;
            case LogLevel.Error:
                GD.PushError(logMessage);
                break;
        }
    }

    public void Debug(object? message) {
        WriteLog(LogLevel.Debug, message);
    }

    public void Debug(object? message, Exception? exception) {
        WriteLog(LogLevel.Debug, message, exception);
    }

    public void Info(object? message) {
        WriteLog(LogLevel.Info, message);
    }

    public void Info(object? message, Exception? exception) {
        WriteLog(LogLevel.Info, message, exception);
    }

    public void Warn(object? message) {
        WriteLog(LogLevel.Warning, message);
    }

    public void Warn(object? message, Exception? exception) {
        WriteLog(LogLevel.Warning, message, exception);
    }

    public void Error(object? message) {
        WriteLog(LogLevel.Error, message);
    }

    public void Error(object? message, Exception? exception) {
        WriteLog(LogLevel.Error, message, exception);
    }

    public bool IsDebugEnabled => level <= LogLevel.Debug;

    public bool IsInfoEnabled => level <= LogLevel.Info;

    public bool IsWarnEnabled => level <= LogLevel.Warning;

    public bool IsErrorEnabled => level <= LogLevel.Error;

}
