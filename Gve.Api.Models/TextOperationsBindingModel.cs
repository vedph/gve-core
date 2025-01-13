using Gve.Text.DTO;
using System.ComponentModel.DataAnnotations;

namespace Gve.Api.Models;

/// <summary>
/// Text with operations binding model.
/// </summary>
/// <seealso cref="OperationsBindingModel" />
public class TextOperationsBindingModel : OperationsBindingModel
{
    /// <summary>
    /// Gets or sets the base text as an array of char-based chain nodes.
    /// </summary>
    [Required]
    public CharChainNode[] Text { get; set; } = [];
}
