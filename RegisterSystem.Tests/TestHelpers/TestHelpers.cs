using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Moq;
using RegisterSystem;
using Xunit.Abstractions;
using log4net;

#pragma warning disable CS8604 // 引用类型参数可能为 null。
namespace RegisterSystem.Tests.TestHelpers;

/// <summary>
/// 测试用的日志实现
/// </summary>
public partial class TestLog : log4net.ILog {

    private readonly ITestOutputHelper? _testOutputHelper;

    private readonly bool _enableConsoleOutput;

    public List<string> DebugMessages { get; } = new List<string>();

    public List<string> InfoMessages { get; } = new List<string>();

    public List<string> WarnMessages { get; } = new List<string>();

    public List<string> ErrorMessages { get; } = new List<string>();

    public TestLog(ITestOutputHelper? testOutputHelper = null, bool enableConsoleOutput = false) {
        _testOutputHelper = testOutputHelper;
        _enableConsoleOutput = enableConsoleOutput;
    }

    // log4net ILog 接口实现
    public bool IsDebugEnabled => true;

    public bool IsInfoEnabled => true;

    public bool IsWarnEnabled => true;

    public bool IsErrorEnabled => true;

    public bool IsFatalEnabled => true;

    public log4net.Core.ILogger Logger => throw new NotImplementedException("Not needed for tests");

    public void Debug(object? message) {
        DebugMessages.Add(message?.ToString() ?? "");
        OutputLog("DEBUG", message?.ToString() ?? "");
    }

    public void Debug(object? message, Exception? exception) {
        var msg = $"{message} - {exception}";
        DebugMessages.Add(msg);
        OutputLog("DEBUG", msg);
    }

    public void DebugFormat(string format, params object?[]? args) {
        var message = string.Format(format, args ?? []);
        DebugMessages.Add(message);
        OutputLog("DEBUG", message);
    }

    public void DebugFormat(string format, object? arg0) {
        var message = string.Format(format, arg0);
        DebugMessages.Add(message);
        OutputLog("DEBUG", message);
    }

    public void DebugFormat(string format, object? arg0, object? arg1) {
        var message = string.Format(format, arg0, arg1);
        DebugMessages.Add(message);
        OutputLog("DEBUG", message);
    }

    public void DebugFormat(string format, object? arg0, object? arg1, object? arg2) {
        if (arg2 == null)
            throw new ArgumentNullException(nameof(arg2));
        var message = string.Format(format, arg0, arg1, arg2);
        DebugMessages.Add(message);
        OutputLog("DEBUG", message);
    }

    public void DebugFormat(IFormatProvider? provider, string format, params object?[]? args) {
        var message = string.Format(provider, format, args ?? []);
        DebugMessages.Add(message);
        OutputLog("DEBUG", message);
    }

    public void Info(object? message) {
        InfoMessages.Add(message?.ToString() ?? "");
        OutputLog("INFO", message?.ToString() ?? "");
    }

    public void Info(object? message, Exception? exception) {
        var msg = $"{message} - {exception}";
        InfoMessages.Add(msg);
        OutputLog("INFO", msg);
    }

    public void InfoFormat(string format, params object?[]? args) {
        var message = string.Format(format, args);
        InfoMessages.Add(message);
        OutputLog("INFO", message);
    }

    public void InfoFormat(string format, object? arg0) {
        var message = string.Format(format, arg0);
        InfoMessages.Add(message);
        OutputLog("INFO", message);
    }

    public void InfoFormat(string format, object? arg0, object? arg1) {
        var message = string.Format(format, arg0, arg1);
        InfoMessages.Add(message);
        OutputLog("INFO", message);
    }

    public void InfoFormat(string format, object? arg0, object? arg1, object? arg2) {
        var message = string.Format(format, arg0, arg1, arg2);
        InfoMessages.Add(message);
        OutputLog("INFO", message);
    }

