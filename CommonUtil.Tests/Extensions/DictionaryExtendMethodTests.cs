using System.Collections.Generic;
using CommonUtil.Extensions;
using FluentAssertions;
using Xunit;

namespace CommonUtil.Tests.Extensions;

public class DictionaryExtendMethodTests
{
    [Fact]
    public void IsEmpty_WithEmptyDictionary_ShouldReturnTrue()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>();

        // Act
        var result = dictionary.IsEmpty();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WithNonEmptyDictionary_ShouldReturnFalse()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>
        {
            { "key1", 1 },
            { "key2", 2 }
        };

        // Act
        var result = dictionary.IsEmpty();

        // Assert
        result.Should().BeFalse();
    }
    
}
