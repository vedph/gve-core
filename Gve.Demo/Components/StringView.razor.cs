using Microsoft.AspNetCore.Components;

namespace Gve.Demo.Components;

public partial class StringView
{
    [Parameter]
    public string? Value { get; set; }

    public List<CharModel> Chars { get; set; } = [];

    private void BuildChars()
    {
        Chars.Clear();

        if (Value is not null)
        {
            for (int i = 0; i < Value.Length; i++)
            {
                Chars.Add(new CharModel
                {
                    Id = i + 1,
                    Index = i,
                    Label = Value[i] == ' '? "_" : Value[i].ToString(),
                    Value = Value[i],
                });
            }
        }
    }

    /// <summary>
    /// Called when parameters are set.
    /// </summary>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        BuildChars();
    }
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
