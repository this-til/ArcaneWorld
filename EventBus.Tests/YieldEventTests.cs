using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace EventBus.Tests;

/// <summary>
/// 可迭代事件测试
/// </summary>
public class YieldEventTests {

    // 可迭代事件
    public class TestYieldEvent : Event, IYieldEvent {
        public string message { get; set; } = "";
        public int count { get; set; }
    }

    // 可取消的可迭代事件
    public class CancellableYieldEvent : Event, IYieldEvent, ICancellations {
        private bool cancelled = false;

        public void cancel() {
            cancelled = true;
        }

        public bool isCancellations() {
            return cancelled;
        }
    }

    // 可迭代监听器 - 返回 IEnumerable
    public class YieldListener {
        public List<string> executionLog = new List<string>();

        [Event(priority = 100)]
        public IEnumerable<int> onYieldEvent(TestYieldEvent @event) {
            executionLog.Add($"Start-{@event.message}");
            for (int i = 0; i < @event.count; i++) {
                executionLog.Add($"Yield-{i}");
                yield return i;
            }
            executionLog.Add($"End-{@event.message}");
        }
    }

    // 可迭代监听器 - 返回 IEnumerator
    public class EnumeratorListener {
        public List<string> executionLog = new List<string>();

        [Event(priority = 100)]
        public IEnumerator onYieldEvent(TestYieldEvent @event) {
            executionLog.Add("Start");
            for (int i = 0; i < @event.count; i++) {
                executionLog.Add($"Item-{i}");
                yield return i * 10;
            }
            executionLog.Add("End");
        }
    }

    // 混合监听器
    public class MixedYieldListener {
        public List<string> executionLog = new List<string>();

        [Event(priority = 100)]
        public IEnumerable<string> method1(TestYieldEvent @event) {
            executionLog.Add("M1-Start");
            yield return "A";
            yield return "B";
            executionLog.Add("M1-End");
        }

        [Event(priority = 90)]
        public IEnumerator method2(TestYieldEvent @event) {
            executionLog.Add("M2-Start");
            yield return 1;
            yield return 2;
            executionLog.Add("M2-End");
        }

        [Event(priority = 80)]
        public void normalMethod(TestYieldEvent @event) {
            executionLog.Add("Normal");
        }
    }

    [Fact]
    public void yieldEvent_Should_ExecuteAndYieldValues() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var listener = new YieldListener();

        // Act
        eventBus.put(listener);
        var yieldEvent = new TestYieldEvent { message = "Test", count = 3 };
        var results = eventBus.onEventYield(yieldEvent).Cast<int>().ToList();

        // Assert
        results.Should().Equal(0, 1, 2);
        listener.executionLog.Should().Contain("Start-Test");
        listener.executionLog.Should().Contain("End-Test");
        listener.executionLog.Should().Contain("Yield-0");
        listener.executionLog.Should().Contain("Yield-1");
        listener.executionLog.Should().Contain("Yield-2");
    }

    [Fact]
    public void yieldEvent_Should_SupportEnumerator() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var listener = new EnumeratorListener();

        // Act
        eventBus.put(listener);
        var yieldEvent = new TestYieldEvent { count = 3 };
        var results = eventBus.onEventYield(yieldEvent).Cast<int>().ToList();

        // Assert
        results.Should().Equal(0, 10, 20);
        listener.executionLog.Should().Contain("Start");
        listener.executionLog.Should().Contain("End");
    }

    [Fact]
    public void yieldEvent_Should_SupportMixedHandlers() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var listener = new MixedYieldListener();

        // Act
        eventBus.put(listener);
        var yieldEvent = new TestYieldEvent { count = 2 };
        var results = eventBus.onEventYield(yieldEvent).Cast<object>().ToList();

        // Assert
        results.Should().HaveCount(4); // 2 from method1 + 2 from method2
        listener.executionLog.Should().Contain("M1-Start");
        listener.executionLog.Should().Contain("M1-End");
        listener.executionLog.Should().Contain("M2-Start");
        listener.executionLog.Should().Contain("M2-End");
        listener.executionLog.Should().Contain("Normal");
    }

    [Fact]
    public void yieldEvent_Should_RespectPriority() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var listener = new MixedYieldListener();

        // Act
        eventBus.put(listener);
        var yieldEvent = new TestYieldEvent { count = 1 };
        var enumerator = eventBus.onEventYield(yieldEvent).GetEnumerator();

        // 收集所有结果
        var results = new List<object>();
        while (enumerator.MoveNext()) {
            results.Add(enumerator.Current);
        }

        // Assert - 验证执行顺序
        listener.executionLog[0].Should().Be("M1-Start");
        listener.executionLog.Last().Should().Be("Normal");
    }

    [Fact]
    public void yieldEvent_Should_SupportCancellation() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var listener = new CancellableYieldListener();

        // Act
        eventBus.put(listener);
        var yieldEvent = new CancellableYieldEvent();
        yieldEvent.cancel();
        var results = eventBus.onEventYield(yieldEvent).Cast<object>().ToList();

        // Assert
        results.Should().BeEmpty();
        listener.executionLog.Should().BeEmpty();
    }

    // 可取消的可迭代监听器
    public class CancellableYieldListener {
        public List<string> executionLog = new List<string>();

        [Event]
        public IEnumerable<int> onEvent(CancellableYieldEvent @event) {
            executionLog.Add("Start");
            for (int i = 0; i < 10; i++) {
                if (@event.isCancellations()) {
                    yield break;
                }
                executionLog.Add($"Yield-{i}");
                yield return i;
            }
            executionLog.Add("End");
        }
    }

    [Fact]
    public void yieldEvent_Should_StopOnCancellation() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var listener = new PartialCancellableListener();

        // Act
        eventBus.put(listener);
        var yieldEvent = new CancellableYieldEvent();
        var results = new List<object>();
        var count = 0;
        foreach (var item in eventBus.onEventYield(yieldEvent)) {
            results.Add(item);
            count++;
            if (count == 2) {
                yieldEvent.cancel(); // 在迭代过程中取消
            }
        }

        // Assert
        results.Should().HaveCountLessThan(10); // 应该少于全部10个
        listener.executionLog.Should().NotContain("End");
    }

    public class PartialCancellableListener {
        public List<string> executionLog = new List<string>();

        [Event]
        public IEnumerable<int> onEvent(CancellableYieldEvent @event) {
            executionLog.Add("Start");
            for (int i = 0; i < 10; i++) {
                executionLog.Add($"Yield-{i}");
                yield return i;
            }
            executionLog.Add("End");
        }
    }
}
