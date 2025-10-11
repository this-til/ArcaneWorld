using System;
using System.Collections.Generic;
using CommonUtil.Container;
using FluentAssertions;
using Xunit;

namespace CommonUtil.Tests.Container;

public class EqListTests
{
    [Fact]
    public void Constructor_Default_ShouldCreateEmptyList()
    {
        // Arrange & Act
        var list = new EqList<string>();

        // Assert
        list.Should().BeEmpty();
        list.Count.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithCapacity_ShouldCreateEmptyList()
    {
        // Arrange & Act
        var list = new EqList<string>(10);

        // Assert
        list.Should().BeEmpty();
        list.Count.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithCollection_ShouldCopyValues()
    {
        // Arrange
        var sourceList = new List<string> { "item1", "item2", "item3" };

        // Act
        var list = new EqList<string>(sourceList);

        // Assert
        list.Should().HaveCount(3);
        list[0].Should().Be("item1");
        list[1].Should().Be("item2");
        list[2].Should().Be("item3");
    }

    [Fact]
    public void Equals_WithSameInstance_ShouldReturnTrue()
    {
        // Arrange
        var list = new EqList<string> { "item1", "item2" };

        // Act
        var result = list.Equals(list);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithSameContent_ShouldReturnTrue()
    {
        // Arrange
        var list1 = new EqList<string> { "item1", "item2" };
        var list2 = new List<string> { "item1", "item2" };

        // Act
        var result = list1.Equals(list2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentContent_ShouldReturnFalse()
    {
        // Arrange
        var list1 = new EqList<string> { "item1", "item2" };
        var list2 = new List<string> { "item1", "item3" }; // 不同的值

        // Act
        var result = list1.Equals(list2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentOrder_ShouldReturnFalse()
    {
        // Arrange
        var list1 = new EqList<string> { "item1", "item2" };
        var list2 = new List<string> { "item2", "item1" }; // 不同的顺序

        // Act
        var result = list1.Equals(list2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentCount_ShouldReturnFalse()
    {
        // Arrange
        var list1 = new EqList<string> { "item1", "item2" };
        var list2 = new List<string> { "item1" };

        // Act
        var result = list1.Equals(list2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var list = new EqList<string> { "item1" };

        // Act
        var result = list.Equals(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNonList_ShouldReturnFalse()
    {
        // Arrange
        var list = new EqList<string> { "item1" };

        // Act
        var result = list.Equals("not a list");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameContent_ShouldReturnSameHashCode()
    {
        // Arrange
        var list1 = new EqList<string> { "item1", "item2" };
        var list2 = new EqList<string> { "item1", "item2" };

        // Act
        var hashCode1 = list1.GetHashCode();
        var hashCode2 = list2.GetHashCode();

        // Assert
        hashCode1.Should().Be(hashCode2);
    }

    [Fact]
    public void GetHashCode_WithDifferentContent_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var list1 = new EqList<string> { "item1", "item2" };
        var list2 = new EqList<string> { "item1", "item3" };

        // Act
        var hashCode1 = list1.GetHashCode();
        var hashCode2 = list2.GetHashCode();

        // Assert
        hashCode1.Should().NotBe(hashCode2);
    }

    [Fact]
    public void GetHashCode_WithEmptyList_ShouldReturnConsistentHashCode()
    {
        // Arrange
        var list1 = new EqList<string>();
        var list2 = new EqList<string>();

        // Act
        var hashCode1 = list1.GetHashCode();
        var hashCode2 = list2.GetHashCode();

        // Assert
        hashCode1.Should().Be(hashCode2);
    }

    [Fact]
    public void GetHashCode_WithNullValues_ShouldHandleNulls()
    {
        // Arrange
        var list = new EqList<string?> { "item1", null, "item3" };

        // Act
        var action = () => list.GetHashCode();

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void ToString_WithValues_ShouldJoinWithCommas()
    {
        // Arrange
        var list = new EqList<string> { "item1", "item2", "item3" };

        // Act
        var result = list.ToString();

        // Assert
        result.Should().Be("item1,item2,item3");
    }

    [Fact]
    public void ToString_WithNullValues_ShouldShowNull()
    {
        // Arrange
        var list = new EqList<string?> { "item1", null, "item3" };

        // Act
        var result = list.ToString();

        // Assert
        result.Should().Be("item1,null,item3");
    }

    [Fact]
    public void ToString_WithEmptyList_ShouldReturnEmptyString()
    {
        // Arrange
        var list = new EqList<string>();

        // Act
        var result = list.ToString();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Equals_WithNullValues_ShouldCompareCorrectly()
    {
        // Arrange
        var list1 = new EqList<string?> { "item1", null, "item3" };
        var list2 = new List<string?> { "item1", null, "item3" };

        // Act
        var result = list1.Equals(list2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentNullValues_ShouldReturnFalse()
    {
        // Arrange
        var list1 = new EqList<string?> { "item1", null, "item3" };
        var list2 = new List<string?> { "item1", "not null", "item3" };

        // Act
        var result = list1.Equals(list2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithSameNullValues_ShouldReturnTrue()
    {
        // Arrange
        var list1 = new EqList<string?> { null, null };
        var list2 = new List<string?> { null, null };

        // Act
        var result = list1.Equals(list2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameNullValues_ShouldReturnSameHashCode()
    {
        // Arrange
        var list1 = new EqList<string?> { null, null };
        var list2 = new EqList<string?> { null, null };

        // Act
        var hashCode1 = list1.GetHashCode();
        var hashCode2 = list2.GetHashCode();

        // Assert
        hashCode1.Should().Be(hashCode2);
    }

    [Fact]
    public void ToString_WithAllNullValues_ShouldShowAllNulls()
    {
        // Arrange
        var list = new EqList<string?> { null, null, null };

        // Act
        var result = list.ToString();

        // Assert
        result.Should().Be("null,null,null");
    }
}
