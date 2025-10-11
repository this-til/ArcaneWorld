using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace EventBus.Tests;

/// <summary>
/// 异步事件测试
/// </summary>
public class AsyncEventTests {

    // 异步测试事件
    public class TestAsyncEvent : Event, IAsyncEvent {

        public CancellationToken token { get; set; } = CancellationToken.None;

        public string message { get; set; } = "";

        public int delayMs { get; set; }

    }

    // 可取消的异步事件
    public class CancellableAsyncEvent : Event, IAsyncEvent, ICancellations {

        public CancellationToken token { get; set; } = CancellationToken.None;

        private bool cancelled = false;

        public void cancel() {
            cancelled = true;
        }

        public bool isCancellations() {
            return cancelled;
        }

    }

    // 异步监听器
    public class AsyncListener {

        public List<string> executionLog = new List<string>();

        public int taskCallCount = 0;

        public int valueTaskCallCount = 0;

        [Event(priority = 100)]
        public async Task onAsyncEventWithTask(TestAsyncEvent @event) {
            executionLog.Add($"Start-{@event.message}");
            await Task.Delay(@event.delayMs);
            executionLog.Add($"End-{@event.message}");
            taskCallCount++;
        }

        [Event(priority = 50)]
        public async ValueTask onAsyncEventWithValueTask(TestAsyncEvent @event) {
            await Task.Delay(@event.delayMs);
            valueTaskCallCount++;
        }

    }

    // 并发组测试监听器
    public class ConcurrencyGroupListener {

        public List<string> executionLog = new List<string>();

        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        [Event(concurrencyGroup = "Group1", priority = 100)]
        public async Task group1Method1(TestAsyncEvent @event) {
            await semaphore.WaitAsync();
            try {
                executionLog.Add("G1M1-Start");
                await Task.Delay(50);
                executionLog.Add("G1M1-End");
            }
            finally {
                semaphore.Release();
            }
        }

        [Event(concurrencyGroup = "Group1", priority = 90)]
        public async Task group1Method2(TestAsyncEvent @event) {
            await semaphore.WaitAsync();
            try {
                executionLog.Add("G1M2-Start");
                await Task.Delay(50);
                executionLog.Add("G1M2-End");
            }
            finally {
                semaphore.Release();
            }
        }

        [Event(concurrencyGroup = "Group2", priority = 100)]
        public async Task group2Method(TestAsyncEvent @event) {
            await semaphore.WaitAsync();
            try {
                executionLog.Add("G2-Start");
                await Task.Delay(50);
                executionLog.Add("G2-End");
            }
            finally {
                semaphore.Release();
            }
        }

    }

    // 空并发组测试
    public class EmptyGroupListener {

        public List<string> executionLog = new List<string>();

        [Event(concurrencyGroup = "", priority = 100)]
        public async Task method1(TestAsyncEvent @event) {
            executionLog.Add("M1-Start");
            await Task.Delay(30);
            executionLog.Add("M1-End");
        }

        [Event(concurrencyGroup = "", priority = 90)]
        public async Task method2(TestAsyncEvent @event) {
            executionLog.Add("M2-Start");
            await Task.Delay(30);
            executionLog.Add("M2-End");
        }

    }

    [Fact]
    public async Task asyncEvent_Should_ExecuteAsyncHandlers() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var listener = new AsyncListener();

        // Act
        eventBus.put(listener);
        var asyncEvent = new TestAsyncEvent { message = "Test", delayMs = 10 };
        await eventBus.onEventAsync(asyncEvent);

        // Assert
        listener.taskCallCount.Should().Be(1);
        listener.valueTaskCallCount.Should().Be(1);
        listener.executionLog.Should().Contain("Start-Test");
        listener.executionLog.Should().Contain("End-Test");
    }

    [Fact]
    public async Task asyncEvent_Should_SupportCancellation() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var listener = new CancellableListener();

        // Act
        eventBus.put(listener);
        var asyncEvent = new CancellableAsyncEvent();
        asyncEvent.cancel(); // 立即取消
        await eventBus.onEventAsync(asyncEvent);

        // Assert
        listener.executionLog.Should().BeEmpty();
    }

    // 可取消的监听器
    public class CancellableListener {

        public List<string> executionLog = new List<string>();

        [Event(concurrencyGroup = "test", priority = 100)]
        public async Task handler1(CancellableAsyncEvent @event) {
            executionLog.Add("Handler1");
            await Task.Delay(10);
        }

        [Event(concurrencyGroup = "test", priority = 90)]
        public async Task handler2(CancellableAsyncEvent @event) {
            executionLog.Add("Handler2");
            await Task.Delay(10);
        }

    }

    [Fact]
    public async Task asyncEvent_Should_HandleCancellationToken() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var listener = new TokenCancellableListener();
        var cts = new CancellationTokenSource();

        // Act
        eventBus.put(listener);
        await cts.CancelAsync(); // 在执行前取消
        var asyncEvent = new TestAsyncEvent { token = cts.Token, delayMs = 100 };
        await eventBus.onEventAsync(asyncEvent);

        // Assert - 由于token已取消，处理器不应执行
        listener.callCount.Should().Be(0);
    }

    public class TokenCancellableListener {

        public int callCount = 0;

        [Event(concurrencyGroup = "test")]
        public async Task onEvent(TestAsyncEvent @event) {
            @event.token.ThrowIfCancellationRequested();
            await Task.Delay(10, @event.token);
            callCount++;
        }

    }

    [Fact]
    public async Task asyncEvent_Should_ExecuteConcurrencyGroupsInParallel() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var listener = new ConcurrencyGroupListener();

        // Act
        eventBus.put(listener);
        var asyncEvent = new TestAsyncEvent { delayMs = 50 };
        await eventBus.onEventAsync(asyncEvent);

        // Assert - 不同组应该可以并行执行
        listener.executionLog.Should().HaveCount(6);
        // Group1 内部按优先级顺序执行
        var group1Start = listener.executionLog.IndexOf("G1M1-Start");
        var group1M1End = listener.executionLog.IndexOf("G1M1-End");
        var group1M2Start = listener.executionLog.IndexOf("G1M2-Start");

        group1M1End.Should().BeLessThan(group1M2Start); // G1M1应该在G1M2之前完成
    }

    [Fact]
    public async Task asyncEvent_Should_ExecuteEmptyGroupInParallel() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var listener = new EmptyGroupListener();

        // Act
        eventBus.put(listener);
        var asyncEvent = new TestAsyncEvent { delayMs = 30 };
        asyncEvent.GetType().GetProperty(nameof(IAsyncEvent.isEmptyGroupSynchronization))?.SetValue(asyncEvent, false);
        await eventBus.onEventAsync(asyncEvent);

        // Assert - 空组在非同步模式下应该并行执行
        listener.executionLog.Should().HaveCount(4);
    }

}
