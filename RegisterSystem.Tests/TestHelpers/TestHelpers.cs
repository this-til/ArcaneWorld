using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Moq;
using RegisterSystem;
using Xunit.Abstractions;

#pragma warning disable CS8604 // 引用类型参数可能为 null。
namespace RegisterSystem.Tests.TestHelpers;

/// <summary>
/// 用于测试的简单 RegisterBasics 实现
/// </summary>
public partial class SimpleRegisterBasics : RegisterBasics {

    public string? TestValue { get; set; }

}

public partial class DerivedSimpleRegisterBasics : SimpleRegisterBasics {

    protected override void setup() {
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

    protected override void setup() {
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

    protected override void setup() {
        base.setup();

        ddd = new DerivedSimpleRegisterBasics();
        eee = new DerivedSimpleRegisterBasics();
        fff = new DerivedSimpleRegisterBasics();
    }

}

public partial class InternalSimpleRegisterManage : RegisterManage<InternalSimpleRegisterBasics> {

    protected override void setup() {
        base.setup();
    }

}
