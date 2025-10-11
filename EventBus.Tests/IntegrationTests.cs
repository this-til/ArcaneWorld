using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace EventBus.Tests;

/// <summary>
/// 集成测试 - 测试多个功能组合使用
/// </summary>
public class IntegrationTests {

    // 复杂的事件层级
    public class GameEvent : Event {
        public string gameId { get; set; } = "";
    }

    public class PlayerEvent : GameEvent {
        public string playerId { get; set; } = "";
    }

    public class PlayerDamageEvent : PlayerEvent {
        public int damage { get; set; }
    }

    public class PlayerHealEvent : PlayerEvent {
        public int healAmount { get; set; }
    }

    // 异步游戏事件
    public class AsyncGameEvent : GameEvent, IAsyncEvent {
        public CancellationToken token { get; set; } = CancellationToken.None;
    }

    // 可迭代游戏事件
    public class YieldGameEvent : GameEvent, IYieldEvent {
    }

    // 游戏系统监听器
    public class GameSystemListener {
        public List<string> eventLog = new List<string>();

        [Event(priority = 100)]
        public void onGameEvent(GameEvent @event) {
            eventLog.Add($"Game:{@event.gameId}");
        }

        [Event(priority = 90)]
        public void onPlayerEvent(PlayerEvent @event) {
            eventLog.Add($"Player:{@event.playerId}");
        }

        [Event(priority = 80)]
        public void onPlayerDamage(PlayerDamageEvent @event) {
            eventLog.Add($"Damage:{@event.damage}");
        }

        [Event(priority = 70)]
        public void onPlayerHeal(PlayerHealEvent @event) {
            eventLog.Add($"Heal:{@event.healAmount}");
        }
    }

    // 日志系统监听器
    public class LoggingSystemListener {
        public List<string> logs = new List<string>();

        [Event(priority = 50)] // 较低优先级，在游戏系统之后
        public void logGameEvent(GameEvent @event) {
            logs.Add($"[LOG] GameEvent: {@event.gameId}");
        }
    }

    // 统计系统监听器
    public class StatisticsListener {
        public int totalEvents = 0;
        public int playerEvents = 0;
        public int damageEvents = 0;

        [Event(priority = 40)]
        public void countGameEvent(GameEvent @event) {
            totalEvents++;
        }

        [Event(priority = 40)]
        public void countPlayerEvent(PlayerEvent @event) {
            playerEvents++;
        }

        [Event(priority = 40)]
        public void countDamageEvent(PlayerDamageEvent @event) {
            damageEvents++;
        }
    }

    [Fact]
    public void integration_Should_HandleComplexEventHierarchy() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var gameSystem = new GameSystemListener();
        var loggingSystem = new LoggingSystemListener();
        var statistics = new StatisticsListener();

        eventBus.put(gameSystem);
        eventBus.put(loggingSystem);
        eventBus.put(statistics);

        // Act - 发送不同层级的事件
        eventBus.onEvent(new GameEvent { gameId = "G1" });
        eventBus.onEvent(new PlayerEvent { gameId = "G2", playerId = "P1" });
        eventBus.onEvent(new PlayerDamageEvent { gameId = "G3", playerId = "P2", damage = 50 });

        // Assert - 验证事件继承机制
        gameSystem.eventLog.Should().Contain("Game:G1");
        gameSystem.eventLog.Should().Contain("Game:G2");
        gameSystem.eventLog.Should().Contain("Player:P1");
        gameSystem.eventLog.Should().Contain("Game:G3");
        gameSystem.eventLog.Should().Contain("Player:P2");
        gameSystem.eventLog.Should().Contain("Damage:50");

        // 验证统计
        statistics.totalEvents.Should().Be(3);
        statistics.playerEvents.Should().Be(2); // PlayerEvent 和 PlayerDamageEvent
        statistics.damageEvents.Should().Be(1);

