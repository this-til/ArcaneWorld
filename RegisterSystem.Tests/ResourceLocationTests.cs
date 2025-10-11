using System;
using FluentAssertions;
using RegisterSystem;
using Xunit;

namespace RegisterSystem.Tests;

public partial class ResourceLocationTests {

    [Fact]
    public void constructor_WithValidDomainAndPath_ShouldCreateResourceLocation() {
        // Arrange
        var domain = "test";
        var path = "item";

        // Act
        var resourceLocation = new ResourceLocation(domain, path);

        // Assert
        resourceLocation.domain.Should().Be(domain);
        resourceLocation.path.Should().Be(path);
        resourceLocation.ToString().Should().Be("test:item");
    }

    [Fact]
    public void constructor_WithLocationString_ShouldParseCorrectly() {
        // Arrange
        var locationString = "test:items/sword";

        // Act
        var resourceLocation = new ResourceLocation(locationString);

        // Assert
        resourceLocation.domain.Should().Be("test");
        resourceLocation.path.Should().Be("items/sword");
        resourceLocation.ToString().Should().Be(locationString);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void constructor_WithNullOrEmptyDomain_ShouldThrowArgumentException(string domain) {
        // Arrange & Act & Assert
        Action act = () => new ResourceLocation(domain, "path");
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*domain cannot be null or empty*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void constructor_WithNullOrEmptyPath_ShouldThrowArgumentException(string path) {
        // Arrange & Act & Assert
        Action act = () => new ResourceLocation("domain", path);
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*path cannot be null or empty*");
    }

    [Theory]
    [InlineData("test:")]
    [InlineData(":path")]
    [InlineData("")]
    [InlineData("no_colon")]
    public void constructor_WithInvalidLocationString_ShouldThrowArgumentException(string locationString) {
        // Arrange & Act & Assert
        Action act = () => new ResourceLocation(locationString);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("domain!")]
    [InlineData("domain@")]
    [InlineData("domain#")]
    [InlineData("domain$")]
    public void constructor_WithInvalidCharacters_ShouldThrowArgumentException(string domain) {
        // Arrange & Act & Assert
        Action act = () => new ResourceLocation(domain, "path");
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*contains invalid character*");
    }

    [Theory]
    [InlineData("test", "valid")]
    [InlineData("test123", "valid")]
    [InlineData("test.domain", "valid")]
    [InlineData("test/path", "valid")]
    [InlineData("test_name", "valid")]
    [InlineData("test-name", "valid")]
    public void constructor_WithValidCharacters_ShouldNotThrow(string domain, string path) {
        // Arrange & Act & Assert
        Action act = () => new ResourceLocation(domain, path);
        act.Should().NotThrow();
    }

    [Fact]
    public void equals_WithSameValues_ShouldReturnTrue() {
        // Arrange
        var location1 = new ResourceLocation("test", "item");
        var location2 = new ResourceLocation("test", "item");

        // Act & Assert
        location1.Equals(location2).Should().BeTrue();
        location1.GetHashCode().Should().Be(location2.GetHashCode());
    }

    [Fact]
    public void equals_WithDifferentValues_ShouldReturnFalse() {
        // Arrange
        var location1 = new ResourceLocation("test", "item1");
        var location2 = new ResourceLocation("test", "item2");

        // Act & Assert
        location1.Equals(location2).Should().BeFalse();
        (location1 == location2).Should().BeFalse();
    }

    [Fact]
    public void equals_WithNull_ShouldReturnFalse() {
        // Arrange
        var location = new ResourceLocation("test", "item");

        // Act & Assert
        location.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void compareTo_ShouldOrderByDomainThenPath() {
        // Arrange
        var locations = new[] {
            new ResourceLocation("z", "a"),
            new ResourceLocation("a", "z"),
            new ResourceLocation("a", "a"),
            new ResourceLocation("b", "a")
        };

        // Act
        Array.Sort(locations);

        // Assert
        locations[0].ToString().Should().Be("a:a");
        locations[1].ToString().Should().Be("a:z");
        locations[2].ToString().Should().Be("b:a");
        locations[3].ToString().Should().Be("z:a");
    }

    [Fact]
    public void compareTo_WithNull_ShouldReturnPositive() {
        // Arrange
        var location = new ResourceLocation("test", "item");

        // Act
        var result = location.CompareTo(null);

        // Assert
        result.Should().BePositive();
    }

    [Fact]
    public void implicitConversion_ToStringShould_ReturnCorrectFormat() {
        // Arrange
        var location = new ResourceLocation("test", "item");

        // Act
        string result = location;

        // Assert
        result.Should().Be("test:item");
    }

    [Fact]
    public void getHashCode_ShouldBeConsistent() {
        // Arrange
        var location = new ResourceLocation("test", "item");

        // Act
        var hash1 = location.GetHashCode();
        var hash2 = location.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void toString_ShouldReturnCachedValue() {
        // Arrange
        var location = new ResourceLocation("test", "item");

        // Act
        var string1 = location.ToString();
        var string2 = location.ToString();

        // Assert
        string1.Should().BeSameAs(string2);
        string1.Should().Be("test:item");
    }

}
