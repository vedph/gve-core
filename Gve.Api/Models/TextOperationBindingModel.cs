using System.ComponentModel.DataAnnotations;

namespace Gve.Api.Models;

public class TextOperationBindingModel
{
    [Required]
    [MaxLength(50000)]
    public string Text { get; set; } = "";

    [Required]
    public IList<CharChainOperationModel> Operations { get; set; } = [];
}
