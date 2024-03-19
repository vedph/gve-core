using Xunit;

namespace Gve.Text.Test;

public sealed class CharChainAnnotateOperationTest
{
    [Fact]
    public void ParseOperation_At0()
    {
        CharChainAnnotateOperation op = CharChainOperation
            .Parse<CharChainAnnotateOperation>("0: {note=sample}");

        Assert.Equal(VarOperationType.Annotate, op.Type);
        Assert.Null(op.InputTag);
        Assert.Null(op.OutputTag);
        Assert.False(op.AtAsIndex);
        Assert.Equal(0, op.At);
        Assert.Equal(1, op.Run);
        Assert.Null(op.Value);
        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Single(op.Features);
        Assert.Empty(op.TaggedOrdinals);
    }

    [Fact]
    public void ParseOperation_At3Run2()
    {
        CharChainAnnotateOperation op = CharChainOperation
            .Parse<CharChainAnnotateOperation>("3x2: {note=sample}");

        Assert.Equal(VarOperationType.Annotate, op.Type);
        Assert.Null(op.InputTag);
        Assert.Null(op.OutputTag);
        Assert.False(op.AtAsIndex);
        Assert.Equal(3, op.At);
        Assert.Equal(2, op.Run);
        Assert.Null(op.Value);
        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Single(op.Features);
        Assert.Empty(op.TaggedOrdinals);
    }
}
