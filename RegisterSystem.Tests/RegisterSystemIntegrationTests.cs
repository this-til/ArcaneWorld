using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Moq;
using RegisterSystem;
using RegisterSystem.Tests.TestHelpers;
using Xunit;
using Xunit.Abstractions;

namespace RegisterSystem.Tests;

public partial  class RegisterSystemIntegrationTests {

    private readonly TestLog _testLog;
    private readonly Assembly _testAssembly;
    private readonly ITestOutputHelper _testOutputHelper;

    public RegisterSystemIntegrationTests(ITestOutputHelper testOutputHelper) {
        _testOutputHelper = testOutputHelper;
        _testLog = new TestLog(testOutputHelper, enableConsoleOutput: true);
        _testAssembly = Assembly.GetExecutingAssembly();
    }

    [Fact]
    public void managedAssemblySet_ShouldBeRequired() {
        // Arrange & Act
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };

        // Assert
        registerSystem.managedAssemblySet.Should().NotBeNull();
        registerSystem.managedAssemblySet.Should().Contain(_testAssembly);
    }

    [Fact]
    public void initRegisterSystem_WithValidAssembly_ShouldInitializeSuccessfully() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };

        // Act
        Action action = () => registerSystem.initRegisterSystem();

        // Assert
        action.Should().NotThrow();
        registerSystem.allType.Should().NotBeNull();
        registerSystem.allType.Should().NotBeEmpty();
    }

    [Fact]
    public void initRegisterSystem_ShouldDiscoverRegisterManageTypes() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };

        // Act
        registerSystem.initRegisterSystem();

        // Assert
        registerSystem.registerManageTypeList.Should().NotBeNull();
        registerSystem.registerManageTypeList.Should().Contain(typeof(TestRegisterManage));
        registerSystem.registerManageTypeList.Should().Contain(typeof(DerivedTestRegisterManage));
    }

    [Fact]
    public void initRegisterSystem_ShouldCreateManageInstances() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };

        // Act
        registerSystem.initRegisterSystem();

        // Assert
        registerSystem.manageList.Should().NotBeNull();
        registerSystem.manageList.Should().NotBeEmpty();
        registerSystem.manageList.Should().Contain(m => m.GetType() == typeof(TestRegisterManage));
        registerSystem.manageList.Should().Contain(m => m.GetType() == typeof(DerivedTestRegisterManage));
    }

    [Fact]
    public void initRegisterSystem_ShouldOrderManagesByPriority() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };

        // Act
        registerSystem.initRegisterSystem();

        // Assert
        List<int> orderedPriorities = registerSystem.manageList.Select(m => m.priority).ToList();
        for (int i = 1; i < orderedPriorities.Count; i++) {
            orderedPriorities[i].Should().BeLessOrEqualTo(orderedPriorities[i - 1]);
        }
    }

    [Fact]
    public void initRegisterSystem_ShouldCreateNameManageMap() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };

        // Act
        registerSystem.initRegisterSystem();

        // Assert
        registerSystem.nameManageMap.Should().NotBeNull();
        registerSystem.nameManageMap.Should().NotBeEmpty();
            
        RegisterManage? testManage = registerSystem.manageList.FirstOrDefault(m => m.GetType() == typeof(TestRegisterManage));
        if (testManage != null) {
            registerSystem.nameManageMap.Should().ContainKey(testManage.name);
            registerSystem.nameManageMap[testManage.name].Should().Be(testManage);
        }
    }

    [Fact]
    public void initRegisterSystem_ShouldCreateManageMap() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };

        // Act
        registerSystem.initRegisterSystem();

        // Assert
        registerSystem.manageMap.Should().NotBeNull();
        registerSystem.manageMap.Should().ContainKey(typeof(TestRegisterManage));
        registerSystem.manageMap.Should().ContainKey(typeof(DerivedTestRegisterManage));
    }

    [Fact]
    public void initRegisterSystem_ShouldIdentifyRootManages() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };

        // Act
        registerSystem.initRegisterSystem();

        // Assert
        registerSystem.rootManageList.Should().NotBeNull();
        registerSystem.rootManageList.Should().Contain(m => m.GetType() == typeof(TestRegisterManage));
        registerSystem.rootManageList.Should().NotContain(m => m.GetType() == typeof(DerivedTestRegisterManage));
    }

    [Fact]
    public void initRegisterSystem_ShouldCreateRootRegisterTypeManageMap() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };

        // Act
        registerSystem.initRegisterSystem();

        // Assert
        registerSystem.rootRegisterTypeManageMap.Should().NotBeNull();
        registerSystem.rootRegisterTypeManageMap.Should().ContainKey(typeof(SimpleRegisterBasics));
    }

    [Fact]
    public void initRegisterSystem_ShouldEstablishHierarchy() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };

        // Act
        registerSystem.initRegisterSystem();

        // Assert
        RegisterManage? derivedManage = registerSystem.manageList.FirstOrDefault(m => m.GetType() == typeof(DerivedTestRegisterManage));
        RegisterManage? testManage = registerSystem.manageList.FirstOrDefault(m => m.GetType() == typeof(TestRegisterManage));

        derivedManage?.basicsRegisterManage.Should().Be(testManage!);
        testManage?.sonRegisterManage.Should().Contain(derivedManage!);
    }

    [Fact]
    public void initRegisterSystem_ShouldCallLifecycleMethods() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };

        // Act
        registerSystem.initRegisterSystem();

        // Assert
        // 验证所有管理器都被正确初始化
        foreach (RegisterManage manage in registerSystem.manageList) {
            manage.registerSystem.Should().Be(registerSystem);
        }
    }

    [Fact]
    public void initRegisterSystem_ShouldInitializeRegisterBasics() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };

        // Act
        registerSystem.initRegisterSystem();

        // Assert
        registerSystem.registerBasicsSortedSet.Should().NotBeNull();
        // 根据实际的 RegisterBasics 实例来验证
    }

    [Fact]
    public void getRegisterManageOfManageType_WithValidType_ShouldReturnCorrectManage() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };
        registerSystem.initRegisterSystem();

        // Act
        RegisterManage? result = registerSystem.getRegisterManageOfManageType(typeof(TestRegisterManage));

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<TestRegisterManage>();
    }

    [Fact]
    public void getRegisterManageOfManageType_WithInvalidType_ShouldReturnNull() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };
        registerSystem.initRegisterSystem();

        // Act
        RegisterManage? result = registerSystem.getRegisterManageOfManageType(typeof(string));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void getRegisterManageOfManageType_Generic_ShouldReturnCorrectType() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };
        registerSystem.initRegisterSystem();

        // Act
        TestRegisterManage? result = registerSystem.getRegisterManageOfManageType<TestRegisterManage>();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<TestRegisterManage>();
    }

    [Fact]
    public void getRegisterManageOfRegisterType_WithValidType_ShouldReturnCorrectManage() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };
        registerSystem.initRegisterSystem();

        // Act
        RegisterManage? result = registerSystem.getRegisterManageOfRegisterType(typeof(SimpleRegisterBasics));

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<TestRegisterManage>();
    }

    [Fact]
    public void getRegisterManageOfRegisterType_WithInheritedType_ShouldReturnParentManage() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };
        registerSystem.initRegisterSystem();

        // Act
        RegisterManage? result = registerSystem.getRegisterManageOfRegisterType<SimpleRegisterBasics>();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<TestRegisterManage>();
    }

    [Fact]
    public void getRegisterManageOfName_WithValidName_ShouldReturnCorrectManage() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };
        registerSystem.initRegisterSystem();

        RegisterManage? testManage = registerSystem.manageList.FirstOrDefault(m => m.GetType() == typeof(TestRegisterManage));
        ResourceLocation? name = testManage?.name;

        // Act
        RegisterManage? result = registerSystem.getRegisterManageOfName(name);

        // Assert
        result.Should().Be(testManage!);
    }

    [Fact]
    public void getRegisterManageOfName_WithInvalidName_ShouldReturnNull() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };
        registerSystem.initRegisterSystem();

        ResourceLocation invalidName = new ResourceLocation("invalid", "name");

        // Act
        RegisterManage? result = registerSystem.getRegisterManageOfName(invalidName);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void logLevel_ShouldFilterLogMessages() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.WARN
        };

        // Act
        registerSystem.initRegisterSystem();

        // Assert
        // 验证只有 WARN 和 ERROR 级别的日志被记录
        _testLog.DebugMessages.Should().BeEmpty();
        _testLog.InfoMessages.Should().BeEmpty();
        // WARN 和 ERROR 消息可能存在，取决于初始化过程
    }

    [Fact]
    public void initRegisterSystem_WithNullLog_ShouldNotThrow() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = null,
            logLevel = LogLevel.DEBUG
        };

        // Act & Assert
        Action action = () => registerSystem.initRegisterSystem();
        action.Should().NotThrow();
    }

    [Fact]
    public void initRegisterSystem_ShouldRegisterRegisterBasics() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };

        // Act
        registerSystem.initRegisterSystem();

        // Assert
        registerSystem.registerBasicsSortedSet.Should().NotBeNull();
        registerSystem.registerBasicsSortedSet.Should().NotBeEmpty();
        
        // 验证 TestRegisterManage 中的 RegisterBasics 被注册
        var testManage = registerSystem.getRegisterManageOfManageType<TestRegisterManage>()!;
        testManage.Should().NotBeNull();
        testManage.values.Should().Contain(r => r.name.path == "aaa");
        testManage.values.Should().Contain(r => r.name.path == "bbb");
        testManage.values.Should().Contain(r => r.name.path == "ccc");
    }

    [Fact]
    public void initRegisterSystem_ShouldSetRegisterBasicsProperties() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };

        // Act
        registerSystem.initRegisterSystem();

        // Assert
        var testManage = registerSystem.getRegisterManageOfManageType<TestRegisterManage>();
        testManage.Should().NotBeNull();

        foreach (var registerBasics in testManage!.values) {
            registerBasics.name.Should().NotBeNull();
            registerBasics.registerManage.rootRegisterManage.Should().Be(testManage);
            registerBasics.registerSystem.Should().Be(registerSystem);
            registerBasics.globalId.Should().BeGreaterOrEqualTo(0);
        }
    }

    [Fact]
    public void initRegisterSystem_ShouldSetRegisterBasicsNames() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };

        // Act
        registerSystem.initRegisterSystem();

        // Assert
        var testManage = registerSystem.getRegisterManageOfManageType<TestRegisterManage>();
        testManage.Should().NotBeNull();

        // 验证名称设置
        testManage!.get(new ResourceLocation(testManage.name.domain, "aaa")).Should().NotBeNull();
        testManage.get(new ResourceLocation(testManage.name.domain, "bbb")).Should().NotBeNull();
        testManage.get(new ResourceLocation(testManage.name.domain, "ccc")).Should().NotBeNull();
    }

    [Fact]
    public void initRegisterSystem_ShouldSetRegisterBasicsIndices() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };

        // Act
        registerSystem.initRegisterSystem();

        // Assert
        var testManage = registerSystem.getRegisterManageOfManageType<TestRegisterManage>();
        testManage.Should().NotBeNull();

        // 验证索引设置
        for (int i = 0; i < testManage!.count; i++) {
            var registerBasics = testManage.index(i);
            registerBasics.Should().NotBeNull();
            registerBasics!.index.Should().Be(i);
        }
    }

    [Fact]
    public void initRegisterSystem_ShouldSortRegisterBasicsByPriority() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };

        // Act
        registerSystem.initRegisterSystem();

        // Assert
        var testManage = registerSystem.getRegisterManageOfManageType<TestRegisterManage>();
        testManage.Should().NotBeNull();

        // 验证排序（按优先级和 globalId 排序）
        var sortedBasics = testManage!.values.ToList();
        for (int i = 1; i < sortedBasics.Count; i++) {
            var current = sortedBasics[i];
            var previous = sortedBasics[i - 1];
            var comparison = current.CompareTo(previous);
            comparison.Should().BeGreaterOrEqualTo(0);
        }
    }

    [Fact]
    public void initRegisterSystem_ShouldEstablishRegisterBasicsHierarchy() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };

        // Act
        registerSystem.initRegisterSystem();

        // Assert
        var derivedManage = registerSystem.getRegisterManageOfManageType<DerivedTestRegisterManage>();
        derivedManage.Should().NotBeNull();

        // 验证继承关系
        foreach (var registerBasics in derivedManage!.values) {
            registerBasics.basics.Should().BeNull();
            registerBasics.registerManage.basicsRegisterManage.Should().BeOfType<TestRegisterManage>();
        }
    }

    [Fact]
    public void initRegisterSystem_ShouldRegisterDerivedRegisterBasics() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };

        // Act
        registerSystem.initRegisterSystem();

        // Assert
        var derivedManage = registerSystem.getRegisterManageOfManageType<DerivedTestRegisterManage>();
        derivedManage.Should().NotBeNull();
        derivedManage!.count.Should().Be(3); // ddd, eee, fff
        derivedManage.values.Should().Contain(r => r.name.path == "ddd");
        derivedManage.values.Should().Contain(r => r.name.path == "eee");
        derivedManage.values.Should().Contain(r => r.name.path == "fff");
    }

    [Fact]
    public void initRegisterSystem_ShouldSetRegisterBasicsTestValues() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };

        // Act
        registerSystem.initRegisterSystem();

        // Assert
        var testManage = registerSystem.getRegisterManageOfManageType<TestRegisterManage>();
        testManage.Should().NotBeNull();

        // 验证 TestValue 属性
        var aaaBasics = testManage!.get(new ResourceLocation(testManage.name.domain, "aaa"));
        var bbbBasics = testManage.get(new ResourceLocation(testManage.name.domain, "bbb"));
        var cccBasics = testManage.get(new ResourceLocation(testManage.name.domain, "ccc"));

        aaaBasics.Should().NotBeNull();
        bbbBasics.Should().NotBeNull();
        cccBasics.Should().NotBeNull();

        aaaBasics!.TestValue.Should().Be("a");
        bbbBasics!.TestValue.Should().Be("b");
        cccBasics!.TestValue.Should().Be("c");
    }

    [Fact]
    public void initRegisterSystem_ShouldRegisterAllRegisterBasicsInSortedSet() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };

        // Act
        registerSystem.initRegisterSystem();

        // Assert
        registerSystem.registerBasicsSortedSet.Should().NotBeNull();
        registerSystem.registerBasicsSortedSet.Should().NotBeEmpty();

        // 验证所有 RegisterBasics 都在 SortedSet 中
        var testManage = registerSystem.getRegisterManageOfManageType<TestRegisterManage>();
        var derivedManage = registerSystem.getRegisterManageOfManageType<DerivedTestRegisterManage>();

        testManage.Should().NotBeNull();
        derivedManage.Should().NotBeNull();

        foreach (var registerBasics in testManage!.values) {
            registerSystem.registerBasicsSortedSet.Should().Contain(registerBasics);
        }

        foreach (var registerBasics in derivedManage!.values) {
            registerSystem.registerBasicsSortedSet.Should().Contain(registerBasics);
        }
    }

    [Fact]
    public void initRegisterSystem_ShouldCallRegisterBasicsLifecycleMethods() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };

        // Act
        registerSystem.initRegisterSystem();

        // Assert
        // 验证所有 RegisterBasics 都被正确初始化
        foreach (var registerBasics in registerSystem.registerBasicsSortedSet) {
            registerBasics.registerSystem.Should().Be(registerSystem);
            registerBasics.registerManage.Should().NotBeNull();
        }
    }

    [Fact]
    public void initRegisterSystem_ShouldRegisterAdditionalRegisterBasics() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };

        // Act
        registerSystem.initRegisterSystem();

        // Assert
        var derivedManage = registerSystem.getRegisterManageOfManageType<DerivedTestRegisterManage>();
        derivedManage.Should().NotBeNull();

        // 验证每个 DerivedSimpleRegisterBasics 都有 qwq 子注册项
        foreach (var derivedBasics in derivedManage!.values) {
            derivedBasics.sonRegisterList.Should().NotBeEmpty();
            derivedBasics.sonRegisterList.Should().HaveCount(1);
            
            var qwqBasics = derivedBasics.sonRegisterList[0];
            qwqBasics.Should().BeOfType<InternalSimpleRegisterBasics>();
            qwqBasics.basics.Should().Be(derivedBasics);
        }
    }

    [Fact]
    public void initRegisterSystem_ShouldRegisterInternalSimpleRegisterBasics() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };

        // Act
        registerSystem.initRegisterSystem();

        // Assert
        var internalManage = registerSystem.getRegisterManageOfManageType<InternalSimpleRegisterManage>();
        internalManage.Should().NotBeNull();
        
        // 验证 InternalSimpleRegisterBasics 被注册
        internalManage!.count.Should().Be(3); // 每个 DerivedSimpleRegisterBasics 都有一个 qwq
        internalManage.values.Should().AllSatisfy(r => r.Should().BeOfType<InternalSimpleRegisterBasics>());
    }

    [Fact]
    public void initRegisterSystem_ShouldSetAdditionalRegisterBasicsProperties() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };

        // Act
        registerSystem.initRegisterSystem();

        // Assert
        var internalManage = registerSystem.getRegisterManageOfManageType<InternalSimpleRegisterManage>();
        internalManage.Should().NotBeNull();

        foreach (var qwqBasics in internalManage!.values) {
            qwqBasics.name.Should().NotBeNull();
            qwqBasics.registerManage.Should().Be(internalManage);
            qwqBasics.registerSystem.Should().Be(registerSystem);
            qwqBasics.basics.Should().NotBeNull();
            qwqBasics.basics!.Should().BeOfType<DerivedSimpleRegisterBasics>();
        }
    }

    [Fact]
    public void initRegisterSystem_ShouldRegisterAdditionalRegisterBasicsInSortedSet() {
        // Arrange
        RegisterSystem registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };

        // Act
        registerSystem.initRegisterSystem();

        // Assert
        var internalManage = registerSystem.getRegisterManageOfManageType<InternalSimpleRegisterManage>();
        internalManage.Should().NotBeNull();

        // 验证所有 qwq RegisterBasics 都在 SortedSet 中
        foreach (var qwqBasics in internalManage!.values) {
            registerSystem.registerBasicsSortedSet.Should().Contain(qwqBasics);
        }
    }
}

