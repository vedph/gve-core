using Fusi.Tools.Configuration;
using Fusi.Tools.Data;
using Proteus.Core.Text;
using Proteus.Rendering;
using System;
using System.Xml.Linq;

namespace Gve.Migration;

/// <summary>
/// Composite text tree renderer for "Saba"-like TEI for the GVE project. This
/// composes the renditions from each sub-tree (=version) into a single output,
/// rooted at the <c>app</c> element.
/// </summary>
[Tag("text-tree-renderer.composite.gve-saba")]
public sealed class GveSabaCompositeTextTreeRenderer :
    CompositeTextTreeRenderer<XElement>
{
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
