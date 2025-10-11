using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace EventBus.Tests;

/// <summary>
/// 异常处理测试
/// </summary>
public class ExceptionHandlingTests {

    public class TestEvent : Event {
        public string message { get; set; } = "";
    }

    public class TestAsyncEvent : Event, IAsyncEvent {
        public System.Threading.CancellationToken token { get; set; } = System.Threading.CancellationToken.None;
    }

    public class TestYieldEvent : Event, IYieldEvent {
    }

    // 会抛出异常的监听器
    public class ExceptionListener {
        public bool beforeExceptionCalled = false;
        public bool afterExceptionCalled = false;

        [Event(priority = 100)]
        public void beforeException(TestEvent @event) {
            beforeExceptionCalled = true;
        }

        [Event(priority = 50)]
        public void throwException(TestEvent @event) {
            throw new InvalidOperationException("Test exception");
        }

        [Event(priority = 10)]
        public void afterException(TestEvent @event) {
            afterExceptionCalled = true;
        }
    }

    // 异步异常监听器
    public class AsyncExceptionListener {
        public bool beforeExceptionCalled = false;
        public bool afterExceptionCalled = false;

        [Event(concurrencyGroup = "test", priority = 100)]
        public async Task beforeException(TestAsyncEvent @event) {
            await Task.Delay(10);
            beforeExceptionCalled = true;
        }

        [Event(concurrencyGroup = "test", priority = 50)]
        public async Task throwException(TestAsyncEvent @event) {
            await Task.Delay(10);
            throw new InvalidOperationException("Async test exception");
        }

        [Event(concurrencyGroup = "test", priority = 10)]
        public async Task afterException(TestAsyncEvent @event) {
            await Task.Delay(10);
            afterExceptionCalled = true;
        }
    }

    // 可迭代异常监听器
    public class YieldExceptionListener {
        public bool beforeExceptionCalled = false;
        public bool afterExceptionCalled = false;

        [Event(priority = 100)]
        public IEnumerable<int> beforeException(TestYieldEvent @event) {
            beforeExceptionCalled = true;
            yield return 1;
        }

        [Event(priority = 50)]
        public IEnumerable<int> throwException(TestYieldEvent @event) {
            yield return 2;
            throw new InvalidOperationException("Yield test exception");
        }

        [Event(priority = 10)]
        public IEnumerable<int> afterException(TestYieldEvent @event) {
            afterExceptionCalled = true;
            yield return 3;
        }
    }

    // 自定义异常处理器
    public class CustomExceptionHandler : IEventExceptionHandle {
        public List<Exception> caughtExceptions = new List<Exception>();

        public bool doCatch(IEventBus iEventBus, IEventInvoke eventInvoke, Event @event, Exception exception) {
            caughtExceptions.Add(exception);
            return true; // 表示已处理
        }
    }

    [Fact]
    public void eventBus_Should_HandleExceptionAndContinue() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var listener = new ExceptionListener();

        // Act
        eventBus.put(listener);
        var testEvent = new TestEvent { message = "Test" };
        eventBus.onEvent(testEvent);

        // Assert - 异常前后的处理器都应该被调用
        listener.beforeExceptionCalled.Should().BeTrue();
        listener.afterExceptionCalled.Should().BeTrue();
    }

    [Fact]
    public async Task asyncEventBus_Should_HandleExceptionAndContinue() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var listener = new AsyncExceptionListener();

        // Act
        eventBus.put(listener);
        var testEvent = new TestAsyncEvent();
        await eventBus.onEventAsync(testEvent);

        // Assert
        listener.beforeExceptionCalled.Should().BeTrue();
        listener.afterExceptionCalled.Should().BeTrue();
    }

    [Fact]
    public void yieldEventBus_Should_HandleExceptionAndContinue() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var listener = new YieldExceptionListener();

        // Act
        eventBus.put(listener);
        var testEvent = new TestYieldEvent();
        var results = new List<object>();
        foreach (var item in eventBus.onEventYield(testEvent)) {
            results.Add(item);
        }

        // Assert - 异常前后的处理器都应该被调用
        listener.beforeExceptionCalled.Should().BeTrue();
        listener.afterExceptionCalled.Should().BeTrue();
        results.Should().Contain(1); // beforeException 的结果
        results.Should().Contain(3); // afterException 的结果
    }

    [Fact]
    public void customExceptionHandler_Should_CatchExceptions() {
        // Arrange
        var customHandler = new CustomExceptionHandler();
        var builder = new EventBus.EventBusBuilder();
        builder.addEventExceptionHandle(customHandler);
        var eventBus = new EventBus(builder);
        var listener = new ExceptionListener();

        // Act
        eventBus.put(listener);
        var testEvent = new TestEvent { message = "Test" };
        eventBus.onEvent(testEvent);

        // Assert
        customHandler.caughtExceptions.Should().HaveCount(1);
        customHandler.caughtExceptions[0].Should().BeOfType<InvalidOperationException>();
        customHandler.caughtExceptions[0].Message.Should().Be("Test exception");
    }

    [Fact]
    public void exceptionHandler_Should_ReceiveCorrectContext() {
        // Arrange
        var contextHandler = new ContextExceptionHandler();
        var builder = new EventBus.EventBusBuilder();
        builder.addEventExceptionHandle(contextHandler);
        var eventBus = new EventBus(builder);
        var listener = new ExceptionListener();

        // Act
        eventBus.put(listener);
        var testEvent = new TestEvent { message = "ContextTest" };
        eventBus.onEvent(testEvent);

        // Assert
        contextHandler.receivedEventBus.Should().BeSameAs(eventBus);
        contextHandler.receivedEvent.Should().BeSameAs(testEvent);
        contextHandler.receivedInvoke.Should().NotBeNull();
        contextHandler.receivedInvoke!.eventType.Should().Be(typeof(TestEvent));
    }

    public class ContextExceptionHandler : IEventExceptionHandle {
        public IEventBus? receivedEventBus;
        public Event? receivedEvent;
        public IEventInvoke? receivedInvoke;

        public bool doCatch(IEventBus iEventBus, IEventInvoke eventInvoke, Event @event, Exception exception) {
            receivedEventBus = iEventBus;
            receivedEvent = @event;
            receivedInvoke = eventInvoke;
            return true;
        }
    }

    [Fact]
    public void multipleExceptionHandlers_Should_ExecuteInOrder() {
        // Arrange
        var handler1 = new OrderedExceptionHandler { order = 1 };
        var handler2 = new OrderedExceptionHandler { order = 2 };
        var builder = new EventBus.EventBusBuilder();
        builder.addEventExceptionHandle(handler2); // 后添加的在前
        builder.addEventExceptionHandle(handler1);
        var eventBus = new EventBus(builder);
        var listener = new ExceptionListener();

        // Act
        eventBus.put(listener);
        var testEvent = new TestEvent { message = "Test" };
        eventBus.onEvent(testEvent);

        // Assert - 先添加的应该先被调用
        handler1.called.Should().BeTrue();
        handler2.called.Should().BeFalse(); // handler1返回true，handler2不会被调用
    }

    public class OrderedExceptionHandler : IEventExceptionHandle {
        public int order;
        public bool called = false;

        public bool doCatch(IEventBus iEventBus, IEventInvoke eventInvoke, Event @event, Exception exception) {
            called = true;
            return true; // 表示已处理，不继续传递
        }
    }
}
