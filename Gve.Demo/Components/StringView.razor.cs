using Microsoft.AspNetCore.Components;

namespace Gve.Demo.Components;

public partial class StringView
{
    [Parameter]
    public IList<CharModel> Chars { get; set; } = [];
}

public class CharModel
{
    public int Id { get; init; }
    public int Index { get; init; }
    public string Label { get; init; } = "";
    public char Value { get; init; }

    public override string ToString()
    {
        return $"#{Id} [{Index}] {Label}";
    }
}
