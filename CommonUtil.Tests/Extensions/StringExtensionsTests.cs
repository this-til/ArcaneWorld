using System;
using System.Collections.Generic;
using System.Text;
using CommonUtil.Extensions;
using FluentAssertions;
using Xunit;

namespace CommonUtil.Tests.Extensions;

public class StringExtensionsTests
{
    [Fact]
    public void Format_WithSimplePlaceholder_ShouldCallStructure()
    {
        // Arrange
        const string format = "Hello {name}!";
        var stringBuilder = new StringBuilder();
        var capturedKeys = new List<string>();

        // Act
        format.Format(stringBuilder, key => capturedKeys.Add(key));

        // Assert
        capturedKeys.Should().Equal("name");
    }

    [Fact]
    public void Format_WithMultiplePlaceholders_ShouldCallStructureForEach()
    {
        // Arrange
        const string format = "Hello {name}, you are {age} years old!";
        var stringBuilder = new StringBuilder();
        var capturedKeys = new List<string>();

        // Act
        format.Format(stringBuilder, key => capturedKeys.Add(key));

        // Assert
        capturedKeys.Should().Equal("name", "age");
    }

    [Fact]
    public void Format_WithNoPlaceholders_ShouldNotCallStructure()
    {
        // Arrange
        const string format = "Hello World!";
        var stringBuilder = new StringBuilder();
        var capturedKeys = new List<string>();

        // Act
        format.Format(stringBuilder, key => capturedKeys.Add(key));

        // Assert
        capturedKeys.Should().BeEmpty();
    }

    [Fact]
    public void Format_WithEmptyString_ShouldNotCallStructure()
    {
        // Arrange
        const string format = "";
        var stringBuilder = new StringBuilder();
        var capturedKeys = new List<string>();

        // Act
        format.Format(stringBuilder, key => capturedKeys.Add(key));

        // Assert
        capturedKeys.Should().BeEmpty();
    }

    [Fact]
    public void Format_WithEscapedBraces_ShouldNotCallStructure()
    {
        // Arrange
        const string format = "Hello {{name}}!";
        var stringBuilder = new StringBuilder();
        var capturedKeys = new List<string>();

        // Act
        format.Format(stringBuilder, key => capturedKeys.Add(key));

        // Assert
        capturedKeys.Should().BeEmpty();
    }

    [Fact]
    public void Format_WithMixedContent_ShouldCallStructureOnlyForValidPlaceholders()
    {
        // Arrange
        const string format = "Hello {name}, {{escaped}}, {age} years old!";
        var stringBuilder = new StringBuilder();
        var capturedKeys = new List<string>();

        // Act
        format.Format(stringBuilder, key => capturedKeys.Add(key));

        // Assert
        capturedKeys.Should().Equal("name", "age");
    }

    [Fact]
    public void Format_WithNullFormat_ShouldThrowArgumentNullException()
    {
        // Arrange
        string? format = null;
        var stringBuilder = new StringBuilder();

        // Act & Assert
        var action = () => format!.Format(stringBuilder, key => { });
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("format");
    }

    [Fact]
    public void Format_WithNullStructure_ShouldThrowArgumentNullException()
    {
        // Arrange
        const string format = "Hello {name}!";
        var stringBuilder = new StringBuilder();

        // Act & Assert
        var action = () => format.Format(stringBuilder, null!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("args");
    }

    [Fact]
    public void Format_WithNullStringBuilder_ShouldNotThrow()
    {
        // Arrange
        const string format = "Hello {name}!";
        var capturedKeys = new List<string>();

        // Act & Assert
        var action = () => format.Format(null, key => capturedKeys.Add(key));
        action.Should().NotThrow();
        capturedKeys.Should().Equal("name");
    }

    [Fact]
    public void Format_WithEmptyPlaceholder_ShouldCallStructureWithEmptyKey()
    {
        // Arrange
        const string format = "Hello {}!";
        var stringBuilder = new StringBuilder();
        var capturedKeys = new List<string>();

        // Act
        format.Format(stringBuilder, key => capturedKeys.Add(key));

        // Assert
        capturedKeys.Should().Equal("");
    }
    
    [Fact]
    public void Format_WithUnclosedBrace_ShouldNotCallStructure()
    {
        // Arrange
        const string format = "Hello {name";
        var stringBuilder = new StringBuilder();
        var capturedKeys = new List<string>();

        // Act
        format.Format(stringBuilder, key => capturedKeys.Add(key));

        // Assert
        capturedKeys.Should().BeEmpty();
    }

    [Fact]
    public void Format_WithOnlyOpeningBrace_ShouldNotCallStructure()
    {
        // Arrange
        const string format = "{";
        var stringBuilder = new StringBuilder();
        var capturedKeys = new List<string>();

        // Act
        format.Format(stringBuilder, key => capturedKeys.Add(key));

        // Assert
        capturedKeys.Should().BeEmpty();
    }
    
    [Fact]
    public void Format_WithTripleBraces_ShouldEscapeCorrectly()
    {
        // Arrange
        const string format = "Hello {{{name}}}!";
        var stringBuilder = new StringBuilder();
        var capturedKeys = new List<string>();

        // Act
        format.Format(stringBuilder, key => capturedKeys.Add(key));

        // Assert
        capturedKeys.Should().Equal("name");
    }

    [Fact]
    public void IsNullOrEmpty_WithNullString_ShouldReturnTrue()
    {
        // Arrange
        string? str = null;

        // Act
        var result = str.IsNullOrEmpty();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsNullOrEmpty_WithEmptyString_ShouldReturnTrue()
    {
        // Arrange
        const string str = "";

        // Act
        var result = str.IsNullOrEmpty();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsNullOrEmpty_WithWhitespaceString_ShouldReturnFalse()
    {
        // Arrange
        const string str = "   ";

        // Act
        var result = str.IsNullOrEmpty();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsNullOrEmpty_WithValidString_ShouldReturnFalse()
    {
        // Arrange
        const string str = "Hello World";

        // Act
        var result = str.IsNullOrEmpty();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsNullOrWhiteSpace_WithNullString_ShouldReturnTrue()
    {
        // Arrange
        string? str = null;

        // Act
        var result = str.IsNullOrWhiteSpace();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsNullOrWhiteSpace_WithEmptyString_ShouldReturnTrue()
    {
        // Arrange
        const string str = "";

        // Act
        var result = str.IsNullOrWhiteSpace();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsNullOrWhiteSpace_WithWhitespaceString_ShouldReturnTrue()
    {
        // Arrange
        const string str = "   ";

        // Act
        var result = str.IsNullOrWhiteSpace();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsNullOrWhiteSpace_WithTabString_ShouldReturnTrue()
    {
        // Arrange
        const string str = "\t";

        // Act
        var result = str.IsNullOrWhiteSpace();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsNullOrWhiteSpace_WithNewlineString_ShouldReturnTrue()
    {
        // Arrange
        const string str = "\n";

        // Act
        var result = str.IsNullOrWhiteSpace();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsNullOrWhiteSpace_WithValidString_ShouldReturnFalse()
    {
        // Arrange
        const string str = "Hello World";

        // Act
        var result = str.IsNullOrWhiteSpace();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsNullOrWhiteSpace_WithStringContainingWhitespace_ShouldReturnFalse()
    {
        // Arrange
        const string str = "Hello World ";

        // Act
        var result = str.IsNullOrWhiteSpace();

        // Assert
        result.Should().BeFalse();
    }
}
