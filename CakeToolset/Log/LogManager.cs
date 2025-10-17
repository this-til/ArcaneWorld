using CommonUtil.Log;

namespace CakeToolset.Log;

public static class LogManager {

    public static LogLevel DefLogLevel = LogLevel.Debug;

    public static ILog GetLogger(Type type) => GetLogger(type.FullName ?? type.Name);

    public static ILog GetLogger(string type) => new Log(DefLogLevel, type);

}
