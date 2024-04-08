using System.ComponentModel.DataAnnotations;

namespace Gve.Api.Models;

public class ParsableTextBindingModel
{
    [Required]
    [MaxLength(5000)]
    public string Text { get; set; } = "";
}
