using Gve.Text.DTO;
using System.ComponentModel.DataAnnotations;

namespace Gve.Api.Models;

public class TextOperationsBindingModel : OperationsBindingModel
{
    [Required]
    public CharChainNode[] Text { get; set; } = [];
}
