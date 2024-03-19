using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Gve.Text.Test;

public sealed class CharChainReplaceOperationTest
{
    #region Parsing
    [Fact]
    public void ParseOperation_At1Run1()
    {
        CharChainReplaceOperation op = CharChainOperation
            .Parse<CharChainReplaceOperation>("1x1=X");

        Assert.Equal(VarOperationType.Replace, op.Type);
        Assert.Null(op.InputTag);
        Assert.Null(op.OutputTag);
        Assert.False(op.AtAsIndex);
        Assert.Equal(1, op.At);
        Assert.Equal(1, op.Run);
        Assert.Equal("X", op.Value);

        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Empty(op.Features);
        Assert.Empty(op.TaggedOrdinals);
    }

    [Fact]
    public void ParseOperation_AtIndex1Run2()
    {
        CharChainReplaceOperation op = CharChainOperation
            .Parse<CharChainReplaceOperation>("@1x2=X");

        Assert.Equal(VarOperationType.Replace, op.Type);
        Assert.Null(op.InputTag);
        Assert.Null(op.OutputTag);
        Assert.True(op.AtAsIndex);
        Assert.Equal(1, op.At);
        Assert.Equal(2, op.Run);
        Assert.Equal("X", op.Value);

        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Empty(op.Features);
        Assert.Empty(op.TaggedOrdinals);
    }

    [Fact]
    public void ParseOperation_VerAt1Run1()
    {
        CharChainReplaceOperation op = CharChainOperation
            .Parse<CharChainReplaceOperation>("(v0:v1)1x1=X");

        Assert.Equal(VarOperationType.Replace, op.Type);
        Assert.Equal("v0", op.InputTag);
        Assert.Equal("v1", op.OutputTag);
        Assert.False(op.AtAsIndex);
        Assert.Equal(1, op.At);
        Assert.Equal(1, op.Run);
        Assert.Equal("X", op.Value);

        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Empty(op.Features);
        Assert.Empty(op.TaggedOrdinals);
    }
    #endregion
}