    public void InfoFormat(IFormatProvider? provider, string format, params object?[]? args) {
        var message = string.Format(provider, format, args);
        InfoMessages.Add(message);
        OutputLog("INFO", message);
    }

    public void Warn(object? message) {
        WarnMessages.Add(message?.ToString() ?? "");
        OutputLog("WARN", message?.ToString() ?? "");
    }

    public void Warn(object? message, Exception? exception) {
        var msg = $"{message} - {exception}";
        WarnMessages.Add(msg);
        OutputLog("WARN", msg);
    }

    public void WarnFormat(string format, params object?[]? args) {
        var message = string.Format(format, args);

        WarnMessages.Add(message);
        OutputLog("WARN", message);
    }

    public void WarnFormat(string format, object? arg0) {
        var message = string.Format(format, arg0);
        WarnMessages.Add(message);
        OutputLog("WARN", message);
    }
    
    public void WarnFormat(string format, object? arg0, object? arg1) {
#pragma warning restore CS8767 // 参数类型中引用类型的为 Null 性与隐式实现的成员不匹配(可能是由于为 Null 性特性)。
        var message = string.Format(format, arg0, arg1);
        WarnMessages.Add(message);
        OutputLog("WARN", message);
    }

    public void WarnFormat(string format, object? arg0, object? arg1, object? arg2) {
        var message = string.Format(format, arg0, arg1, arg2);
        WarnMessages.Add(message);
        OutputLog("WARN", message);
    }

    public void WarnFormat(IFormatProvider? provider, string format, params object?[]? args) {
        var message = string.Format(provider, format, args);
        WarnMessages.Add(message);
        OutputLog("WARN", message);
    }

    public void Error(object? message) {
        ErrorMessages.Add(message?.ToString() ?? "");
        OutputLog("ERROR", message?.ToString() ?? "");
    }

    public void Error(object? message, Exception? exception) {
        var msg = $"{message} - {exception}";
        ErrorMessages.Add(msg);
        OutputLog("ERROR", msg);
    }
    
    public void ErrorFormat(string format, params object?[]? args) {
#pragma warning restore CS8614 // 参数类型中引用类型的为 Null 性与隐式实现的成员不匹配。
        var message = string.Format(format, args);
        ErrorMessages.Add(message);
        OutputLog("ERROR", message);
    }

    public void ErrorFormat(string format, object? arg0) {
        var message = string.Format(format, arg0);
        ErrorMessages.Add(message);
        OutputLog("ERROR", message);
    }

    public void ErrorFormat(string format, object? arg0, object? arg1) {
        var message = string.Format(format, arg0, arg1);
        ErrorMessages.Add(message);
        OutputLog("ERROR", message);
    }

    public void ErrorFormat(string format, object? arg0, object? arg1, object? arg2) {
        var message = string.Format(format, arg0, arg1, arg2);
        ErrorMessages.Add(message);
        OutputLog("ERROR", message);
    }

    public void ErrorFormat(IFormatProvider? provider, string format, params object?[]? args) {
        var message = string.Format(provider, format, args);
        ErrorMessages.Add(message);
        OutputLog("ERROR", message);
    }

    public void Fatal(object? message) {
        ErrorMessages.Add(message?.ToString() ?? ""); // 将Fatal也记录到ErrorMessages中
        OutputLog("FATAL", message?.ToString() ?? "");
    }

    public void Fatal(object? message, Exception? exception) {
        var msg = $"{message} - {exception}";
        ErrorMessages.Add(msg);
        OutputLog("FATAL", msg);
    }

    public void FatalFormat(string format, params object?[]? args) {
        var message = string.Format(format, args);
        ErrorMessages.Add(message);
        OutputLog("FATAL", message);
    }

    public void FatalFormat(string format, object? arg0) {
        var message = string.Format(format, arg0);
        ErrorMessages.Add(message);
        OutputLog("FATAL", message);
    }

    public void FatalFormat(string format, object? arg0, object? arg1) {
        var message = string.Format(format, arg0, arg1);
        ErrorMessages.Add(message);
        OutputLog("FATAL", message);
    }

