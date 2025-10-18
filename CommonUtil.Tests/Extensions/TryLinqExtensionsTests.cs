using System;
using System.Collections.Generic;
using System.Linq;
using CommonUtil.Extensions;
using FluentAssertions;
using Xunit;

namespace CommonUtil.Tests.Extensions;

public class TryLinqExtensionsTests
{
    [Fact]
    public void TryPeek_WithValidSource_ShouldExecuteActionAndReturnItems()
    {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5 };
        var executedItems = new List<int>();
        var exceptions = new List<(int, Exception)>();

        // Act
        var result = source.TryPeek(
            x => executedItems.Add(x),
            (item, ex) => exceptions.Add((item, ex))
        ).ToList();

        // Assert
        result.Should().Equal(1, 2, 3, 4, 5);
        executedItems.Should().Equal(1, 2, 3, 4, 5);
        exceptions.Should().BeEmpty();
    }

    [Fact]
    public void TryPeek_WithActionThrowingException_ShouldCatchException()
    {
        // Arrange
        var source = new[] { 1, 2, 3 };
        var executedItems = new List<int>();
        var exceptions = new List<(int, Exception)>();

        // Act
        var result = source.TryPeek(
            x => {
                if (x == 2) {
                    throw new InvalidOperationException("Test exception");
                }
                executedItems.Add(x);
            },
            (item, ex) => exceptions.Add((item, ex))
        ).ToList();

        // Assert
        result.Should().Equal(1, 2, 3);
        executedItems.Should().Equal(1, 3);
        exceptions.Should().HaveCount(1);
        exceptions[0].Item1.Should().Be(2);
        exceptions[0].Item2.Message.Should().Be("Test exception");
    }

    [Fact]
    public void TryPeek_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<int>? source = null;

        // Act & Assert
        var action = () => source!.TryPeek(x => { }, (item, ex) => { }).ToList();
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("source");
    }

    [Fact]
    public void TryPeek_WithNullAction_ShouldThrowArgumentNullException()
    {
        // Arrange
        var source = new[] { 1, 2, 3 };

        // Act & Assert
        var action = () => source.TryPeek(null!, (item, ex) => { }).ToList();
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("action");
    }

    [Fact]
    public void TryPeek_WithNullExceptionHandler_ShouldThrowArgumentNullException()
    {
        // Arrange
        var source = new[] { 1, 2, 3 };

        // Act & Assert
        var action = () => source.TryPeek(x => { }, null!).ToList();
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("ex");
    }

    [Fact]
    public void TrySelect_WithValidSource_ShouldTransformItems()
    {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5 };
        var exceptions = new List<(int, Exception)>();

        // Act
        var result = source.TrySelect(
            x => x * 2,
            (item, ex) => {
                exceptions.Add((item, ex));
                return -1;
            }
        ).ToList();

        // Assert
        result.Should().Equal(2, 4, 6, 8, 10);
        exceptions.Should().BeEmpty();
    }

    [Fact]
    public void TrySelect_WithSelectorThrowingException_ShouldCatchException()
    {
        // Arrange
        var source = new[] { 1, 2, 3 };
        var exceptions = new List<(int, Exception)>();

        // Act
        var result = source.TrySelect(
            x => {
                if (x == 2) {
                    throw new InvalidOperationException("Test exception");
                }
                return x * 2;
            },
            (item, ex) => {
                exceptions.Add((item, ex));
                return -1;
            }
        ).ToList();

        // Assert
        result.Should().Equal(2, -1, 6);
        exceptions.Should().HaveCount(1);
        exceptions[0].Item1.Should().Be(2);
        exceptions[0].Item2.Message.Should().Be("Test exception");
    }

    [Fact]
    public void TrySelect_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<int>? source = null;

        // Act & Assert
        var action = () => source!.TrySelect(x => x, (item, ex) => -1).ToList();
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("source");
    }

    [Fact]
    public void TrySelect_WithNullSelector_ShouldThrowArgumentNullException()
    {
        // Arrange
        var source = new[] { 1, 2, 3 };

        // Act & Assert
        var action = () => source.TrySelect(null!, (item, ex) => -1).ToList();
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("selector");
    }

    [Fact]
    public void TrySelect_WithNullExceptionHandler_ShouldThrowArgumentNullException()
    {
        // Arrange
        var source = new[] { 1, 2, 3 };

        // Act & Assert
        var action = () => source.TrySelect(x => x, null!).ToList();
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("ex");
    }

    [Fact]
    public void TrySelectMany_WithValidSource_ShouldFlattenItems()
    {
        // Arrange
        var source = new[] { 1, 2, 3 };
        var exceptions = new List<(int, Exception)>();

        // Act
        var result = source.TrySelectMany(
            x => Enumerable.Range(1, x),
            (item, ex) => {
                exceptions.Add((item, ex));
                return Enumerable.Empty<int>();
            }
        ).ToList();

        // Assert
        result.Should().Equal(1, 1, 2, 1, 2, 3);
        exceptions.Should().BeEmpty();
    }

    [Fact]
    public void TrySelectMany_WithSelectorThrowingException_ShouldCatchException()
    {
        // Arrange
        var source = new[] { 1, 2, 3 };
        var exceptions = new List<(int, Exception)>();

        // Act
        var result = source.TrySelectMany(
            x => {
                if (x == 2) {
                    throw new InvalidOperationException("Test exception");
                }
                return Enumerable.Range(1, x);
            },
            (item, ex) => {
                exceptions.Add((item, ex));
                return new[] { -1 };
            }
        ).ToList();

        // Assert
        result.Should().Equal(1, -1, 1, 2, 3);
        exceptions.Should().HaveCount(1);
        exceptions[0].Item1.Should().Be(2);
        exceptions[0].Item2.Message.Should().Be("Test exception");
    }

    [Fact]
    public void TrySelectMany_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<int>? source = null;

        // Act & Assert
        var action = () => source!.TrySelectMany(x => Enumerable.Empty<int>(), (item, ex) => Enumerable.Empty<int>()).ToList();
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("source");
    }

    [Fact]
    public void TrySelectMany_WithNullSelector_ShouldThrowArgumentNullException()
    {
        // Arrange
        var source = new[] { 1, 2, 3 };

        // Act & Assert
        var action = () => source.TrySelectMany(null!, (item, ex) => Enumerable.Empty<int>()).ToList();
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("selector");
    }
    
    [Fact]
    public void TryWhere_WithValidSource_ShouldFilterItems()
    {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5 };

        // Act
        var result = source.TryWhere(x => x % 2 == 0).ToList();

        // Assert
        result.Should().Equal(2, 4);
    }

    [Fact]
    public void TryWhere_WithPredicateThrowingException_ShouldSkipItem()
    {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5 };

        // Act
        var result = source.TryWhere(x => {
            if (x == 3) {
                throw new InvalidOperationException("Test exception");
            }
            return x % 2 == 0;
        }).ToList();

        // Assert
        result.Should().Equal(2, 4);
    }

    [Fact]
    public void TryWhere_WithPredicateThrowingExceptionAndCallback_ShouldCallCallback()
    {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5 };
        var exceptions = new List<(int, Exception)>();

        // Act
        var result = source.TryWhere(
            x => {
                if (x == 3) {
                    throw new InvalidOperationException("Test exception");
                }
                return x % 2 == 0;
            },
            (item, ex) => exceptions.Add((item, ex))
        ).ToList();

        // Assert
        result.Should().Equal(2, 4);
        exceptions.Should().HaveCount(1);
        exceptions[0].Item1.Should().Be(3);
        exceptions[0].Item2.Message.Should().Be("Test exception");
    }

    [Fact]
    public void TryWhere_WithAllPredicatesThrowingException_ShouldReturnEmpty()
    {
        // Arrange
        var source = new[] { 1, 2, 3 };

        // Act
        var result = source.TryWhere(x => throw new InvalidOperationException("Test exception")).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void TryWhere_WithAllPredicatesThrowingExceptionAndCallback_ShouldCallCallbackForAll()
    {
        // Arrange
        var source = new[] { 1, 2, 3 };
        var exceptions = new List<(int, Exception)>();

        // Act
        var result = source.TryWhere(
            x => throw new InvalidOperationException($"Exception for {x}"),
            (item, ex) => exceptions.Add((item, ex))
        ).ToList();

        // Assert
        result.Should().BeEmpty();
        exceptions.Should().HaveCount(3);
        exceptions[0].Item1.Should().Be(1);
        exceptions[1].Item1.Should().Be(2);
        exceptions[2].Item1.Should().Be(3);
        exceptions[0].Item2.Message.Should().Be("Exception for 1");
        exceptions[1].Item2.Message.Should().Be("Exception for 2");
        exceptions[2].Item2.Message.Should().Be("Exception for 3");
    }

    [Fact]
    public void TryWhere_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<int>? source = null;

        // Act & Assert
        var action = () => source!.TryWhere(x => true).ToList();
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("source");
    }

    [Fact]
    public void TryWhere_WithNullPredicate_ShouldThrowArgumentNullException()
    {
        // Arrange
        var source = new[] { 1, 2, 3 };

        // Act & Assert
        var action = () => source.TryWhere(null!).ToList();
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("predicate");
    }

    [Fact]
    public void TryWhere_WithEmptySource_ShouldReturnEmpty()
    {
        // Arrange
        var source = new int[0];

        // Act
        var result = source.TryWhere(x => true).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void TryWhere_WithMixedExceptions_ShouldFilterCorrectly()
    {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5, 6 };

        // Act
        var result = source.TryWhere(x => {
            if (x == 2 || x == 4) {
                throw new InvalidOperationException("Test exception");
            }
            return x % 2 == 1; // 奇数
        }).ToList();

        // Assert
        result.Should().Equal(1, 3, 5);
    }

    [Fact]
    public void TryWhere_WithMixedExceptionsAndCallback_ShouldCallCallbackForExceptions()
    {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5, 6 };
        var exceptions = new List<(int, Exception)>();

        // Act
        var result = source.TryWhere(
            x => {
                if (x == 2 || x == 4) {
                    throw new InvalidOperationException($"Exception for {x}");
                }
                return x % 2 == 1; // 奇数
            },
            (item, ex) => exceptions.Add((item, ex))
        ).ToList();

        // Assert
        result.Should().Equal(1, 3, 5);
        exceptions.Should().HaveCount(2);
        exceptions[0].Item1.Should().Be(2);
        exceptions[1].Item1.Should().Be(4);
        exceptions[0].Item2.Message.Should().Be("Exception for 2");
        exceptions[1].Item2.Message.Should().Be("Exception for 4");
    }

    [Fact]
    public void TryWhere_WithNullCallback_ShouldNotThrow()
    {
        // Arrange
        var source = new[] { 1, 2, 3 };

        // Act
        var result = source.TryWhere(
            x => {
                if (x == 2) {
                    throw new InvalidOperationException("Test exception");
                }
                return x % 2 == 1;
            },
            null
        ).ToList();

        // Assert
        result.Should().Equal(1, 3);
    }
}
