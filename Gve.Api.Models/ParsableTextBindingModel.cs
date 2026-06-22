using System.ComponentModel.DataAnnotations;

namespace Gve.Api.Models;

/// <summary>
/// Parsable text binding model.
/// </summary>
public class ParsableTextBindingModel
{
    /// <summary>
    /// Gets or sets the text to parse.
    /// </summary>
    [Required]
    [MaxLength(20000)]
    public string Text { get; set; } = "";
}