    public void FatalFormat(string format, object? arg0, object? arg1, object? arg2) {
        var message = string.Format(format, arg0, arg1, arg2);
        ErrorMessages.Add(message);
        OutputLog("FATAL", message);
    }

    public void FatalFormat(IFormatProvider? provider, string format, params object?[]? args) {
        var message = string.Format(provider, format, args);
        ErrorMessages.Add(message);
        OutputLog("FATAL", message);
    }
    

    private void OutputLog(string level, string message) {
        var formattedMessage = $"[{level}] {message}";

        // 输出到测试输出助手
        _testOutputHelper?.WriteLine(formattedMessage);

        // 如果启用了控制台输出，也输出到控制台
        if (_enableConsoleOutput) {
            Console.WriteLine(formattedMessage);
        }
    }

    public void Clear() {
        DebugMessages.Clear();
        InfoMessages.Clear();
        WarnMessages.Clear();
        ErrorMessages.Clear();
    }

    /// <summary>
    /// 获取所有日志消息的摘要
    /// </summary>
    public string GetLogSummary() {
        return $"Debug: {DebugMessages.Count}, Info: {InfoMessages.Count}, Warn: {WarnMessages.Count}, Error: {ErrorMessages.Count}";
    }

    /// <summary>
    /// 获取指定级别的所有消息
    /// </summary>
    public List<string> GetMessages(LogLevel level) {
        return level switch {
            LogLevel.DEBUG => DebugMessages,
            LogLevel.INFO => InfoMessages,
            LogLevel.WARN => WarnMessages,
            LogLevel.ERROR => ErrorMessages,
            _ => new List<string>()
        };
    }

    /// <summary>
    /// 检查是否有任何错误或警告
    /// </summary>
    public bool HasErrorsOrWarnings() {
        return ErrorMessages.Count > 0 || WarnMessages.Count > 0;
    }

    /// <summary>
    /// 获取所有错误和警告消息
    /// </summary>
    public List<string> GetErrorsAndWarnings() {
        var result = new List<string>();
        result.AddRange(WarnMessages.Select(m => $"[WARN] {m}"));
        result.AddRange(ErrorMessages.Select(m => $"[ERROR] {m}"));
        return result;
    }

}

/// <summary>
/// 用于测试的简单 RegisterBasics 实现
/// </summary>
public partial class SimpleRegisterBasics : RegisterBasics {

    public string? TestValue { get; set; }

}

public partial class DerivedSimpleRegisterBasics : SimpleRegisterBasics {

    public override void setup() {
        base.setup();
        qwq = new InternalSimpleRegisterBasics();
    }

}

public partial class InternalSimpleRegisterBasics : RegisterBasics {

}

/// <summary>
/// 用于测试的 RegisterManage 实现
/// </summary>
public partial class TestRegisterManage : RegisterManage<SimpleRegisterBasics> {

    public override Type registerType => typeof(SimpleRegisterBasics);

    public override int priority => 0;

    public override void setup() {
        base.setup();
        aaa = new SimpleRegisterBasics() { TestValue = "a" };
        bbb = new SimpleRegisterBasics() { TestValue = "b" };
        ccc = new SimpleRegisterBasics() { TestValue = "c" };
    }

}

/// <summary>
/// 继承的 RegisterManage 测试类
/// </summary>
public partial class DerivedTestRegisterManage : RegisterManage<DerivedSimpleRegisterBasics> {

    public override Type? basicsRegisterManageType => typeof(TestRegisterManage);

    public override int priority => 10;

    public override void setup() {
        base.setup();

        ddd = new DerivedSimpleRegisterBasics();
        eee = new DerivedSimpleRegisterBasics();
        fff = new DerivedSimpleRegisterBasics();
    }

}

public partial class InternalSimpleRegisterManage : RegisterManage<InternalSimpleRegisterBasics> {

    public override void setup() {
        base.setup();
    }

}
