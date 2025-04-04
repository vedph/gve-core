using Fusi.Tools.Configuration;
using Fusi.Tools.Data;
using Proteus.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Gve.Migration;

/// <summary>
/// A "Saba"-like TEI renderer for the GVE project. This renders a TEI structure
/// like that used for the Saba 1919 Project, based on the hierarchy <c>app</c>,
/// <c>lem</c>/<c>rdg</c>, <c>mod</c>, and text with critical markup.
/// <para>Tag: <c>text-tree-renderer.gve-saba</c>.</para>
/// </summary>
/// <seealso cref="TextTreeRenderer" />
[Tag("text-tree-renderer.gve-saba")]
public sealed class GveSabaTextTreeRenderer : TextTreeRenderer,
    IConfigurable<XmlTextTreeRendererOptions>
{
    private XmlTextTreeRendererOptions _options = new()
    {
        RootElement = "tei:app",
        BlockElements = new Dictionary<string, string>
        {
            { "default", "tei:rdg" },
            { "final", "tei:lem" }
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

    /// <summary>
    /// Renders the specified text linear tree.
    /// </summary>
    /// <param name="tree">The root node of the text tree.</param>
    /// <param name="context">The renderer context.</param>
    /// <returns>Rendered output.</returns>
    /// <exception cref="ArgumentNullException">tree or context</exception>
    protected override string DoRender(TreeNode<ExportedSegment> tree,
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

        // create root element
        XElement root = new(rootName);
        XElement block = new(blockName);
        root.Add(block);

        // TODO

        return _options.IsRootIncluded
            ? root.ToString(_options.IsIndented
                ? SaveOptions.OmitDuplicateNamespaces
                : SaveOptions.OmitDuplicateNamespaces |
                  SaveOptions.DisableFormatting)
            : string.Concat(root.Nodes().Select(
                node => node.ToString(_options.IsIndented
                ? SaveOptions.OmitDuplicateNamespaces
                : SaveOptions.OmitDuplicateNamespaces |
                    SaveOptions.DisableFormatting)));
    }
}
