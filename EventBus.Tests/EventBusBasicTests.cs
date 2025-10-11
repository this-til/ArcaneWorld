using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace EventBus.Tests;

/// <summary>
/// EventBus 基础功能测试
/// </summary>
public class EventBusBasicTests {

    // 测试事件类
    public class TestEvent : Event {
        public string message { get; set; } = "";
        public int value { get; set; }
    }

    public class DerivedEvent : TestEvent {
        public bool flag { get; set; }
    }

    // 测试监听器类
    public class TestListener {
        public List<string> receivedMessages = new List<string>();
        public int callCount = 0;

        [Event(priority = 0)]
        public void onTestEvent(TestEvent @event) {
            receivedMessages.Add(@event.message);
            callCount++;
        }
    }

    public class PriorityListener {
        public List<int> executionOrder = new List<int>();

        [Event(priority = 100)]
        public void highPriority(TestEvent @event) {
            executionOrder.Add(100);
        }

        [Event(priority = 50)]
        public void mediumPriority(TestEvent @event) {
            executionOrder.Add(50);
        }

        [Event(priority = 10)]
        public void lowPriority(TestEvent @event) {
            executionOrder.Add(10);
        }
    }

    public class StaticListener {
        public static int staticCallCount = 0;

        [Event]
        public static void onStaticEvent(TestEvent @event) {
            staticCallCount++;
        }
    }

    [Fact]
    public void eventBus_Should_RegisterAndTriggerEvent() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var listener = new TestListener();

        // Act
        eventBus.put(listener);
        var testEvent = new TestEvent { message = "Hello", value = 42 };
        eventBus.onEvent(testEvent);

        // Assert
        listener.receivedMessages.Should().ContainSingle("Hello");
        listener.callCount.Should().Be(1);
    }

    [Fact]
    public void eventBus_Should_SupportMultipleListeners() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var listener1 = new TestListener();
        var listener2 = new TestListener();

        // Act
        eventBus.put(listener1);
        eventBus.put(listener2);
        var testEvent = new TestEvent { message = "Test", value = 1 };
        eventBus.onEvent(testEvent);

        // Assert
        listener1.callCount.Should().Be(1);
        listener2.callCount.Should().Be(1);
    }

    [Fact]
    public void eventBus_Should_RemoveListener() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var listener = new TestListener();

        // Act
        eventBus.put(listener);
        eventBus.remove(listener);
        var testEvent = new TestEvent { message = "Test", value = 1 };
        eventBus.onEvent(testEvent);

        // Assert
        listener.callCount.Should().Be(0);
    }

    [Fact]
    public void eventBus_Should_RespectEventPriority() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var listener = new PriorityListener();

        // Act
        eventBus.put(listener);
        var testEvent = new TestEvent { message = "Priority Test", value = 1 };
        eventBus.onEvent(testEvent);

        // Assert - 优先级高的先执行
        listener.executionOrder.Should().ContainInOrder(100, 50, 10);
    }

    [Fact]
    public void eventBus_Should_SupportStaticMethods() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        StaticListener.staticCallCount = 0;

        // Act
        eventBus.put(typeof(StaticListener));
        var testEvent = new TestEvent { message = "Static", value = 1 };
        eventBus.onEvent(testEvent);

        // Assert
        StaticListener.staticCallCount.Should().Be(1);
    }

    [Fact]
    public void eventBus_Should_SupportEventInheritance() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var listener = new TestListener();

        // Act
        eventBus.put(listener);
        var derivedEvent = new DerivedEvent { message = "Derived", value = 10, flag = true };
        eventBus.onEvent(derivedEvent);

        // Assert - 派生事件应该被基类的监听器接收
        listener.callCount.Should().Be(1);
        listener.receivedMessages.Should().Contain("Derived");
    }

    [Fact]
    public void eventBus_Should_NotRegisterSameListenerTwice() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var listener = new TestListener();

        // Act
        eventBus.put(listener);
        eventBus.put(listener); // 尝试重复注册
        var testEvent = new TestEvent { message = "Test", value = 1 };
        eventBus.onEvent(testEvent);

        // Assert - 只应该被调用一次
        listener.callCount.Should().Be(1);
    }

    [Fact]
    public void eventBus_Should_ThrowExceptionForAsyncEventInOnEvent() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);

        // Act & Assert
        var asyncEvent = new TestAsyncEvent();
        eventBus.Invoking(eb => eb.onEvent(asyncEvent))
            .Should().Throw<NotSupportedException>();
    }

    // 用于测试异常的异步事件
    private class TestAsyncEvent : Event, IAsyncEvent {
        public System.Threading.CancellationToken token => System.Threading.CancellationToken.None;
    }
}
