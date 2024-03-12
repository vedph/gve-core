using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gve.Text.Test;

public sealed class CharChainDeleteOperationTest
{
    [Fact]
    public void ParseOperation_At1Run1()
    {
        CharChainDeleteOperation op = CharChainOperation
            .Parse<CharChainDeleteOperation>("1x1-");

        Assert.Equal(VarOperationType.Delete, op.Type);
        Assert.Null(op.InputTag);
        Assert.Null(op.OutputTag);
        Assert.False(op.AtAsIndex);
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
        CharChainDeleteOperation op = CharChainOperation
            .Parse<CharChainDeleteOperation>("@1x2-");

        Assert.Equal(VarOperationType.Delete, op.Type);
        Assert.Null(op.InputTag);
        Assert.Null(op.OutputTag);
        Assert.True(op.AtAsIndex);
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
        CharChainDeleteOperation op = CharChainOperation
            .Parse<CharChainDeleteOperation>("(v0:v1)1x1-");

        Assert.Equal(VarOperationType.Delete, op.Type);
        Assert.Equal("v0", op.InputTag);
        Assert.Equal("v1", op.OutputTag);
        Assert.False(op.AtAsIndex);
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
        CharChainDeleteOperation op = CharChainOperation
            .Parse<CharChainDeleteOperation>("(v0:v1)1x1-^2");

        Assert.Equal(VarOperationType.Delete, op.Type);
        Assert.Equal("v0", op.InputTag);
        Assert.Equal("v1", op.OutputTag);
        Assert.False(op.AtAsIndex);
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
        CharChainDeleteOperation op = CharChainOperation
            .Parse<CharChainDeleteOperation>("(v0:v1)1x1-@g1");

        Assert.Equal(VarOperationType.Delete, op.Type);
        Assert.Equal("v0", op.InputTag);
        Assert.Equal("v1", op.OutputTag);
        Assert.False(op.AtAsIndex);
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
        CharChainDeleteOperation op = CharChainOperation
            .Parse<CharChainDeleteOperation>("(v0:v1)1x1- [alpha=1 beta=2]");

        Assert.Equal(VarOperationType.Delete, op.Type);
        Assert.Equal("v0", op.InputTag);
        Assert.Equal("v1", op.OutputTag);
        Assert.False(op.AtAsIndex);
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
        CharChainDeleteOperation op = CharChainOperation
            .Parse<CharChainDeleteOperation>("(v0:v1)1x1- {note=sample}");

        Assert.Equal(VarOperationType.Delete, op.Type);
        Assert.Equal("v0", op.InputTag);
        Assert.Equal("v1", op.OutputTag);
        Assert.False(op.AtAsIndex);
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

    private static string GetVersionString(IList<ChainNode<char>> nodes) =>
        new(nodes.Select(n => n.Data).ToArray());

    private static List<ChainNode<char>> GetNodesFrom(string text)
    {
        return text.Select((c, _) =>
            new ChainNode<char>(c, $"{(char.IsWhiteSpace(c) ? '_' : c)}"))
            .ToList();
    }

    [Fact]
    public void Execute_DeleteSingle()
    {
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", GetNodesFrom("ABC"));
        CharChainDeleteOperation op = new()
        {
            At = 1,
            Run = 1,
            InputTag = "v0",
            OutputTag = "v1",
        };

        op.Execute(context);

        Assert.Equal("BC", GetVersionString(context.Chain.GetTagNodes("v1")));
    }
}
