using System;
using Xunit;

namespace Gve.Text.Test;

public sealed class CharChainAddAfterOperationTest
{
    #region Parsing
    [Fact]
    public void ParseOperation_At1Run1()
    {
        CharChainAddAfterOperation op = CharChainOperation
            .Parse<CharChainAddAfterOperation>("1+]X");

        Assert.Equal(VarOperationType.AddAfter, op.Type);
        Assert.Null(op.InputTag);
        Assert.Null(op.OutputTag);
        Assert.False(op.AtAsIndex);
        Assert.Equal(1, op.At);
        Assert.Equal(0, op.Run);
        Assert.Equal("X", op.Value);

        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Empty(op.Features);
        Assert.Empty(op.TaggedOrdinals);
    }

    [Fact]
    public void ParseOperation_At1Run1_Invalid()
    {
        CharChainAddAfterOperation op = CharChainOperation
            .Parse<CharChainAddAfterOperation>("1x1+]X");

        Assert.Throws<InvalidOperationException>(() =>
            op.Execute(new ChainOperationContext<char>()));
    }
    #endregion
}
