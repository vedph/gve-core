using Gve.Text.DTO;
using System.ComponentModel.DataAnnotations;

namespace Gve.Api.Models;

public class TextOperationBindingModel
{
    [Required]
    public CharChainNode[] Text { get; set; } = [];

    [Required]
    public IList<CharChainOperationSource> Operations { get; set; } = [];
}
