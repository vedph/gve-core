using System.ComponentModel.DataAnnotations;

namespace Gve.Api.Models;

public class TextOperationBindingModel
{
    [Required]
    public CharChainNodeModel[] Text { get; set; } = [];

    [Required]
    public IList<CharChainOperationModel> Operations { get; set; } = [];
}
