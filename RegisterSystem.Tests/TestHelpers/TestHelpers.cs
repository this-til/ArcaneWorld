using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Moq;
using RegisterSystem;
using Xunit.Abstractions;

namespace RegisterSystem.Tests.TestHelpers;

/// <summary>
/// 测试用的日志实现
/// </summary>
public partial class TestLog : ILog {

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

    public void debug(params object[] obj) {
        var message = string.Join(" ", obj);
        DebugMessages.Add(message);
        OutputLog("DEBUG", message);
    }

    public void info(params object[] obj) {
        var message = string.Join(" ", obj);
        InfoMessages.Add(message);
        OutputLog("INFO", message);
    }

    public void warn(params object[] obj) {
        var message = string.Join(" ", obj);
        WarnMessages.Add(message);
        OutputLog("WARN", message);
    }

    public void error(params object[] obj) {
        var message = string.Join(" ", obj);
        ErrorMessages.Add(message);
        OutputLog("ERROR", message);
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
