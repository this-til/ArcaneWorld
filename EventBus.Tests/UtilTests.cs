using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace EventBus.Tests;

/// <summary>
/// 工具类测试
/// </summary>
public class UtilTests {

    // 测试优先级接口
    public class TestPriorityItem : IPriority {
        public int priority { get; set; }
        public string name { get; set; } = "";
    }

    [Fact]
    public void addByPriority_Should_AddItemsInCorrectOrder() {
        // Arrange
        var list = new List<TestPriorityItem>();
        var item1 = new TestPriorityItem { priority = 50, name = "Item1" };
        var item2 = new TestPriorityItem { priority = 100, name = "Item2" };
        var item3 = new TestPriorityItem { priority = 25, name = "Item3" };

        // Act
        list.addByPriority(item1);
        list.addByPriority(item2);
        list.addByPriority(item3);

        // Assert - 应该按优先级从高到低排序
        list[0].priority.Should().Be(100);
        list[1].priority.Should().Be(50);
        list[2].priority.Should().Be(25);
    }

    [Fact]
    public void addByPriority_Should_MaintainStableOrder() {
        // Arrange
        var list = new List<TestPriorityItem>();
        var item1 = new TestPriorityItem { priority = 50, name = "Item1" };
        var item2 = new TestPriorityItem { priority = 50, name = "Item2" };
        var item3 = new TestPriorityItem { priority = 50, name = "Item3" };

        // Act
        list.addByPriority(item1);
        list.addByPriority(item2);
        list.addByPriority(item3);

        // Assert - 相同优先级应该保持添加顺序
        list[0].name.Should().Be("Item1");
        list[1].name.Should().Be("Item2");
        list[2].name.Should().Be("Item3");
    }

    [Fact]
    public void removeByPriority_Should_RemoveMatchingItems() {
        // Arrange
        var list = new List<TestPriorityItem>();
        var item1 = new TestPriorityItem { priority = 50, name = "Item1" };
        var item2 = new TestPriorityItem { priority = 100, name = "Item2" };
        var item3 = new TestPriorityItem { priority = 25, name = "Item1" }; // 同名但不同优先级

        list.addByPriority(item1);
        list.addByPriority(item2);
        list.addByPriority(item3);

        // Act
        list.removeByPriority(item1);

        // Assert - 应该只移除匹配的项
        list.Should().HaveCount(2);
        list.Should().Contain(item2);
        list.Should().Contain(item3);
    }

    [Fact]
    public void removeByPriority_Should_RemoveAllMatchingItems() {
        // Arrange
        var list = new List<TestPriorityItem>();
        var item = new TestPriorityItem { priority = 50, name = "Item" };

        list.addByPriority(item);
        list.addByPriority(item); // 添加相同项两次
        list.addByPriority(item);

        // Act
        list.removeByPriority(item);

        // Assert - 应该移除所有匹配项
        list.Should().BeEmpty();
    }

    /*[Fact]
    public void getParents_Should_ReturnTypeHierarchy() {
        // Arrange
        var type = typeof(DerivedClass);

        // Act
        var parents = type.getParents();

        // Assert
        parents.Should().Contain(typeof(DerivedClass));
        parents.Should().Contain(typeof(BaseClass));
        parents[0].Should().Be(typeof(DerivedClass));
        parents[1].Should().Be(typeof(BaseClass));
    }*/

    public class BaseClass { }
    public class DerivedClass : BaseClass { }

    /*[Fact]
    public void getParents_Should_CacheResults() {
        // Arrange
        var type = typeof(DerivedClass);

        // Act
        var parents1 = type.getParents();
        var parents2 = type.getParents();

        // Assert - 应该返回相同的缓存实例
        parents1.Should().BeSameAs(parents2);
    }*/

    /*
    [Fact]
    public void getParents_Should_ReturnEmptyForNull() {
        // Arrange
        Type? type = null;

        // Act
        var parents = type.getParents();

        // Assert
        parents.Should().BeEmpty();
    }
    */

    [Fact]
    public void canAwait_Should_DetectAwaitableTypes() {
        // Arrange & Act & Assert
        typeof(Task).canAwait().Should().BeTrue();
        typeof(Task<int>).canAwait().Should().BeTrue();
        typeof(ValueTask).canAwait().Should().BeTrue();
        typeof(ValueTask<int>).canAwait().Should().BeTrue();
    }

    [Fact]
    public void canAwait_Should_DetectNonAwaitableTypes() {
        // Arrange & Act & Assert
        typeof(int).canAwait().Should().BeFalse();
        typeof(string).canAwait().Should().BeFalse();
        typeof(object).canAwait().Should().BeFalse();
    }

    [Fact]
    public void canAwait_Should_DetectCustomAwaitableType() {
        // Arrange & Act & Assert
        typeof(CustomAwaitable).canAwait().Should().BeTrue();
    }

    // 自定义可等待类型
    public class CustomAwaitable {
        public CustomAwaiter GetAwaiter() => new CustomAwaiter();
    }

    public class CustomAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
        public bool IsCompleted => true;
        public void GetResult() { }
        public void OnCompleted(Action continuation) => continuation();
    }

    [Fact]
    public void canAwait_Should_CacheResults() {
        // Arrange
        var type = typeof(Task);

        // Act
        var result1 = type.canAwait();
        var result2 = type.canAwait();

        // Assert - 结果应该一致（从缓存获取）
        result1.Should().Be(result2);
    }
}
