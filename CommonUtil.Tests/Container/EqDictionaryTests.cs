using System;
using System.Collections.Generic;
using CommonUtil.Container;
using FluentAssertions;
using Xunit;

namespace CommonUtil.Tests.Container;

public class EqDictionaryTests
{
    [Fact]
    public void Constructor_Default_ShouldCreateEmptyDictionary()
    {
        // Arrange & Act
        var dict = new EqDictionary<string, int>();

        // Assert
        dict.Should().BeEmpty();
        dict.Count.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithCapacity_ShouldCreateEmptyDictionary()
    {
        // Arrange & Act
        var dict = new EqDictionary<string, int>(10);

        // Assert
        dict.Should().BeEmpty();
        dict.Count.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithComparer_ShouldCreateEmptyDictionary()
    {
        // Arrange & Act
        var dict = new EqDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // Assert
        dict.Should().BeEmpty();
        dict.Count.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithDictionary_ShouldCopyValues()
    {
        // Arrange
        var sourceDict = new Dictionary<string, int>
        {
            { "key1", 1 },
            { "key2", 2 },
            { "key3", 3 }
        };

        // Act
        var dict = new EqDictionary<string, int>(sourceDict);

        // Assert
        dict.Should().HaveCount(3);
        dict["key1"].Should().Be(1);
        dict["key2"].Should().Be(2);
        dict["key3"].Should().Be(3);
    }

    [Fact]
    public void Constructor_WithKeyValuePairs_ShouldCreateDictionary()
    {
        // Arrange
        var pairs = new[]
        {
            new KeyValuePair<string, int>("key1", 1),
            new KeyValuePair<string, int>("key2", 2)
        };

        // Act
        var dict = new EqDictionary<string, int>(pairs);

        // Assert
        dict.Should().HaveCount(2);
        dict["key1"].Should().Be(1);
        dict["key2"].Should().Be(2);
    }

    [Fact]
    public void Equals_WithSameInstance_ShouldReturnTrue()
    {
        // Arrange
        var dict = new EqDictionary<string, int>
        {
            { "key1", 1 },
            { "key2", 2 }
        };

        // Act
        var result = dict.Equals(dict);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithSameContent_ShouldReturnTrue()
    {
        // Arrange
        var dict1 = new EqDictionary<string, int>
        {
            { "key1", 1 },
            { "key2", 2 }
        };

        var dict2 = new Dictionary<string, int>
        {
            { "key1", 1 },
            { "key2", 2 }
        };

        // Act
        var result = dict1.Equals(dict2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentContent_ShouldReturnFalse()
    {
        // Arrange
        var dict1 = new EqDictionary<string, int>
        {
            { "key1", 1 },
            { "key2", 2 }
        };

        var dict2 = new Dictionary<string, int>
        {
            { "key1", 1 },
            { "key2", 3 } // 不同的值
        };

        // Act
        var result = dict1.Equals(dict2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentKeys_ShouldReturnFalse()
    {
        // Arrange
        var dict1 = new EqDictionary<string, int>
        {
            { "key1", 1 },
            { "key2", 2 }
        };

        var dict2 = new Dictionary<string, int>
        {
            { "key1", 1 },
            { "key3", 2 } // 不同的键
        };

        // Act
        var result = dict1.Equals(dict2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentCount_ShouldReturnFalse()
    {
        // Arrange
        var dict1 = new EqDictionary<string, int>
        {
            { "key1", 1 },
            { "key2", 2 }
        };

        var dict2 = new Dictionary<string, int>
        {
            { "key1", 1 }
        };

        // Act
        var result = dict1.Equals(dict2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var dict = new EqDictionary<string, int>
        {
            { "key1", 1 }
        };

        // Act
        var result = dict.Equals(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNonDictionary_ShouldReturnFalse()
    {
        // Arrange
        var dict = new EqDictionary<string, int>
        {
            { "key1", 1 }
        };

        // Act
        var result = dict.Equals("not a dictionary");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameContent_ShouldReturnSameHashCode()
    {
        // Arrange
        var dict1 = new EqDictionary<string, int>
        {
            { "key1", 1 },
            { "key2", 2 }
        };

        var dict2 = new EqDictionary<string, int>
        {
            { "key1", 1 },
            { "key2", 2 }
        };

        // Act
        var hashCode1 = dict1.GetHashCode();
        var hashCode2 = dict2.GetHashCode();

        // Assert
        hashCode1.Should().Be(hashCode2);
    }

    [Fact]
    public void GetHashCode_WithDifferentContent_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var dict1 = new EqDictionary<string, int>
        {
            { "key1", 1 },
            { "key2", 2 }
        };

        var dict2 = new EqDictionary<string, int>
        {
            { "key1", 1 },
            { "key2", 3 }
        };

        // Act
        var hashCode1 = dict1.GetHashCode();
        var hashCode2 = dict2.GetHashCode();

        // Assert
        hashCode1.Should().NotBe(hashCode2);
    }

    [Fact]
    public void GetHashCode_WithEmptyDictionary_ShouldReturnConsistentHashCode()
    {
        // Arrange
        var dict1 = new EqDictionary<string, int>();
        var dict2 = new EqDictionary<string, int>();

        // Act
        var hashCode1 = dict1.GetHashCode();
        var hashCode2 = dict2.GetHashCode();

        // Assert
        hashCode1.Should().Be(hashCode2);
    }

    [Fact]
    public void GetHashCode_WithNullValues_ShouldHandleNulls()
    {
        // Arrange
        var dict = new EqDictionary<string, string?>
        {
            { "key1", "value1" },
            { "key2", null },
            { "key3", "value3" }
        };

        // Act
        var action = () => dict.GetHashCode();

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Equals_WithNullValues_ShouldCompareCorrectly()
    {
        // Arrange
        var dict1 = new EqDictionary<string, string?>
        {
            { "key1", "value1" },
            { "key2", null }
        };

        var dict2 = new Dictionary<string, string?>
        {
            { "key1", "value1" },
            { "key2", null }
        };

        // Act
        var result = dict1.Equals(dict2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentNullValues_ShouldReturnFalse()
    {
        // Arrange
        var dict1 = new EqDictionary<string, string?>
        {
            { "key1", "value1" },
            { "key2", null }
        };

        var dict2 = new Dictionary<string, string?>
        {
            { "key1", "value1" },
            { "key2", "not null" }
        };

        // Act
        var result = dict1.Equals(dict2);

        // Assert
        result.Should().BeFalse();
    }
}
