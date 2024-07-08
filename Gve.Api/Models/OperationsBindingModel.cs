using Gve.Text.DTO;
using System.ComponentModel.DataAnnotations;

namespace Gve.Api.Models;

public class OperationsBindingModel
{
    [Required]
    public IList<CharChainOperationSource> Operations { get; set; } = [];
}
