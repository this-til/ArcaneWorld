using System;
using System.Collections.Generic;
using System.Linq;
using CommonUtil.Container;
using FluentAssertions;
using Xunit;

namespace CommonUtil.Tests.Container;

public class ValueListTests
{
    [Fact]
    public void Constructor_ShouldCreateEmptyList()
    {
        // Arrange & Act
        var valueList = new ValueList<string>();

        // Assert
        valueList.Count.Should().Be(0);
        valueList.allStorage.Should().BeEmpty();
    }

    [Fact]
    public void Add_WithValue_ShouldAddToLazyStorage()
    {
        // Arrange
        var valueList = new ValueList<string>();
        var value = new Value<string>("test");

        // Act
        valueList.Add(value);

        // Assert
        valueList.allStorage.Should().HaveCount(1);
        valueList.allStorage.First().Should().Be(value);
        valueList.Count.Should().Be(1); // 这会触发 map() 方法
    }

    [Fact]
    public void Add_WithNullValue_ShouldNotAddToStorage()
    {
        // Arrange
        var valueList = new ValueList<string>();
        var value = new Value<string?>((string?)null);

        // Act
        valueList.Add(value);

        // Assert
        valueList.allStorage.Should().HaveCount(1);
        valueList.Count.Should().Be(0); // null 值不会被添加到 storage
    }

    [Fact]
    public void Add_WithValueFactory_ShouldLazyLoad()
    {
        // Arrange
        var valueList = new ValueList<string>();
        var factoryCallCount = 0;
        var value = new Value<string>(() =>
        {
            factoryCallCount++;
            return "lazy";
        });

        // Act
        valueList.Add(value);

        // Assert
        valueList.allStorage.Should().HaveCount(1);
        factoryCallCount.Should().Be(0); // 工厂方法尚未被调用
        valueList.Count.Should().Be(1); // 这会触发 map() 和工厂方法调用
        factoryCallCount.Should().Be(1);
    }

    [Fact]
    public void AddRange_WithMultipleValues_ShouldAddAll()
    {
        // Arrange
        var valueList = new ValueList<string>();
        var values = new[]
        {
            new Value<string>("first"),
            new Value<string>("second"),
            new Value<string>("third")
        };

        // Act
        valueList.AddRange(values);

        // Assert
        valueList.allStorage.Should().HaveCount(3);
        valueList.Count.Should().Be(3);
    }

    [Fact]
    public void AddRange_WithMixedValues_ShouldHandleNulls()
    {
        // Arrange
        var valueList = new ValueList<string>();
        var values = new[]
        {
            new Value<string>("valid"),
            new Value<string?>((string?)null),
            new Value<string>("another")
        };

        // Act
        valueList.AddRange(values);

        // Assert
        valueList.allStorage.Should().HaveCount(3);
        valueList.Count.Should().Be(2); // 只有非 null 值被添加到 storage
    }

    [Fact]
    public void Contains_WithExistingValue_ShouldReturnTrue()
    {
        // Arrange
        var valueList = new ValueList<string>();
        var value = new Value<string>("test");
        valueList.Add(value);

        // Act
        var result = valueList.contains("test");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_WithNonExistingValue_ShouldReturnFalse()
    {
        // Arrange
        var valueList = new ValueList<string>();
        var value = new Value<string>("test");
        valueList.Add(value);

        // Act
        var result = valueList.contains("other");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Contains_WithEmptyList_ShouldReturnFalse()
    {
        // Arrange
        var valueList = new ValueList<string>();

        // Act
        var result = valueList.contains("test");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Count_ShouldTriggerMap()
    {
        // Arrange
        var valueList = new ValueList<string>();
        var value1 = new Value<string>("first");
        var value2 = new Value<string>("second");
        valueList.Add(value1);
        valueList.Add(value2);

        // Act
        var count = valueList.Count;

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public void AllStorage_ShouldReturnAllAddedValues()
    {
        // Arrange
        var valueList = new ValueList<string>();
        var value1 = new Value<string>("first");
        var value2 = new Value<string>("second");
        var value3 = new Value<string?>((string?)null);

        // Act
        valueList.Add(value1);
        valueList.Add(value2);
        valueList.Add(value3);

        // Assert
        valueList.allStorage.Should().HaveCount(3);
        valueList.allStorage.Should().Contain(value1);
        valueList.allStorage.Should().Contain(value2);
        valueList.allStorage.Should().Contain(value3);
    }

    [Fact]
    public void GetEnumerator_ShouldThrowNotImplementedException()
    {
        // Arrange
        var valueList = new ValueList<string>();

        // Act & Assert
        var action = () => valueList.GetEnumerator();
        action.Should().Throw<NotImplementedException>();
    }

    [Fact]
    public void IEnumerableGetEnumerator_ShouldThrowNotImplementedException()
    {
        // Arrange
        var valueList = new ValueList<string>();

        // Act & Assert
        var action = () => ((System.Collections.IEnumerable)valueList).GetEnumerator();
        action.Should().Throw<NotImplementedException>();
    }

    [Fact]
    public void Map_WithEmptyLazyStorage_ShouldNotModifyStorage()
    {
        // Arrange
        var valueList = new ValueList<string>();

        // Act
        var count = valueList.Count;

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public void Map_ShouldProcessLazyStorageInReverseOrder()
    {
        // Arrange
        var valueList = new ValueList<string>();
        var value1 = new Value<string>("first");
        var value2 = new Value<string>("second");
        var value3 = new Value<string>("third");

        // Act
        valueList.Add(value1);
        valueList.Add(value2);
        valueList.Add(value3);
        var count = valueList.Count; // 触发 map()

        // Assert
        count.Should().Be(3);
        valueList.contains("first").Should().BeTrue();
        valueList.contains("second").Should().BeTrue();
        valueList.contains("third").Should().BeTrue();
    }
}
