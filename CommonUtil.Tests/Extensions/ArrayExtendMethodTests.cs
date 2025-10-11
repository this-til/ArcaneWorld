using System;
using CommonUtil.Extensions;
using FluentAssertions;
using Xunit;

namespace CommonUtil.Tests.Extensions;

public class ArrayExtendMethodTests
{
    [Fact]
    public void IsEmpty_WithNullArray_ShouldReturnTrue()
    {
        // Arrange
        string[]? array = null;

        // Act
        var result = array.IsEmpty();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WithEmptyArray_ShouldReturnTrue()
    {
        // Arrange
        var array = new string[0];

        // Act
        var result = array.IsEmpty();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WithNonEmptyArray_ShouldReturnFalse()
    {
        // Arrange
        var array = new[] { "item1", "item2" };

        // Act
        var result = array.IsEmpty();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsEmpty_WithSingleItemArray_ShouldReturnFalse()
    {
        // Arrange
        var array = new[] { "item" };

        // Act
        var result = array.IsEmpty();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void InArray_WithNullArray_ShouldReturnFalse()
    {
        // Arrange
        string[]? array = null;

        // Act
        var result = array.InArray(0);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void InArray_WithValidIndex_ShouldReturnTrue()
    {
        // Arrange
        var array = new[] { "item1", "item2", "item3" };

        // Act
        var result = array.InArray(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void InArray_WithFirstIndex_ShouldReturnTrue()
    {
        // Arrange
        var array = new[] { "item1", "item2", "item3" };

        // Act
        var result = array.InArray(0);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void InArray_WithLastIndex_ShouldReturnTrue()
    {
        // Arrange
        var array = new[] { "item1", "item2", "item3" };

        // Act
        var result = array.InArray(2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void InArray_WithNegativeIndex_ShouldReturnFalse()
    {
        // Arrange
        var array = new[] { "item1", "item2", "item3" };

        // Act
        var result = array.InArray(-1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void InArray_WithIndexEqualToLength_ShouldReturnFalse()
    {
        // Arrange
        var array = new[] { "item1", "item2", "item3" };

        // Act
        var result = array.InArray(3);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void InArray_WithIndexGreaterThanLength_ShouldReturnFalse()
    {
        // Arrange
        var array = new[] { "item1", "item2", "item3" };

        // Act
        var result = array.InArray(5);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void InArray_WithEmptyArray_ShouldReturnFalse()
    {
        // Arrange
        var array = new string[0];

        // Act
        var result = array.InArray(0);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void DepthHashCode_WithNullArray_ShouldReturnZero()
    {
        // Arrange
        string[]? array = null;

        // Act
        var result = array.DepthHashCode();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void DepthHashCode_WithEmptyArray_ShouldReturnNonZero()
    {
        // Arrange
        var array = new string[0];

        // Act
        var result = array.DepthHashCode();

        // Assert
        result.Should().NotBe(0);
    }

    [Fact]
    public void DepthHashCode_WithSameContent_ShouldReturnSameHashCode()
    {
        // Arrange
        var array1 = new[] { "item1", "item2", "item3" };
        var array2 = new[] { "item1", "item2", "item3" };

        // Act
        var hashCode1 = array1.DepthHashCode();
        var hashCode2 = array2.DepthHashCode();

        // Assert
        hashCode1.Should().Be(hashCode2);
    }

    [Fact]
    public void DepthHashCode_WithDifferentContent_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var array1 = new[] { "item1", "item2", "item3" };
        var array2 = new[] { "item1", "item2", "item4" };

        // Act
        var hashCode1 = array1.DepthHashCode();
        var hashCode2 = array2.DepthHashCode();

        // Assert
        hashCode1.Should().NotBe(hashCode2);
    }

    [Fact]
    public void DepthHashCode_WithNullValues_ShouldHandleNulls()
    {
        // Arrange
        var array = new string?[] { "item1", null, "item3" };

        // Act
        var action = () => array.DepthHashCode();

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void DepthHashCode_WithAllNullValues_ShouldReturnConsistentHashCode()
    {
        // Arrange
        var array1 = new string?[] { null, null, null };
        var array2 = new string?[] { null, null, null };

        // Act
        var hashCode1 = array1.DepthHashCode();
        var hashCode2 = array2.DepthHashCode();

        // Assert
        hashCode1.Should().Be(hashCode2);
    }

    [Fact]
    public void DepthHashCode_WithDifferentOrder_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var array1 = new[] { "item1", "item2", "item3" };
        var array2 = new[] { "item3", "item2", "item1" };

        // Act
        var hashCode1 = array1.DepthHashCode();
        var hashCode2 = array2.DepthHashCode();

        // Assert
        hashCode1.Should().NotBe(hashCode2);
    }

    [Fact]
    public void DepthHashCode_WithSingleItem_ShouldReturnNonZero()
    {
        // Arrange
        var array = new[] { "single" };

        // Act
        var result = array.DepthHashCode();

        // Assert
        result.Should().NotBe(0);
    }

    [Fact]
    public void DepthHashCode_WithMixedTypes_ShouldWork()
    {
        // Arrange
        var array = new object[] { "string", 42, 3.14, true };

        // Act
        var action = () => array.DepthHashCode();

        // Assert
        action.Should().NotThrow();
    }
}
