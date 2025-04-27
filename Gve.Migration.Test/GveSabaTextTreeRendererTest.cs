using Fusi.Tools.Data;
using Gve.Text;
using Proteus.Rendering;
using System.Xml.Linq;

namespace Gve.Migration.Test;

public sealed class GveSabaTextTreeRendererTest
{
    private static (TreeNode<ExportedSegment>, GveRendererContext) GetPenMultiTree()
    {
        // get chain and its context
        var (context, operations) = GveTextTreeBuilderTest.GetPenContext();

        // configure the builder
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
            IncludeDeleted = true,
            StagedOnly = true
        };

        // build version trees
        TreeNode<ExportedSegment> multi = builder.Build(context, operations);

        // configure a linear merge filter to build export segments
        LinearMergeTextTreeFilter mergeFilter = new();
        mergeFilter.Configure(new MergeTextTreeFilterOptions
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

        // wrap the filter in a multi-text tree filter and apply it
        CompositeTextTreeFilter filter = new()
        {
            Component = mergeFilter
        };
        multi = filter.Apply(multi, context);

        return (multi, new()
        {
            OpContext = context,
            Operations = operations
        });
    }

    [Fact]
    public void Apply_Pen_Ok()
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
        (TreeNode<ExportedSegment> multi, GveRendererContext context) =
            GetPenMultiTree();

        // render the tree
        XElement? result = composite.Render(multi, context);

        Assert.NotNull(result);
        // TODO
    }

    private static (TreeNode<ExportedSegment>, GveRendererContext) GetHatMultiTree()
    {
        // get chain and its context
        var (context, operations) = GveTextTreeBuilderTest.GetHatContext();

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
            IncludeDeleted = true,
            StagedOnly = true
        };

        // build version trees
        TreeNode<ExportedSegment> multi = builder.Build(context, operations);

        // configure a linear merge filter to build export segments
        LinearMergeTextTreeFilter mergeFilter = new();
        mergeFilter.Configure(new MergeTextTreeFilterOptions
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

        // wrap the filter in a multi-text tree filter and apply it
        CompositeTextTreeFilter filter = new()
        {
            Component = mergeFilter
        };
        multi = filter.Apply(multi, context);

        return (multi, new()
        {
            OpContext = context,
            Operations = operations
        });
    }

    [Fact]
    public void Apply_Hat_Ok()
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
        (TreeNode<ExportedSegment> multi, GveRendererContext context) =
            GetHatMultiTree();

        // render the tree
        XElement? result = composite.Render(multi, context);

        Assert.NotNull(result);
        // TODO
    }
}
