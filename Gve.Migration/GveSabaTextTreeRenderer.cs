using Fusi.Tools.Configuration;
using Fusi.Tools.Data;
using Gve.Text;
using Proteus.Core.Text;
using Proteus.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Gve.Migration;

/// <summary>
/// A "Saba"-like TEI renderer for the GVE project. This renders a TEI structure
/// like that used for the Saba 1919 Project, based on the hierarchy <c>app</c>,
/// <c>lem</c>/<c>rdg</c>, <c>mod</c>, and text with critical markup. This
/// renderer is limited to rendering the descendants of <c>app</c>, because
/// each of them represents a text version. A composite renderer is then used
/// to compose the versions together.
/// <para>Tag: <c>text-tree-renderer.gve-saba</c>.</para>
/// </summary>
/// <seealso cref="TextTreeRenderer{THandledType}" />
[Tag("text-tree-renderer.gve-saba")]
public sealed class GveSabaTextTreeRenderer : TextTreeRenderer<XElement>,
    IConfigurable<XmlTextTreeRendererOptions>
{
    private XmlTextTreeRendererOptions _options = new()
    {
        RootElement = "tei:app",
        BlockElements = new Dictionary<string, string>
        {
            { "default", "tei:rdg" },
            { "preferred", "tei:lem" }
        },
    };

    private GveRendererContext? _context;

    /// <summary>
    /// Configures this renderer with the specified options.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <exception cref="ArgumentNullException">options</exception>
    public void Configure(XmlTextTreeRendererOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    private CharChainOperation GetSourceOperation(string traceValue)
    {
        // value is OPID TAGIN:TAGOUT NR, extract OPID
        int i = traceValue.IndexOf(' ');
        string opid = traceValue[..i];
        return _context!.Operations!.Find(op => op.Id == opid)!;
    }

    /// <summary>
    /// Renders the specified text linear tree.
    /// </summary>
    /// <param name="tree">The root node of the text tree.</param>
    /// <param name="context">The renderer context.</param>
    /// <returns>Rendered output.</returns>
    /// <exception cref="ArgumentNullException">tree or context</exception>
    protected override XElement DoRender(TreeNode<ExportedSegment> tree,
        IRendererContext context)
    {
        ArgumentNullException.ThrowIfNull(tree);
        ArgumentNullException.ThrowIfNull(context);

        // cast context
        _context = context as GveRendererContext;
        if (_context == null)
        {
            throw new InvalidOperationException(
                $"Context must be of type {nameof(GveRendererContext)}.");
        }

        // get the root element name (app)
        XName rootName = _options.ResolvePrefixedName(_options.RootElement);

        // get the block name (lem/rdg)
        string blockType = "default";
        if (context.Data.TryGetValue(
            XmlTextTreeRendererOptions.CONTEXT_BLOCK_TYPE_KEY,
            out object? value))
        {
            blockType = value as string ?? "default";
        }

        XName blockName = _options.ResolvePrefixedName(
            _options.BlockElements[blockType]);

        // create root element and add block as its child
        XElement root = new(rootName);
        XElement block = new(blockName);
        root.Add(block);

        TreeNode<ExportedSegment>? sourceNode = tree.FirstChild;
        XElement current = block;

        while (sourceNode != null)
        {
            // del-feature: subst/del or just del
            if (sourceNode.Data?.HasFeature(OperationFeature.F_TRACE_DELETED)
                == true)
            {
                // get the source operation
                CharChainOperation operation = GetSourceOperation(
                    sourceNode.Data.Features!.First(
                        f => f.Name == OperationFeature.F_TRACE_DELETED).Value!);

                // rep/swp is subst, else just del
                if (operation.Type == OperationType.Replace ||
                    operation.Type == OperationType.Swap)
                {
                    // subst/del/TEXT
                    XElement subst = new(NamespaceOptions.TEI + "subst",
                        new XAttribute("source", "o" + operation.Id),
                        new XElement(NamespaceOptions.TEI + "del",
                            sourceNode.Data.Text));
                    current.Add(subst);

                    // current XML element is now subst
                    current = subst;
                }
                else
                {
                    // del/TEXT
                    XElement del = new(NamespaceOptions.TEI + "del",
                        new XAttribute("source", "o" + operation.Id),
                        sourceNode.Data.Text);
                    current.Add(del);
                }
            } //del
            // seg-out/seg-out2 feature: ins/TEXT, then close subst if required
            else if ((sourceNode.Data?.HasFeature(
                         OperationFeature.F_TRACE_SEG_OUT) == true) ||
                     (sourceNode.Data?.HasFeature(
                         OperationFeature.F_TRACE_SEG2_OUT) == true))
            {
                // get the source operation
                CharChainOperation operation = GetSourceOperation(
                    sourceNode.Data.Features!.FirstOrDefault(
                        f => f.Name == OperationFeature.F_TRACE_SEG_OUT)?.Value!
                        ?? sourceNode.Data.Features!.First(f =>
                            f.Name == OperationFeature.F_TRACE_SEG_OUT).Value!);

                // ins/TEXT
                string sourceValue = "o" + operation.Id;
                XElement ins = new(NamespaceOptions.TEI + "ins",
                    new XAttribute("source", sourceValue),
                    sourceNode.Data.Text);
                current.Add(ins);

                // if current is child of subst, keep popping subst parents
                // until we find the one whose source attribute is = o + OPID
                // (i.e. the one we just created)
                while (current.Parent != null &&
                       current.Parent.Name == NamespaceOptions.TEI + "subst")
                {
                    XElement parent = current.Parent;
                    if (parent.Attribute("source")?.Value == sourceValue)
                        break;
                    // pop the subst
                    current = parent;
                }
            }
            else
            {
                current.Add(sourceNode.Data?.Text);
            }

            // move to next segment
            sourceNode = sourceNode.FirstChild;
        }

        return root;
    }
}
