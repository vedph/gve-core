using Fusi.Tools.Data;
using Gve.Text;
using System.Collections.Generic;

namespace Gve.Migration.Test;

public sealed class LinearMergeTextTreeFilterTest
{
    [Fact]
    public void Apply_Pen_NoDeletion()
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
                    Find = "$([^ ]+)",
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
}
