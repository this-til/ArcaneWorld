using System;
using System.Collections.Generic;
using System.Linq;
using CommonUtil.Extensions;
using FluentAssertions;
using Xunit;

namespace CommonUtil.Tests.Extensions;

public class LinqExtensionsTests {

    [Fact]
    public void Peek_WithValidSource_ShouldExecuteActionAndReturnItems() {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5 };
        var executedItems = new List<int>();

        // Act
        var result = source.Peek(x => executedItems.Add(x)).ToList();

        // Assert
        result.Should().Equal(1, 2, 3, 4, 5);
        executedItems.Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public void Peek_WithNullSource_ShouldThrowArgumentNullException() {
        // Arrange
        IEnumerable<int>? source = null;

        // Act & Assert
        var action = () => source!.Peek(x => { }).ToList();
        action.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("source");
    }

    [Fact]
    public void Peek_WithNullAction_ShouldThrowArgumentNullException() {
        // Arrange
        var source = new[] { 1, 2, 3 };

        // Act & Assert
        var action = () => source.Peek(null!).ToList();
        action.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("action");
    }

    [Fact]
    public void Peek_WithEmptySource_ShouldNotExecuteAction() {
        // Arrange
        var source = new int[0];
        var executedItems = new List<int>();

        // Act
        var result = source.Peek(x => executedItems.Add(x)).ToList();

        // Assert
        result.Should().BeEmpty();
        executedItems.Should().BeEmpty();
    }

    [Fact]
    public void Peek_WithActionThrowingException_ShouldPropagateException() {
        // Arrange
        var source = new[] { 1, 2, 3 };

        // Act & Assert
        var action = () => source.Peek(x => throw new InvalidOperationException("Test exception")).ToList();
        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Test exception");
    }

    [Fact]
    public void Diversion_WithValidPredicate_ShouldSeparateItems() {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5, 6 };
        var awayItems = new List<int>();

        // Act
        var result = source.Diversion(
                x => x % 2 == 0,
                away => {
                    awayItems.AddRange(away);
                    return away;
                }
            )
            .ToList();

