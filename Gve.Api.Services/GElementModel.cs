using Gve.Text;
using System.Collections.Generic;

namespace Gve.Api.Services;

public class GElementModel
{
    public string Id { get; set; } = "";

    public string? ParentId { get; set; }

    public IList<Feature>? Features { get; set; }

    public override string ToString()
    {
        return $"{Id} F={Features?.Count ?? 0}";
    }
}
