using Fusi.Tools;
using Fusi.Tools.Configuration;
using Fusi.Tools.Data;
using Proteus.Core.Text;
using Proteus.Rendering;
using System;
using System.Linq;
using System.Xml.Linq;

namespace Gve.Migration;

/// <summary>
/// Composite text tree renderer for "Saba"-like TEI for the GVE project. This
/// composes the renditions from each sub-tree (=version) into a single output,
/// rooted at the <c>app</c> element.
/// </summary>
[Tag("text-tree-renderer.composite.gve-saba")]
public sealed class GveSabaCompositeTextTreeRenderer :
    CompositeTextTreeRenderer<XElement>,
    IConfigurable<GveSabaCompositeTextTreeRendererOptions>
{
    private GveSabaCompositeTextTreeRendererOptions _options = new();

    /// <summary>
    /// Configures this renderer with the specified options.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <exception cref="ArgumentNullException">options</exception>
    public void Configure(GveSabaCompositeTextTreeRendererOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Called before rendering each sub-tree, this defines the block type
    /// (default or preferred) according to the version being rendered.
    /// </summary>
    /// <param name="tree">The multi-branches tree.</param>
    /// <param name="context">The renderer context.</param>
    /// <param name="index">The index of the sub-tree child of
    /// <paramref name="tree" /> to be rendered.</param>
    protected override void OnBeforeRendering(TreeNode<ExportedSegment> tree,
        IRendererContext context, int index)
    {
        switch (_options.PreferredTag)
        {
            case "$first":
                context.Data[XmlTextTreeRendererOptions.CONTEXT_BLOCK_TYPE_KEY]
                    = index == 0 ? "preferred" : "default";
                break;
            case "$last":
                context.Data[XmlTextTreeRendererOptions.CONTEXT_BLOCK_TYPE_KEY]
                    = index + 1 == tree.Children.Count ? "preferred" : "default";
                break;
            default:
                StringPair? f = tree.Children[index].Data?.Features?.FirstOrDefault(
                    f => f.Name == "v");
                context.Data[XmlTextTreeRendererOptions.CONTEXT_BLOCK_TYPE_KEY]
                    = f?.Name == _options.PreferredTag? "preferred" : "default";
                break;
        }
    }

    /// <summary>
    /// Composes the result for the specified sub-tree into
    /// <paramref name="target" />.
    /// </summary>
    /// <param name="tree">The sub-tree being the source of the result.</param>
    /// <param name="context">The rendering context.</param>
    /// <param name="result">The sub-tree rendering result.</param>
    /// <param name="target">The compose target.</param>
    /// <returns>
    /// New or received compose target.
    /// </returns>
    protected override XElement ComposeResult(TreeNode<ExportedSegment> tree,
        IRendererContext context, XElement? result, XElement? target)
    {
        ArgumentNullException.ThrowIfNull(tree);
        ArgumentNullException.ThrowIfNull(context);

        target ??= new XElement(NamespaceOptions.TEI + "app");
        if (result != null) target.Add(result);

        return target;
    }
}

/// <summary>
/// Options for <see cref="GveSabaCompositeTextTreeRenderer"/>.
/// </summary>
public class GveSabaCompositeTextTreeRendererOptions
{
    /// <summary>
    /// Gets or sets the version tag for the preferred text version. If not
    /// set, all the versions will be wrapped in <c>rdg</c>; else, the preferred
    /// version will be wrapped in <c>lem</c>. Use <c>$last</c> and <c>$first</c>
    /// to designate the last or first child of the source multi-tree root
    /// as the preferred version.
    /// </summary>
    public string? PreferredTag { get; set; }
}
