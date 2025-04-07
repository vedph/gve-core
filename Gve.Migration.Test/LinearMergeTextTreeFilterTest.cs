using Fusi.Tools.Data;
using Gve.Text;
using Proteus.Rendering;
using System.Collections.Generic;

namespace Gve.Migration.Test;

public sealed class LinearMergeTextTreeFilterTest
{
    [Fact]
    public void Apply_Pen_NoDeletions()
    {
        // get chain and its context
        var (context, operations) = GveTextTreeBuilderTest.GetPenContext();

        // get tree for final version (v7)
        GveTextTreeBuilder builder = new()
        {
            FeatureNames =
            [
                OperationFeature.F_TRACE_SEG_IN,
                OperationFeature.F_TRACE_SEG2_IN,
                OperationFeature.F_TRACE_SEG_OUT,
                OperationFeature.F_TRACE_SEG2_OUT,
                OperationFeature.F_TRACE_DELETED
            ],
            TraceFeatureFlattening = true,
            IncludeDeleted = false
        };

        IDictionary<string, TreeNode<ExportedSegment>> versionTrees =
            builder.Build(context, operations);

        TreeNode<ExportedSegment> tree = versionTrees["v6"];

        LinearMergeTextTreeFilter filter = new();
        filter.Configure(new MergeTextTreeFilterOptions
        {
            Features =
            [
                OperationFeature.F_TRACE_SEG_OUT,
                OperationFeature.F_TRACE_SEG2_OUT,
            ],
            ValueFilters =
            [
                new MergeTextTreeFilterRepOption
                {
                    IsRegex = true,
                    Find = "^([^ ]+).*$",
                    Replace = "$1",
                }
            ]
        });

        // act
        TreeNode<ExportedSegment> filtered = filter.Apply(tree);

        // assert
        Assert.NotNull(filtered);

        // a+space
        TreeNode<ExportedSegment>? seg = filtered.FirstChild;
        Assert.NotNull(seg);
        Assert.Equal("a ", seg.Data?.Text);

        // cat
        seg = seg.FirstChild;
        Assert.NotNull(seg);
        Assert.Equal("cat", seg.Data?.Text);

        // space
        seg = seg.FirstChild;
        Assert.NotNull(seg);
        Assert.Equal(" ", seg.Data?.Text);

        // is+space
        seg = seg.FirstChild;
        Assert.NotNull(seg);
        Assert.Equal("is ", seg.Data?.Text);

        // nice
        seg = seg.FirstChild;
        Assert.NotNull(seg);
        Assert.Equal("nice", seg.Data?.Text);

        // end
        Assert.Null(seg.FirstChild);
    }

    [Fact]
    public void Apply_Pen_Deletions()
    {
        // get chain and its context
        var (context, operations) = GveTextTreeBuilderTest.GetPenContext();

        // get tree for final version (v7)
        GveTextTreeBuilder builder = new()
        {
            FeatureNames =
            [
                OperationFeature.F_TRACE_SEG_IN,
                OperationFeature.F_TRACE_SEG2_IN,
                OperationFeature.F_TRACE_SEG_OUT,
                OperationFeature.F_TRACE_SEG2_OUT,
                OperationFeature.F_TRACE_DELETED
            ],
            TraceFeatureFlattening = true,
            IncludeDeleted = true
        };

        IDictionary<string, TreeNode<ExportedSegment>> versionTrees =
            builder.Build(context, operations);

        TreeNode<ExportedSegment> tree = versionTrees["v6"];

        LinearMergeTextTreeFilter filter = new();
        filter.Configure(new MergeTextTreeFilterOptions
        {
            Features =
            [
                OperationFeature.F_TRACE_SEG_OUT,
                OperationFeature.F_TRACE_SEG2_OUT,
                OperationFeature.F_TRACE_DELETED
            ],
            ValueFilters =
            [
                new MergeTextTreeFilterRepOption
                {
                    IsRegex = true,
                    Find = "^([^ ]+).*$",
                    Replace = "$1",
                }
            ]
        });

        // act
        TreeNode<ExportedSegment> filtered = filter.Apply(tree);

        // assert
        Assert.NotNull(filtered);

        // a+space
        TreeNode<ExportedSegment>? seg = filtered.FirstChild;
        Assert.NotNull(seg);
        Assert.Equal("a ", seg.Data?.Text);

        // nice (del)
        seg = seg.FirstChild;
        Assert.NotNull(seg);
        Assert.Equal("nice", seg.Data?.Text);
        Assert.Equal(seg.Data!.Text, seg.Label);
        Assert.NotNull(seg.Data?.Features);
        Assert.Contains(seg.Data.Features,
            f => f.Name == OperationFeature.F_TRACE_DELETED);

        // red+space (del)
        seg = seg.FirstChild;
        Assert.NotNull(seg);
        Assert.Equal("red ", seg.Data?.Text);
        Assert.Equal(seg.Data!.Text, seg.Label);
        Assert.NotNull(seg.Data?.Features);
        Assert.Contains(seg.Data.Features,
            f => f.Name == OperationFeature.F_TRACE_DELETED);

        // pen (del)
        seg = seg.FirstChild;
        Assert.NotNull(seg);
        Assert.Equal("pen", seg.Data?.Text);
        Assert.Equal(seg.Data!.Text, seg.Label);
        Assert.NotNull(seg.Data?.Features);
        Assert.Contains(seg.Data.Features,
            f => f.Name == OperationFeature.F_TRACE_DELETED);

        // hat (del)
        seg = seg.FirstChild;
        Assert.NotNull(seg);
        Assert.Equal("hat", seg.Data?.Text);
        Assert.Equal(seg.Data!.Text, seg.Label);
        Assert.NotNull(seg.Data?.Features);
        Assert.Contains(seg.Data.Features,
            f => f.Name == OperationFeature.F_TRACE_DELETED);

        // cat
        seg = seg.FirstChild;
        Assert.NotNull(seg);
        Assert.Equal("cat", seg.Data?.Text);
        Assert.Equal(seg.Data!.Text, seg.Label);

        // space
        seg = seg.FirstChild;
        Assert.NotNull(seg);
        Assert.Equal(" ", seg.Data?.Text);
        Assert.Equal(seg.Data!.Text, seg.Label);

        // is+space
        seg = seg.FirstChild;
        Assert.NotNull(seg);
        Assert.Equal("is ", seg.Data?.Text);
        Assert.Equal(seg.Data!.Text, seg.Label);

        // cat (del)
        seg = seg.FirstChild;
        Assert.NotNull(seg);
        Assert.Equal("cat", seg.Data?.Text);
        Assert.Equal(seg.Data!.Text, seg.Label);
        Assert.NotNull(seg.Data?.Features);
        Assert.Contains(seg.Data.Features,
            f => f.Name == OperationFeature.F_TRACE_DELETED);

        // nice
        seg = seg.FirstChild;
        Assert.NotNull(seg);
        Assert.Equal("nice", seg.Data?.Text);
        Assert.Equal(seg.Data!.Text, seg.Label);
    }
}
