using Fusi.Tools.Data;
using Gve.Text;
using Proteus.Rendering;
using System.Collections.Generic;

namespace Gve.Migration.Test;

public sealed class GveSabaTextTreeRendererTest
{
    private static TreeNode<ExportedSegment> GetMultiTree()
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

        // use just 2 versions for short
        TreeNode<ExportedSegment> tree = new TreeNode<ExportedSegment>();
        for (int n = 5; n <= 6; n++)
        {
            TreeNode<ExportedSegment> sub = versionTrees[$"v{n}"];
            sub = filter.Apply(sub);
            tree.AddChild(sub);
        }

        return tree;
    }

    // TODO
}
