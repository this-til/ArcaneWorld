using System;
using System.Threading.Tasks;
using CommonUtil.Container;
using FluentAssertions;
using Xunit;

namespace CommonUtil.Tests.Container;

public class ValueTests {

    [Fact]
    public void Constructor_WithValue_ShouldSetValue() {
        // Arrange
        const string expectedValue = "test";

        // Act
        var value = new Value<string>(expectedValue);

        // Assert
        value.value.Should().Be(expectedValue);
    }

    [Fact]
    public void Constructor_WithNullValue_ShouldSetNull() {
        // Arrange & Act
        var value = new Value<string?>((string?)null);

        // Assert
        value.value.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithValueFactory_ShouldLazyLoad() {
        // Arrange
        var factoryCallCount = 0;
        const string expectedValue = "lazy";
        var value = new Value<string>(
            () => {
                factoryCallCount++;
                return expectedValue;
            }
        );

        // Act & Assert
        factoryCallCount.Should().Be(0); // 工厂方法尚未被调用
        value.value.Should().Be(expectedValue);
        factoryCallCount.Should().Be(1); // 工厂方法被调用一次
        value.value.Should().Be(expectedValue);
        factoryCallCount.Should().Be(1); // 工厂方法不再被调用
    }

    [Fact]
    public void Constructor_WithValueFactoryReturningNull_ShouldNotClearFactory() {
        // Arrange
        var factoryCallCount = 0;
        var value = new Value<string?>(
            () => {
                factoryCallCount++;
                return null;
            }
        );

        // Act & Assert
        value.value.Should().BeNull();
        factoryCallCount.Should().Be(1);
        value.value.Should().BeNull();
        factoryCallCount.Should().Be(2); // 工厂方法被再次调用
    }

    [Fact]
    public void ImplicitConversion_FromValue_ShouldWork() {
        // Arrange
        const string expectedValue = "test";
        var value = new Value<string>(expectedValue);

        // Act
        string result = value;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Fact]
    public void ImplicitConversion_FromValueFactory_ShouldWork() {
        // Arrange
        const string expectedValue = "factory";
        Func<string> factory = () => expectedValue;

        // Act
        Value<string> value = factory;

        // Assert
        value.value.Should().Be(expectedValue);
    }

    [Fact]
    public void ImplicitConversion_FromDirectValue_ShouldWork() {
        // Arrange
        const string expectedValue = "direct";

        // Act
        Value<string> value = expectedValue;

        // Assert
        value.value.Should().Be(expectedValue);
    }

    [Fact]
    public async Task Value_ShouldBeThreadSafe() {
        // Arrange
        var factoryCallCount = 0;
        var value = new Value<string>(
            () => {
                factoryCallCount++;
                return "thread-safe";
            }
        );

        // Act
        var tasks = new Task[10];
        for (int i = 0; i < 10; i++) {
            tasks[i] = Task.Run(() => _ = value.value);
        }

        await Task.WhenAll(tasks);

        // Assert
        factoryCallCount.Should().Be(1); // 工厂方法只被调用一次
        value.value.Should().Be("thread-safe");
    }

    [Fact]
    public void Value_WithExceptionInFactory_ShouldPropagateException() {
        // Arrange
        var value = new Value<string>(() => throw new InvalidOperationException("Factory error"));

        // Act & Assert
        var action = () => _ = value.value;
        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Factory error");
    }

}
