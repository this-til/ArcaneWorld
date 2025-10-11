namespace RegisterSystem;

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
/// 带有日志等级过滤的日志包装器
/// </summary>
public class LogWrapper : ILog {

    private readonly ILog? _innerLog;

    private readonly RegisterSystem _registerSystem;

    public LogWrapper(ILog? innerLog, RegisterSystem registerSystem) {
        _innerLog = innerLog;
        _registerSystem = registerSystem;
    }

    public void debug(params object[] obj) {
        if (_registerSystem.logLevel <= LogLevel.DEBUG) {
            _innerLog?.debug(obj);
        }
    }

    public void info(params object[] obj) {
        if (_registerSystem.logLevel <= LogLevel.INFO) {
            _innerLog?.info(obj);
        }
    }

    public void warn(params object[] obj) {
        if (_registerSystem.logLevel <= LogLevel.WARN) {
            _innerLog?.warn(obj);
        }
    }

    public void error(params object[] obj) {
        if (_registerSystem.logLevel <= LogLevel.ERROR) {
            _innerLog?.error(obj);
        }
    }

}