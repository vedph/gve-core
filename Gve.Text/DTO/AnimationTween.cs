using System.Collections.Generic;

namespace Gve.Text.DTO;

/// <summary>
/// A GVE animation tween.
/// </summary>
public class AnimationTween
{
    /// <summary>
    /// Gets or sets the tween type.
    /// </summary>
    public string Type { get; set; } = "";

    /// <summary>
    /// Gets or sets the selector for the target of this tween.
    /// </summary>
    public string Selector { get; set; } = "";

    /// <summary>
    /// Gets or sets the vars set serialized into a string.
    /// </summary>
    public string? Vars { get; set; }

    /// <summary>
    /// Gets or sets the secondary vars set serialized into a string.
    /// </summary>
    public string? Vars2 { get; set; }

    /// <summary>
    /// Gets or sets the optional position.
    /// </summary>
    public string? Position { get; set; }

    /// <summary>
    /// Gets or sets the label.
    /// </summary>
    public string Label { get; set; } = "";

    /// <summary>
    /// Gets or sets an optional note.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return $"[{Type}] {Label}: {Selector}";
    }
}
