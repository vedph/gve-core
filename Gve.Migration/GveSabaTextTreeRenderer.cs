using Fusi.Tools.Configuration;
using Fusi.Tools.Data;
using Gve.Text;
using Proteus.Core.Text;
using Proteus.Rendering;
using Proteus.Rendering.Filters;
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
    IConfigurable<GveSabaTextTreeRendererOptions>
{
    private GveSabaTextTreeRendererOptions _options = new()
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
    public void Configure(GveSabaTextTreeRendererOptions options)
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

    private static XElement? GetAscendantOrSelf(XElement? element, XName name)
    {
        if (element == null) return null;
        if (element.Name ==  name) return element;

        XElement? parent = element.Parent;
        while (parent != null)
        {
            if (parent.Name == name) return parent;
            parent = parent.Parent;
        }
        return null;
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
        XElement block = new(blockName);

        // add mod as child of lem/rdg
        XElement mod = new(NamespaceOptions.TEI + "mod");
        string? tag = tree.Data?.Features?.FirstOrDefault(
                f => f.Name == CompositeTextTreeFilter.F_SUBTREE_ID)?.Value;
        if (tag != null) mod.SetAttributeValue("n", tag);
        block.Add(mod);

        // add content inside mod
        TreeNode<ExportedSegment>? sourceNode = tree.FirstChild;
        XElement current = mod;

        // add line element if requested
        if (_options.LineElement != null)
        {
            XElement line = new(_options.ResolvePrefixedName(_options.LineElement));
            current.Add(line);
            current = line;
        }

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
                            f.Name == OperationFeature.F_TRACE_SEG2_OUT).Value!);

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

            // handle end-of-line after processed node
            if (sourceNode.Data?.HasFeature(ExportedSegment.F_EOL_TAIL) == true)
            {
                if (_options.LineElement != null)
                {
                    // get the line element name
                    XName lineElementName = _options.ResolvePrefixedName(
                        _options.LineElement);

                    // get the nearest enclosing line element starting from current
                    XElement? currentLine = GetAscendantOrSelf(current, lineElementName);

                    // if found (this should always happen), add a new sibling
                    // line element and make it the current target element
                    // TODO: consider subst?
                    if (currentLine?.Parent != null)
                    {
                        XElement line = new(lineElementName);
                        currentLine.Parent.Add(line);
                        current = line;
                    }
                }
                else
                {
                    // if no line element, just add a new line
                    current.Add(Environment.NewLine);
                }
            }

            // move to next segment
            sourceNode = sourceNode.FirstChild;
        }

        return block;
    }
}

/// <summary>
/// Options for <see cref="GveSabaTextTreeRenderer"/>.
/// </summary>
public class GveSabaTextTreeRendererOptions : XmlTextTreeRendererOptions
{
    /// <summary>
    /// The name of the element representing each line of the source text
    /// (e.g. <c>tei:l</c>, <c>tei:p</c>), or null to avoid emitting such
    /// elements. When set, line elements are children of <c>mod</c>.
    /// </summary>
    public string? LineElement { get; set; }
}
