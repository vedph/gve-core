using System.Collections.Generic;

namespace Gve.Text.DTO;

/// <summary>
/// A GVE snapshot DTO object containing all the data required to represent
/// a snapshot. All its objects are POCO objects without any logic.
/// </summary>
public class Snapshot
{
    /// <summary>
    /// Gets or sets the visual snapshot's size in pixels.
    /// </summary>
    public Size Size { get; set; }

    /// <summary>
    /// Gets or sets the snapshot CSS style.
    /// </summary>
    public string? Style { get; set; }

    /// <summary>
    /// Gets or sets the optional SVG defs element code. The <c>defs</c>
    /// element is used to store graphical objects that will be used at a later
    /// time (via <c>use</c>: see e.g.
    /// https://developer.mozilla.org/en-US/docs/Web/SVG/Element/defs).
    /// </summary>
    public string? Defs { get; set; }

    /// <summary>
    /// Gets or sets the optional snapshot background image.
    /// </summary>
    public SnapshotImage? Image { get; set; }

    /// <summary>
    /// Gets or sets the text nodes.
    /// </summary>
    public List<CharChainNode> Text { get; set; } = [];

    /// <summary>
    /// Gets or sets the base text layer CSS style.
    /// </summary>
    public string? TextStyle { get; set; }

    /// <summary>
    /// Gets or sets the SVG base text options.
    /// </summary>
    public SvgBaseTextOptions TextOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets the snapshot operations.
    /// </summary>
    public IList<CharChainOperationSource> Operations { get; set; } = [];

    /// <summary>
    /// Gets or sets the operations layer CSS style.
    /// </summary>
    public string? OpStyle { get; set; }

    /// <summary>
    /// Gets or sets the animation timelines.
    /// </summary>
    public IDictionary<string, AnimationTimeline>? Timelines { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Snapshot"/> class.
    /// </summary>
    public Snapshot()
    {
        Size = new Size { Width = 800, Height = 600 };
    }
}