public class RegisterSystemErrorHandlingTests {

    private readonly TestLog _testLog;
    private readonly Assembly _testAssembly;
    private readonly ITestOutputHelper _testOutputHelper;

    public RegisterSystemErrorHandlingTests(ITestOutputHelper testOutputHelper) {
        _testOutputHelper = testOutputHelper;
        _testLog = new TestLog(testOutputHelper, enableConsoleOutput: true);
        _testAssembly = Assembly.GetExecutingAssembly();
    }

    [Fact]
    public void initRegisterSystem_WithEmptyAssemblySet_ShouldHandleGracefully() {
        // Arrange
        var registerSystem = new RegisterSystem {
            managedAssemblySet = [
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };

        // Act
        var action = () => registerSystem.initRegisterSystem();

        // Assert
        action.Should().NotThrow();
        registerSystem.allType.Should().BeEmpty();
        registerSystem.manageList.Should().BeEmpty();
    }

    [Fact]
    public void getRegisterManageOfManageType_WithNullType_ShouldReturnNull() {
        // Arrange
        var registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };
        registerSystem.initRegisterSystem();

        // Act
        var result = registerSystem.getRegisterManageOfManageType(null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void getRegisterManageOfName_WithNullName_ShouldReturnNull() {
        // Arrange
        var registerSystem = new RegisterSystem {
            managedAssemblySet = [
                _testAssembly
            ],
            log = _testLog,
            logLevel = LogLevel.DEBUG
        };
        registerSystem.initRegisterSystem();

        // Act
        var result = registerSystem.getRegisterManageOfName(null);

        // Assert
        result.Should().BeNull();
    }
}