        // 验证日志
        loggingSystem.logs.Should().HaveCount(3);
    }

    [Fact]
    public void integration_Should_RespectPriorityAcrossListeners() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var tracker = new ExecutionOrderTracker();

        eventBus.put(tracker);

        // Act
        eventBus.onEvent(new GameEvent { gameId = "Test" });

        // Assert - 验证执行顺序
        tracker.executionOrder.Should().ContainInOrder(100, 90, 80, 70);
    }

    public class ExecutionOrderTracker {
        public List<int> executionOrder = new List<int>();

        [Event(priority = 100)]
        public void handler1(GameEvent @event) => executionOrder.Add(100);

        [Event(priority = 90)]
        public void handler2(GameEvent @event) => executionOrder.Add(90);

        [Event(priority = 80)]
        public void handler3(GameEvent @event) => executionOrder.Add(80);

        [Event(priority = 70)]
        public void handler4(GameEvent @event) => executionOrder.Add(70);
    }

    [Fact]
    public async Task integration_Should_HandleAsyncEventsWithHierarchy() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var asyncListener = new AsyncGameListener();

        eventBus.put(asyncListener);

        // Act
        await eventBus.onEventAsync(new AsyncGameEvent { gameId = "AsyncTest" });

        // Assert
        asyncListener.asyncCalled.Should().BeTrue();
        asyncListener.gameEventCalled.Should().BeTrue();
    }

    public class AsyncGameListener {
        public bool asyncCalled = false;
        public bool gameEventCalled = false;

        [Event(concurrencyGroup = "game", priority = 100)]
        public async Task onAsyncGameEvent(AsyncGameEvent @event) {
            await Task.Delay(10);
            asyncCalled = true;
        }

        [Event(concurrencyGroup = "game", priority = 90)]
        public async Task onGameEvent(GameEvent @event) {
            await Task.Delay(10);
            gameEventCalled = true;
        }
    }

    [Fact]
    public void integration_Should_SupportDynamicRegistration() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var listener1 = new DynamicListener { id = "L1" };
        var listener2 = new DynamicListener { id = "L2" };

        // Act - 动态添加和移除监听器
        eventBus.put(listener1);
        eventBus.onEvent(new GameEvent { gameId = "E1" });

        eventBus.put(listener2);
        eventBus.onEvent(new GameEvent { gameId = "E2" });

        eventBus.remove(listener1);
        eventBus.onEvent(new GameEvent { gameId = "E3" });

        // Assert
        listener1.receivedEvents.Should().Equal("E1", "E2");
        listener2.receivedEvents.Should().Equal("E2", "E3");
    }

    public class DynamicListener {
        public string id { get; set; } = "";
        public List<string> receivedEvents = new List<string>();

        [Event]
        public void onEvent(GameEvent @event) {
            receivedEvents.Add(@event.gameId);
        }
    }

    [Fact]
    public void integration_Should_HandleExceptionsWithoutStoppingOthers() {
        // Arrange
        var exceptionHandler = new TestExceptionHandler();
        var builder = new EventBus.EventBusBuilder();
        builder.addEventExceptionHandle(exceptionHandler);
        var eventBus = new EventBus(builder);

        var listener1 = new ReliableListener();
        var listener2 = new FaultyListener();
        var listener3 = new ReliableListener();

        eventBus.put(listener1);
        eventBus.put(listener2);
        eventBus.put(listener3);

        // Act
        eventBus.onEvent(new GameEvent { gameId = "Test" });

        // Assert
        listener1.callCount.Should().Be(1);
        listener3.callCount.Should().Be(1);
        exceptionHandler.exceptionCount.Should().Be(1);
    }

    public class ReliableListener {
        public int callCount = 0;

        [Event]
        public void onEvent(GameEvent @event) {
            callCount++;
        }
    }

    public class FaultyListener {
        [Event]
        public void onEvent(GameEvent @event) {
            throw new InvalidOperationException("Faulty listener error");
        }
    }

    public class TestExceptionHandler : IEventExceptionHandle {
        public int exceptionCount = 0;

        public bool doCatch(IEventBus iEventBus, IEventInvoke eventInvoke, Event @event, Exception exception) {
            exceptionCount++;
            return true;
        }
    }

    [Fact]
    public void integration_Should_SupportMixedEventTypes() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var mixedListener = new MixedEventListener();

        eventBus.put(mixedListener);

        // Act
        eventBus.onEvent(new GameEvent { gameId = "Sync" });
        var yieldResults = eventBus.onEventYield(new YieldGameEvent { gameId = "Yield" }).Cast<object>().ToList();

        // Assert
        mixedListener.syncCalled.Should().BeTrue();
        mixedListener.yieldCalled.Should().BeTrue();
        yieldResults.Should().NotBeEmpty();
    }

    public class MixedEventListener {
        public bool syncCalled = false;
        public bool yieldCalled = false;

        [Event]
        public void onSyncEvent(GameEvent @event) {
            syncCalled = true;
        }

        [Event]
        public System.Collections.IEnumerable onYieldEvent(YieldGameEvent @event) {
            yieldCalled = true;
            yield return 1;
            yield return 2;
        }
    }
}
