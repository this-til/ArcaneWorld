using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace EventBus.Tests;

/// <summary>
/// 过滤器测试
/// </summary>
public class FilterTests {

    public class TestEvent : Event {
        public string message { get; set; } = "";
    }

    // 测试监听器
    public class TestListener {
        public int callCount = 0;

        [Event]
        public void onEvent(TestEvent @event) {
            callCount++;
        }
    }

    // 带排除特性的监听器
    [EventSupplierExclude(excludeInstance = true, excludeStatic = false)]
    public class ExcludedInstanceListener {
        public int instanceCallCount = 0;
        public static int staticCallCount = 0;

        [Event]
        public void onInstanceEvent(TestEvent @event) {
            instanceCallCount++;
        }

        [Event]
        public static void onStaticEvent(TestEvent @event) {
            staticCallCount++;
        }
    }

    [EventSupplierExclude(excludeInstance = false, excludeStatic = true)]
    public class ExcludedStaticListener {
        public int instanceCallCount = 0;
        public static int staticCallCount = 0;

        [Event]
        public void onInstanceEvent(TestEvent @event) {
            instanceCallCount++;
        }

        [Event]
        public static void onStaticEvent(TestEvent @event) {
            staticCallCount++;
        }
    }

    // 自定义事件注册过滤器
    public class CustomRegistrantFilter : IEventRegistrantFilter {
        public bool isFilter(IEventBus eventBus, object registrant) {
            return registrant is TestListener; // 过滤掉所有 TestListener
        }
    }

    // 自定义事件调用过滤器
    public class CustomInvokeFilter : IEventInvokeFilter {
        public bool isFilter(
            IEventBus eventBus,
            object? obj,
            Type objType,
            Type eventType,
            MethodInfo methodInfo,
            EventAttribute? eventAttribute) {
            // 过滤掉所有名为 "onEvent" 的方法
            return methodInfo.Name == "onEvent";
        }
    }

    [Fact]
    public void registrantFilter_Should_FilterExcludedInstances() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var listener = new ExcludedInstanceListener();
        ExcludedInstanceListener.staticCallCount = 0;

        // Act
        eventBus.put(listener); // 实例应该被过滤
        eventBus.put(typeof(ExcludedInstanceListener)); // 静态不应该被过滤
        var testEvent = new TestEvent { message = "Test" };
        eventBus.onEvent(testEvent);

        // Assert
        listener.instanceCallCount.Should().Be(0); // 实例方法不应该被调用
        ExcludedInstanceListener.staticCallCount.Should().Be(1); // 静态方法应该被调用
    }

    [Fact]
    public void registrantFilter_Should_FilterExcludedStatics() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);
        var listener = new ExcludedStaticListener();
        ExcludedStaticListener.staticCallCount = 0;

        // Act
        eventBus.put(listener); // 实例不应该被过滤
        eventBus.put(typeof(ExcludedStaticListener)); // 静态应该被过滤
        var testEvent = new TestEvent { message = "Test" };
        eventBus.onEvent(testEvent);

        // Assert
        listener.instanceCallCount.Should().Be(1); // 实例方法应该被调用
        ExcludedStaticListener.staticCallCount.Should().Be(0); // 静态方法不应该被调用
    }

    [Fact]
    public void customRegistrantFilter_Should_FilterRegistrant() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        builder.addEventRegistrantFilter(new CustomRegistrantFilter());
        var eventBus = new EventBus(builder);
        var listener = new TestListener();

        // Act
        eventBus.put(listener);
        var testEvent = new TestEvent { message = "Test" };
        eventBus.onEvent(testEvent);

        // Assert
        listener.callCount.Should().Be(0); // 应该被过滤掉
    }

    [Fact]
    public void customInvokeFilter_Should_FilterMethod() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        builder.addEventTriggerFilter(new CustomInvokeFilter());
        var eventBus = new EventBus(builder);
        var listener = new FilterMethodListener();

        // Act
        eventBus.put(listener);
        var testEvent = new TestEvent { message = "Test" };
        eventBus.onEvent(testEvent);

        // Assert
        listener.onEventCalled.Should().BeFalse(); // onEvent 应该被过滤
        listener.onOtherEventCalled.Should().BeTrue(); // onOtherEvent 不应该被过滤
    }

    public class FilterMethodListener {
        public bool onEventCalled = false;
        public bool onOtherEventCalled = false;

        [Event]
        public void onEvent(TestEvent @event) {
            onEventCalled = true;
        }

        [Event]
        public void onOtherEvent(TestEvent @event) {
            onOtherEventCalled = true;
        }
    }

    [Fact]
    public void typeFilter_Should_FilterPrimitiveTypes() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);

        // Act & Assert
        eventBus.put(42); // int 是基础类型，应该被过滤
        var testEvent = new TestEvent { message = "Test" };
        eventBus.onEvent(testEvent); // 不应该抛出异常
    }

    [Fact]
    public void typeFilter_Should_FilterStructTypes() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);

        // Act & Assert
        eventBus.put(new TestStruct()); // struct 应该被过滤
        var testEvent = new TestEvent { message = "Test" };
        eventBus.onEvent(testEvent); // 不应该抛出异常
    }

    public struct TestStruct {
        public int value;
    }

    [Fact]
    public void typeFilter_Should_FilterEnumTypes() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);

        // Act & Assert
        eventBus.put(TestEnum.Value1); // enum 应该被过滤
        var testEvent = new TestEvent { message = "Test" };
        eventBus.onEvent(testEvent); // 不应该抛出异常
    }

    public enum TestEnum {
        Value1,
        Value2
    }

    [Fact]
    public void typeFilter_Should_FilterEventBusItself() {
        // Arrange
        var builder = new EventBus.EventBusBuilder();
        var eventBus = new EventBus(builder);

        // Act & Assert
        eventBus.put(eventBus); // EventBus 不能注册自己
        var testEvent = new TestEvent { message = "Test" };
        eventBus.onEvent(testEvent); // 不应该抛出异常
    }
}
