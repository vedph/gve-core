using Fusi.Tools.Data;
using Gve.Text;
using Proteus.Rendering;
using System.Collections.Generic;
using System.Xml.Linq;

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

        // build version trees
        IDictionary<string, TreeNode<ExportedSegment>> versionTrees =
            builder.Build(context, operations);

        // configure a linear merge filter to build export segments
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

        // use just the 2 last versions for short
        TreeNode<ExportedSegment> tree = new();
        for (int n = 5; n <= 6; n++)
        {
            // get the version tree and segment it
            string tag = $"v{n}";
            TreeNode<ExportedSegment> sub = versionTrees[tag];
            sub = filter.Apply(sub);

            // add the segment to the multi-tree as a child
            // with a "v" feature equal to the version tag
            sub.Data ??= new ExportedSegment();
            sub.Data.AddFeature("v", tag);
            tree.AddChild(sub);
        }

        return tree;
    }

    [Fact]
    public void Apply_Ok()
    {
        GveSabaCompositeTextTreeRenderer composite = new()
        {
            Component = new GveSabaTextTreeRenderer()
        };
        TreeNode<ExportedSegment> tree = GetMultiTree();

        GveRendererContext context = new();

        XElement? result = composite.Render(tree, context);

        Assert.NotNull(result);
        // TODO
    }
}
