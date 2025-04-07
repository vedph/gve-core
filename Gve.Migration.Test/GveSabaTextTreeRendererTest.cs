using Fusi.Tools.Data;
using Gve.Text;
using Proteus.Rendering;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Gve.Migration.Test;

public sealed class GveSabaTextTreeRendererTest
{
    private static (TreeNode<ExportedSegment>, GveRendererContext) GetMultiTree()
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
        TreeNode<ExportedSegment> tree = builder.Build(context, operations);

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

        // use just 2 versions: v4 and v6

        // create rendering context
        GveRendererContext rc = new()
        {
            OpContext = context,
            Operations = operations
        };

        return (tree, rc);
    }

    [Fact]
    public void Apply_Ok()
    {
        // create a composite renderer
        GveSabaCompositeTextTreeRenderer composite = new()
        {
            Component = new GveSabaTextTreeRenderer()
        };
        // configure it with a GVE tree renderer
        composite.Configure(new GveSabaCompositeTextTreeRendererOptions
        {
            PreferredTag = "$last"
        });
        // get the multi-tree
        (TreeNode<ExportedSegment> tree, GveRendererContext context) =
            GetMultiTree();

        // render the tree
        XElement? result = composite.Render(tree, context);

        Assert.NotNull(result);
        // TODO
    }
}
