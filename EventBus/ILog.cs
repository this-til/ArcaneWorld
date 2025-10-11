namespace EventBus;

/// <summary>
///  定义一个ILog接口，用于记录日志
/// </summary>
public interface ILog {

    void debug(params object[] obj);
    void info(params object[] obj);
    void warn(params object[] obj);
    void error(params object[] obj);

}

/// <summary>
/// Log level
/// </summary>
public enum LogLevel {
    DEBUG, INFO, WARN, ERROR
}