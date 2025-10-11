using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace EventBus.Tests;

/// <summary>
/// await 转换器测试
/// </summary>
public class ConvertAwaitTests {

    public class TestAsyncEvent : Event, IAsyncEvent {
        public CancellationToken token { get; set; } = CancellationToken.None;
    }

    [Fact]
    public void taskConvertAwaiter_Should_ConvertTask() {
        // Arrange
        var converter = TaskConvertAwaiter.instance;
        var task = Task.CompletedTask;

        // Act
        var canConvert = converter.canConvert(typeof(Task));
        var awaiter = converter.convert(task);

        // Assert
        canConvert.Should().BeTrue();
        awaiter.Should().NotBeNull();
    }

    [Fact]
    public void taskConvertAwaiter_Should_ConvertGenericTask() {
        // Arrange
        var converter = TaskConvertAwaiter.instance;
        var task = Task.FromResult(42);

        // Act
        var canConvert = converter.canConvert(typeof(Task<int>));
        var awaiter = converter.convert(task);

        // Assert
        canConvert.Should().BeTrue();
        awaiter.Should().NotBeNull();
    }

    [Fact]
    public void taskConvertAwaiter_Should_NotConvertNonTask() {
        // Arrange
        var converter = TaskConvertAwaiter.instance;

        // Act
        var canConvert = converter.canConvert(typeof(int));

        // Assert
        canConvert.Should().BeFalse();
    }

    [Fact]
    public void taskConvertAwaiter_Should_ReturnNullForNonTask() {
        // Arrange
        var converter = TaskConvertAwaiter.instance;

        // Act
        var awaiter = converter.convert(42);

        // Assert
        awaiter.Should().BeNull();
    }

    [Fact]
    public void valueTaskConvertAwaiter_Should_ConvertValueTask() {
        // Arrange
        var converter = ValueTaskConvertAwaiter.instance;
        var valueTask = ValueTask.CompletedTask;

        // Act
        var canConvert = converter.canConvert(typeof(ValueTask));
        var awaiter = converter.convert(valueTask);

        // Assert
        canConvert.Should().BeTrue();
        awaiter.Should().NotBeNull();
    }

    [Fact]
    public void valueTaskConvertAwaiter_Should_NotConvertNonValueTask() {
        // Arrange
        var converter = ValueTaskConvertAwaiter.instance;

        // Act
        var canConvert = converter.canConvert(typeof(string));

        // Assert
        canConvert.Should().BeFalse();
    }

    [Fact]
    public async Task eventBus_Should_AutoConvertTaskReturnValue() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var listener = new TaskReturnListener();

        // Act
        eventBus.put(listener);
        var asyncEvent = new TestAsyncEvent();
        await eventBus.onEventAsync(asyncEvent);

        // Assert
        listener.executed.Should().BeTrue();
    }

    public class TaskReturnListener {
        public bool executed = false;

        [Event(concurrencyGroup = "test")]
        public Task onEvent(TestAsyncEvent @event) {
            return Task.Run(() => {
                Task.Delay(10).Wait();
                executed = true;
            });
        }
    }

    [Fact]
    public async Task eventBus_Should_AutoConvertValueTaskReturnValue() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var listener = new ValueTaskReturnListener();

        // Act
        eventBus.put(listener);
        var asyncEvent = new TestAsyncEvent();
        await eventBus.onEventAsync(asyncEvent);

        // Assert
        listener.executed.Should().BeTrue();
    }

    public class ValueTaskReturnListener {
        public bool executed = false;

        [Event(concurrencyGroup = "test")]
        public ValueTask onEvent(TestAsyncEvent @event) {
            return new ValueTask(Task.Run(() => {
                Task.Delay(10).Wait();
                executed = true;
            }));
        }
    }

    [Fact]
    public async Task eventBus_Should_WaitForTaskCompletion() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var listener = new DelayedTaskListener();

        // Act
        eventBus.put(listener);
        var asyncEvent = new TestAsyncEvent();
        var startTime = DateTime.UtcNow;
        await eventBus.onEventAsync(asyncEvent);
        var elapsed = DateTime.UtcNow - startTime;

        // Assert - 应该等待任务完成
        listener.executed.Should().BeTrue();
        elapsed.Should().BeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(50));
    }

    public class DelayedTaskListener {
        public bool executed = false;

        [Event(concurrencyGroup = "test")]
        public Task onEvent(TestAsyncEvent @event) {
            return Task.Run(async () => {
                await Task.Delay(50);
                executed = true;
            });
        }
    }

    [Fact]
    public async Task customConvertAwait_Should_BeUsedByEventBus() {
        // Arrange
        var customConverter = new CustomConvertAwait();
        var builder = new EventBus.EventBusBuilder();
        builder.convertAwaitList.Insert(0, customConverter);
        var eventBus = new EventBus(builder);
        var listener = new CustomAwaitableListener();

        // Act
        eventBus.put(listener);
        var testEvent = new TestAsyncEvent();
        await eventBus.onEventAsync(testEvent);

        // Assert
        customConverter.convertCalled.Should().BeTrue();
    }

    public class CustomConvertAwait : IConvertAwait {
        public bool convertCalled = false;

        public bool canConvert(Type type) {
            return type == typeof(CustomAwaitable);
        }

        public IGetAwaiter? convert(object obj) {
            convertCalled = true;
            return obj is CustomAwaitable ca ? new CustomGetAwaiter(ca) : null;
        }
    }

    public class CustomAwaitable {
        public bool completed = false;
    }

    public class CustomGetAwaiter : IGetAwaiter {
        private readonly CustomAwaitable awaitable;

        public CustomGetAwaiter(CustomAwaitable awaitable) {
            this.awaitable = awaitable;
        }

        public IAwaiter GetAwaiter() {
            return new CustomAwaiter(awaitable);
        }
    }

    public class CustomAwaiter : IAwaiter {
        private readonly CustomAwaitable awaitable;

        public CustomAwaiter(CustomAwaitable awaitable) {
            this.awaitable = awaitable;
        }

        public bool IsCompleted => true; // 立即完成

        public void GetResult() {
            awaitable.completed = true;
        }

        public void OnCompleted(Action continuation) {
            awaitable.completed = true;
            continuation();
        }
    }

    public class CustomAwaitableListener {
        [Event(concurrencyGroup = "test")]
        public CustomAwaitable onEvent(TestAsyncEvent @event) {
            return new CustomAwaitable();
        }
    }
}