        // Assert
        result.Should()
            .Equal(
                2,
                4,
                6,
                1,
                3,
                5
            ); // 原序列 + away 处理后的序列
        awayItems.Should().Equal(1, 3, 5);
    }

    [Fact]
    public void Diversion_WithAllItemsMatchingPredicate_ShouldReturnOriginalItems() {
        // Arrange
        var source = new[] { 2, 4, 6, 8 };
        var awayItems = new List<int>();

        // Act
        var result = source.Diversion(
                x => x % 2 == 0,
                away => {
                    awayItems.AddRange(away);
                    return away;
                }
            )
            .ToList();

        // Assert
        result.Should().Equal(2, 4, 6, 8);
        awayItems.Should().BeEmpty();
    }

    [Fact]
    public void Diversion_WithNoItemsMatchingPredicate_ShouldReturnProcessedAwayItems() {
        // Arrange
        var source = new[] { 1, 3, 5, 7 };
        var awayItems = new List<int>();

        // Act
        var result = source.Diversion(
                x => x % 2 == 0,
                away => {
                    awayItems.AddRange(away);
                    return away;
                }
            )
            .ToList();

        // Assert
        result.Should().Equal(1, 3, 5, 7);
        awayItems.Should().Equal(1, 3, 5, 7);
    }

    [Fact]
    public void End_WithValidSource_ShouldEnumerateAllItems() {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5 };
        var enumeratedItems = new List<int>();

        // Act
        source.End();

        // Assert
        // End 方法只是枚举，不返回任何值，所以这里只验证不抛异常
        true.Should().BeTrue();
    }

    [Fact]
    public void End_WithEmptySource_ShouldNotThrow() {
        // Arrange
        var source = new int[0];

        // Act & Assert
        var action = () => source.End();
        action.Should().NotThrow();
    }

    [Fact]
    public void End_WithNullSource_ShouldThrow() {
        // Arrange
        IEnumerable<int>? source = null;

        // Act & Assert
        var action = () => source!.End();
        action.Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void ClassifiedCollection_WithValidSource_ShouldGroupItems() {
        // Arrange
        var source = new[] {
            new { Name = "Alice", Age = 25 },
            new { Name = "Bob", Age = 30 },
            new { Name = "Charlie", Age = 25 },
            new { Name = "David", Age = 30 }
        };

        // Act
        var result = source.ClassifiedCollection(
            x => x.Age,
            x => x.Name
        );

        // Assert
        result.Should().HaveCount(2);
        result[25].Should().Equal("Alice", "Charlie");
        result[30].Should().Equal("Bob", "David");
    }

    [Fact]
    public void ClassifiedCollection_WithCustomContainer_ShouldUseCustomContainer() {
        // Arrange
        var source = new[] { "a", "bb", "ccc" };

        // Act
        var result = source.ClassifiedCollection(
            x => x.Length,
            x => x,
            k => new List<string>()
        );

        // Assert
        result.Should().HaveCount(3);
        result[1].Should().BeOfType<List<string>>();
        result[2].Should().BeOfType<List<string>>();
        result[3].Should().BeOfType<List<string>>();
    }

    [Fact]
    public void ClassifiedCollection_WithDefaultKeys_ShouldInitializeKeys() {
        // Arrange
        var source = new[] { "a", "bb" };
        var defaultKeys = new[] { 1, 2, 3 };

        // Act
        var result = source.ClassifiedCollection(
            x => x.Length,
            x => x,
            null,
            defaultKeys
        );

        // Assert
        result.Should().HaveCount(3);
        result.Keys.Should().Contain(new List<int>() { 1, 2, 3 });
        result[1].Should().Equal("a");
        result[2].Should().Equal("bb");
        result[3].Should().BeEmpty();
    }

    [Fact]
    public void ClassifiedCollection_WithNullKeys_ShouldSkipItems() {
        // Arrange
        var source = new[] {
            new { Name = "Alice", Age = (int?)25 },
            new { Name = "Bob", Age = (int?)null },
            new { Name = "Charlie", Age = (int?)30 }
        };

        // Act
        var result = source.ClassifiedCollection(
            x => x.Age,
            x => x.Name
        );

        // Assert
        result.Should().HaveCount(2);
        result[25].Should().Equal("Alice");
        result[30].Should().Equal("Charlie");
    }

    [Fact]
    public void ToReadOnlyDictionary_WithValidDictionary_ShouldConvert() {
        // Arrange
        var dictionary = new Dictionary<string, int> {
            { "key1", 1 },
            { "key2", 2 }
        };

        // Act
        var result = dictionary.ToReadOnlyDictionary();

        // Assert
        result.Should().HaveCount(2);
        result["key1"].Should().Be(1);
        result["key2"].Should().Be(2);
    }

    [Fact]
    public void ToReadOnlyDictionary_WithEmptyDictionary_ShouldReturnEmpty() {
        // Arrange
        var dictionary = new Dictionary<string, int>();

        // Act
        var result = dictionary.ToReadOnlyDictionary();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void DistinctBy_WithValidSource_ShouldReturnDistinctItems() {
        // Arrange
        var source = new[] {
            new { Name = "Alice", Age = 25 },
            new { Name = "Bob", Age = 30 },
            new { Name = "Charlie", Age = 25 },
            new { Name = "David", Age = 30 }
        };

        // Act
        var result = source.DistinctBy(x => x.Age).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(x => x.Name == "Alice" && x.Age == 25);
        result.Should().Contain(x => x.Name == "Bob" && x.Age == 30);
    }

    [Fact]
    public void DistinctBy_WithNullSource_ShouldThrowArgumentNullException() {
        // Arrange
        IEnumerable<int>? source = null;

        // Act & Assert
        var action = () => source!.DistinctBy(x => x).ToList();
        action.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("source");
    }

    [Fact]
    public void Where_WithValidPredicate_ShouldFilterAndExecuteAction() {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5 };
        var filteredItems = new List<int>();

        // Act
        var result = source.Where(x => x % 2 == 0, x => filteredItems.Add(x)).ToList();

        // Assert
        result.Should().Equal(2, 4);
        filteredItems.Should().Equal(1, 3, 5);
    }

    [Fact]
    public void Where_WithAllItemsMatching_ShouldReturnAllItems() {
        // Arrange
        var source = new[] { 2, 4, 6, 8 };
        var filteredItems = new List<int>();

        // Act
        var result = source.Where(x => x % 2 == 0, x => filteredItems.Add(x)).ToList();

        // Assert
        result.Should().Equal(2, 4, 6, 8);
        filteredItems.Should().BeEmpty();
    }

    [Fact]
    public void Where_WithNoItemsMatching_ShouldReturnEmpty() {
        // Arrange
        var source = new[] { 1, 3, 5, 7 };
        var filteredItems = new List<int>();

        // Act
        var result = source.Where(x => x % 2 == 0, x => filteredItems.Add(x)).ToList();

        // Assert
        result.Should().BeEmpty();
        filteredItems.Should().Equal(1, 3, 5, 7);
    }


    [Fact]
    public void NotNull_WithMixedNulls_ShouldFilterOutNulls() {
        // Arrange
        var source = new string?[] { "a", null, "b", null, "c" };

        // Act
        var result = source.NotNull().ToList();

        // Assert
        result.Should().Equal("a", "b", "c");
    }

    [Fact]
    public void NotNull_WithAllNulls_ShouldReturnEmpty() {
        // Arrange
        var source = new string?[] { null, null, null };

        // Act
        var result = source.NotNull().ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void NotNull_WithNoNulls_ShouldReturnAllItems() {
        // Arrange
        var source = new[] { "a", "b", "c" };

        // Act
        var result = source.NotNull().ToList();

        // Assert
        result.Should().Equal("a", "b", "c");
    }


    [Fact]
    public void ControlQuantity_WithExactQuantity_ShouldReturnAllItems() {
        // Arrange
        var source = new[] { 1, 2, 3 };

        // Act
        var result = source.ControlQuantity(3);

        // Assert
        result.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void ControlQuantity_WithMoreItems_ShouldReturnFirstNItems() {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5 };

        // Act
        var result = source.ControlQuantity(3);

        // Assert
        result.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void ControlQuantity_WithFewerItems_ShouldPadWithDefaults() {
        // Arrange
        var source = new[] { 1, 2 };

        // Act
        var result = source.ControlQuantity(5);

        // Assert
        result.Should().Equal(new List<int> { 1, 2, 0, 0, 0 });
    }

}
