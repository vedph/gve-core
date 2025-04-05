using Fusi.Tools.Data;

namespace Proteus.Rendering;

/// <summary>
/// Base class for composite <see cref="ITextTreeRenderer{T}"/>'s. This
/// executes the inner renderer for each sub-tree in the received tree,
/// progressively composing the result into a single output.
/// </summary>
/// <seealso cref="IHasCompositeComponent{T}"/>
public abstract class CompositeTextTreeRenderer<THandledType> :
    TextTreeRenderer<THandledType>,
    IHasCompositeComponent<ITextTreeRenderer<THandledType>>
{
    /// <summary>
    /// Gets or sets the inner component.
    /// </summary>
    public ITextTreeRenderer<THandledType>? Component { get; set; }

    /// <summary>
    /// Composes the result for the specified sub-tree into <paramref name="target"/>.
    /// </summary>
    /// <param name="tree">The sub-tree being the source of the result.</param>
    /// <param name="context">The rendering context.</param>
    /// <param name="result">The sub-tree rendering result.</param>
    /// <param name="target">The compose target.</param>
    /// <returns>New or received compose target.</returns>
    protected abstract THandledType ComposeResult(TreeNode<ExportedSegment> tree,
        IRendererContext context, THandledType? result, THandledType? target);

    /// <summary>
    /// Renders the specified multi-tree.
    /// </summary>
    /// <param name="tree">The root node of the text tree.</param>
    /// <param name="context">The renderer context.</param>
    /// <returns>Rendered output.</returns>
    protected override THandledType? DoRender(TreeNode<ExportedSegment> tree,
        IRendererContext context)
    {
        if (!tree.HasChildren || Component == null) return default;

        THandledType? target = default;
        foreach (TreeNode<ExportedSegment> sub in tree.Children)
        {
            THandledType? result = Component.Render(sub, context);
            target = ComposeResult(sub, context, result, target);
        }

        return target;
    }
}
