using Fusi.Tools.Data;
using Microsoft.AspNetCore.Components;

namespace Gve.Demo.Client.Components;

public partial class StringView
{
    [Parameter]
    public IList<CharModel> Chars { get; set; } = [];
}

public class CharModel
{
    private string _label = "";

    public int Id { get; init; }
    public int Index { get; init; }
    public string Label
    {
        get => _label;
        init => _label = DumpHelper.MapNonPrintables(value) ?? "";
    }
    public char Value { get; init; }

    public override string ToString()
    {
        return $"#{Id} [{Index}] {Label}";
    }
}
