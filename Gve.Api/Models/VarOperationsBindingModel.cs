using System.ComponentModel.DataAnnotations;

namespace Gve.Api.Models;

public class VarOperationsBindingModel
{
    [Required]
    [MaxLength(5000)]
    public string Text { get; set; } = "";

    [Required]
    [MaxLength(1000)]
    public string OperationsText { get; set; } = "";
}
