using Gve.Text.DTO;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gve.Api.Models;

/// <summary>
/// Operations binding model.
/// </summary>
public class OperationsBindingModel
{
    /// <summary>
    /// Gets or sets the operations.
    /// </summary>
    [Required]
    public IList<CharChainOperationSource> Operations { get; set; } = [];
}
