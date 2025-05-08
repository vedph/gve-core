using Fusi.Tools.Data;
using Gve.Text;
using Proteus.Rendering;
using System.Collections.Generic;
using System;
using System.Xml.Linq;
using Proteus.Rendering.Filters;

namespace Gve.Migration.Test;

public sealed class GveSabaTextTreeRendererTest
{
    private static TreeNode<ExportedSegment> WrapAndApply(
        TreeNode<ExportedSegment> tree,
        ChainOperationContext<char> context,
        ITextTreeFilter filter)
    {
        CompositeTextTreeFilter composite = new()
        {
            Component = filter
        };
        return composite.Apply(tree, context);
    }

    private static (TreeNode<ExportedSegment>, GveRendererContext)
        GetMultiTree(ChainOperationContext<char> context,
        List<CharChainOperation> operations)
    {
        // get tree for final version
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

        // apply a linear merge filter to build export segments
        MergeLinearTextTreeFilter mergeFilter = new();
        mergeFilter.Configure(new MergeLinearTextTreeFilterOptions
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
        multi = WrapAndApply(multi, context, mergeFilter);

        // apply a block filter
        multi = WrapAndApply(multi, context, new BlockLinearTextTreeFilter());

        return (multi, new()
        {
            OpContext = context,
            Operations = operations
        });
    }

    private static (
        ChainOperationContext<char> Context,
        List<CharChainOperation> Operations) GetLimerickContext()
    {
        // https://vedph.github.io/gve-doc/model/sample-limerick.html
        const string text = "there was an old man with a beard,\n" +
            "who cried: \"It is just as I feared!\n" +
            "four larks and a wren,\n" +
            "two swans and a hen,\n" +
            "all built their nests in my beard!\"";

        // setup text
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom(text));
        context.LastTag = "v0";
        Assert.Equal(text, TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v0")));

        // setup operations
        const string operationsText =
            // v0 -> v1
            "40x5=\"said\" [*log=\"said for cried\" x=\"150\" y=\"80\" " +
                "style=\"font-size:28px;fill:red\"]\n" +
            // v1 -> v2
            "99x5=\"crows\" [*log=\"crows for swans\" x=\"120\" y=\"200\" " +
                "style=\"font-size:28px;fill:red\"]\n" +
            // v2 -> v3
            "116+[\"have \" [*log=\"insert have\" *version^=\"alpha\" " +
                "reason=\"metrical\" x=\"35\" y=\"260\" " +
                "style=\"font-size:28px;fill:red\"]\n" +
            // v3 -> v4
            "72x23<>95x21 [*log=\"swap verses\"]\n" +
            // v4 -> v5
            "155x5=\"owls\" [*log=\"owls for crows\" *version^=\"beta\" " +
                "x=\"195\" y=\"195\" style=\"font-size:28px;fill:red\"]";

        List<CharChainOperation> operations = [];
        foreach (string line in operationsText.Split('\n',
            StringSplitOptions.RemoveEmptyEntries))
        {
            CharChainOperation op = CharChainOperation
                .Parse<CharChainOperation>(line);
            operations.Add(op);
        }

        // execute operations
        foreach (CharChainOperation op in operations) op.Execute(context);

        return (context, operations);
    }

    private static (TreeNode<ExportedSegment>, GveRendererContext) GetPenMultiTree()
    {
        // get chain and its context
        var (context, operations) = GveTextTreeBuilderTest.GetPenContext();

        return GetMultiTree(context, operations);
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

        return GetMultiTree(context, operations);
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

    private static (
        TreeNode<ExportedSegment>, GveRendererContext) GetLimerickMultiTree()
    {
        // get chain and its context
        var (context, operations) = GetLimerickContext();
        return GetMultiTree(context, operations);
    }

    [Fact]
    public void Apply_Limerick_Ok()
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
            GetLimerickMultiTree();

        // render the tree
        XElement? result = composite.Render(multi, context);

        Assert.NotNull(result);
        // TODO
    }
}
