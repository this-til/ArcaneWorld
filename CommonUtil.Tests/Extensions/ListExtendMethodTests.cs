using System;
using System.Collections.Generic;
using System.Linq;
using CommonUtil.Extensions;
using FluentAssertions;
using Xunit;

namespace CommonUtil.Tests.Extensions;

public class ListExtendMethodTests
{
    [Fact]
    public void IsEmpty_WithNullList_ShouldReturnTrue()
    {
        // Arrange
        IList<string>? list = null;

        // Act
        var result = list.IsEmpty();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WithEmptyList_ShouldReturnTrue()
    {
        // Arrange
        var list = new List<string>();

        // Act
        var result = list.IsEmpty();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WithNonEmptyList_ShouldReturnFalse()
    {
        // Arrange
        var list = new List<string> { "item1", "item2" };

        // Act
        var result = list.IsEmpty();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsEmpty_WithSingleItemList_ShouldReturnFalse()
    {
        // Arrange
        var list = new List<string> { "item" };

        // Act
        var result = list.IsEmpty();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void InList_WithNullList_ShouldReturnFalse()
    {
        // Arrange
        IList<string>? list = null;

        // Act
        var result = list.InList(0);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void InList_WithValidIndex_ShouldReturnTrue()
    {
        // Arrange
        var list = new List<string> { "item1", "item2", "item3" };

        // Act
        var result = list.InList(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void InList_WithFirstIndex_ShouldReturnTrue()
    {
        // Arrange
        var list = new List<string> { "item1", "item2", "item3" };

        // Act
        var result = list.InList(0);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void InList_WithLastIndex_ShouldReturnTrue()
    {
        // Arrange
        var list = new List<string> { "item1", "item2", "item3" };

        // Act
        var result = list.InList(2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void InList_WithNegativeIndex_ShouldReturnFalse()
    {
        // Arrange
        var list = new List<string> { "item1", "item2", "item3" };

        // Act
        var result = list.InList(-1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void InList_WithIndexEqualToCount_ShouldReturnFalse()
    {
        // Arrange
        var list = new List<string> { "item1", "item2", "item3" };

        // Act
        var result = list.InList(3);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void InList_WithIndexGreaterThanCount_ShouldReturnFalse()
    {
        // Arrange
        var list = new List<string> { "item1", "item2", "item3" };

        // Act
        var result = list.InList(5);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void InList_WithEmptyList_ShouldReturnFalse()
    {
        // Arrange
        var list = new List<string>();

        // Act
        var result = list.InList(0);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RandomElement_WithEmptyList_ShouldReturnDefault()
    {
        // Arrange
        var list = new List<string>();

        // Act
        var result = list.RandomElement();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void RandomElement_WithSingleItem_ShouldReturnThatItem()
    {
        // Arrange
        var list = new List<string> { "single" };

        // Act
        var result = list.RandomElement();

        // Assert
        result.Should().Be("single");
    }

    [Fact]
    public void RandomElement_WithMultipleItems_ShouldReturnOneOfThem()
    {
        // Arrange
        var list = new List<string> { "item1", "item2", "item3" };

        // Act
        var result = list.RandomElement();

        // Assert
        list.Should().Contain(result);
    }

    [Fact]
    public void RandomElement_WithCustomRandom_ShouldUseCustomRandom()
    {
        // Arrange
        var list = new List<string> { "item1", "item2", "item3" };
        var customRandom = new Random(12345); // 固定种子以确保可预测性

        // Act
        var result = list.RandomElement(customRandom);

        // Assert
        list.Should().Contain(result);
    }

    [Fact]
    public void RandomElement_WithNullList_ShouldReturnDefault()
    {
        // Arrange
        IList<string>? list = null;

        // Act
        var result = list!.RandomElement();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void RandomElement_WithArray_ShouldWork()
    {
        // Arrange
        var array = new[] { "item1", "item2", "item3" };

        // Act
        var result = array.RandomElement();

        // Assert
        array.Should().Contain(result);
    }

    [Fact]
    public void RandomElement_WithCustomList_ShouldWork()
    {
        // Arrange
        var customList = new List<string> { "item1", "item2", "item3" };

        // Act
        var result = ((IList<string>)customList).RandomElement();

        // Assert
        customList.Should().Contain(result);
    }
}

public class IReadOnlyExtendMethodTests
{
    [Fact]
    public void RandomElement_WithEmptyList_ShouldReturnDefault()
    {
        // Arrange
        var list = new List<string>().AsReadOnly();

        // Act
        var result = list.RandomElement();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void RandomElement_WithSingleItem_ShouldReturnThatItem()
    {
        // Arrange
        var list = new List<string> { "single" }.AsReadOnly();

        // Act
        var result = list.RandomElement();

        // Assert
        result.Should().Be("single");
    }

    [Fact]
    public void RandomElement_WithMultipleItems_ShouldReturnOneOfThem()
    {
        // Arrange
        var list = new List<string> { "item1", "item2", "item3" }.AsReadOnly();

        // Act
        var result = list.RandomElement();

        // Assert
        list.Should().Contain(result);
    }
    
}
