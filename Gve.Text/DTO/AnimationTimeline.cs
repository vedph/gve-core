using System.Collections.Generic;

namespace Gve.Text.DTO;

/// <summary>
/// A GVE animation timeline.
/// </summary>
public class AnimationTimeline
{
    /// <summary>
    /// Gets or sets the output version tag, used as the identifier for each
    /// timeline in the scope of a given snapshot.
    /// </summary>
    public string Tag { get; set; } = "";

    /// <summary>
    /// Gets or sets the tweens in the timeline.
    /// </summary>
    public IList<AnimationTween> Tweens { get; set; } = [];

    /// <summary>
    /// Gets or sets the vars set.
    /// </summary>
    public Dictionary<string, object>? Vars { get; set; }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return $"{Tag}: {Tweens.Count}";
    }
}
