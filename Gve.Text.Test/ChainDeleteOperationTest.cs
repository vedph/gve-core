using Xunit;

namespace Gve.Text.Test;

public sealed class ChainDeleteOperationTest
{
    [Fact]
    public void ParseOperation_At1Run1()
    {
        ChainDeleteOperation<char> op = ChainOperation<char>
            .Parse<ChainDeleteOperation<char>>("1x1-");

        Assert.Equal(VarOperationType.Delete, op.Type);
        Assert.Null(op.InputTag);
        Assert.Null(op.OutputTag);
        Assert.False(op.UseIndex);
        Assert.Equal(1, op.At);
        Assert.Equal(1, op.Run);
        Assert.Null(op.Value);

        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Empty(op.Features);
        Assert.Empty(op.TaggedOrdinals);
    }

    [Fact]
    public void ParseOperation_AtIndex1Run2()
    {
        ChainDeleteOperation<char> op = ChainOperation<char>
            .Parse<ChainDeleteOperation<char>>("@1x2-");

        Assert.Equal(VarOperationType.Delete, op.Type);
        Assert.Null(op.InputTag);
        Assert.Null(op.OutputTag);
        Assert.True(op.UseIndex);
        Assert.Equal(1, op.At);
        Assert.Equal(2, op.Run);
        Assert.Null(op.Value);

        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Empty(op.Features);
        Assert.Empty(op.TaggedOrdinals);
    }

    [Fact]
    public void ParseOperation_VerAt1Run1()
    {
        ChainDeleteOperation<char> op = ChainOperation<char>
            .Parse<ChainDeleteOperation<char>>("(v0:v1)1x1-");

        Assert.Equal(VarOperationType.Delete, op.Type);
        Assert.Equal("v0", op.InputTag);
        Assert.Equal("v1", op.OutputTag);
        Assert.False(op.UseIndex);
        Assert.Equal(1, op.At);
        Assert.Equal(1, op.Run);
        Assert.Null(op.Value);

        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Empty(op.Features);
        Assert.Empty(op.TaggedOrdinals);
    }

    [Fact]
    public void ParseOperation_VerAt1Run1Rank()
    {
        ChainDeleteOperation<char> op = ChainOperation<char>
            .Parse<ChainDeleteOperation<char>>("(v0:v1)1x1-^2");

        Assert.Equal(VarOperationType.Delete, op.Type);
        Assert.Equal("v0", op.InputTag);
        Assert.Equal("v1", op.OutputTag);
        Assert.False(op.UseIndex);
        Assert.Equal(1, op.At);
        Assert.Equal(1, op.Run);
        Assert.Null(op.Value);

        Assert.Equal(2, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Empty(op.Features);
        Assert.Empty(op.TaggedOrdinals);
    }

    [Fact]
    public void ParseOperation_VerAt1Run1Group()
    {
        ChainDeleteOperation<char> op = ChainOperation<char>
            .Parse<ChainDeleteOperation<char>>("(v0:v1)1x1-@g1");

        Assert.Equal(VarOperationType.Delete, op.Type);
        Assert.Equal("v0", op.InputTag);
        Assert.Equal("v1", op.OutputTag);
        Assert.False(op.UseIndex);
        Assert.Equal(1, op.At);
        Assert.Equal(1, op.Run);
        Assert.Null(op.Value);

        Assert.Equal(0, op.Rank);
        Assert.Equal("g1", op.GroupId);
        Assert.Empty(op.Features);
        Assert.Empty(op.TaggedOrdinals);
    }

    [Fact]
    public void ParseOperation_VerAt1Run1Tags()
    {
        ChainDeleteOperation<char> op = ChainOperation<char>
            .Parse<ChainDeleteOperation<char>>("(v0:v1)1x1- [alpha=1 beta=2]");

        Assert.Equal(VarOperationType.Delete, op.Type);
        Assert.Equal("v0", op.InputTag);
        Assert.Equal("v1", op.OutputTag);
        Assert.False(op.UseIndex);
        Assert.Equal(1, op.At);
        Assert.Equal(1, op.Run);
        Assert.Null(op.Value);

        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Empty(op.Features);
        Assert.Equal(2, op.TaggedOrdinals.Count);
        Assert.Equal(1, op.TaggedOrdinals["alpha"]);
        Assert.Equal(2, op.TaggedOrdinals["beta"]);
    }

    [Fact]
    public void ParseOperation_VerAt1Run1Meta()
    {
        ChainDeleteOperation<char> op = ChainOperation<char>
            .Parse<ChainDeleteOperation<char>>("(v0:v1)1x1- {note=sample}");

        Assert.Equal(VarOperationType.Delete, op.Type);
        Assert.Equal("v0", op.InputTag);
        Assert.Equal("v1", op.OutputTag);
        Assert.False(op.UseIndex);
        Assert.Equal(1, op.At);
        Assert.Equal(1, op.Run);
        Assert.Null(op.Value);

        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Single(op.Features);
        Assert.Equal("note", op.Features[0].Name);
        Assert.Equal("sample", op.Features[0].Value);
        Assert.False(op.Features[0].IsUnique);
        Assert.Empty(op.TaggedOrdinals);
    }
}
