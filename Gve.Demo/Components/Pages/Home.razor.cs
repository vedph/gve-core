using Gve.Text;
using Radzen;
using Radzen.Blazor;
using System.Diagnostics;

namespace Gve.Demo.Components.Pages;

public partial class Home
{
    private readonly List<CharChainOperation> _operations = [];

    public string Text { get; set; } = "ARZDC";
    public string OperationText { get; set; }
    public IEnumerable<CharChainOperation> Operations { get; set; } = [];
    public IList<string> Tags { get; set; } = [];
    public string? Error { get; set; }
    public RadzenDataGrid<CharChainOperation>? OperationsGrid { get; set; }

    public Home()
    {
        OperationText = "3- [alpha=1 beta=1] {reason=\"don't like Z\"}\n" +
            "2=V [alpha=2]\n" +
            "@2=B [alpha=3]\n" +
            "2=P [beta=2]\n" +
            "4<>5 [alpha=4 beta=3]";
    }

    public void ParseOperationText()
    {
        if (string.IsNullOrWhiteSpace(OperationText)) return;

        try
        {
            _operations.Clear();

            foreach (string line in OperationText.Split('\n',
                StringSplitOptions.RemoveEmptyEntries))
            {
                CharChainOperation op = CharChainOperation
                    .Parse<CharChainOperation>(line);
                _operations.Add(op);
            }

            Operations = [.._operations];
            StateHasChanged();
            OperationsGrid?.Reload();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
            NotificationService.Notify(NotificationSeverity.Error, "Error",
                ex.Message);
        }
    }

    private static List<ChainNode<char>> GetNodesFrom(string text)
    {
        return text.Select((c, _) =>
            new ChainNode<char>(c, $"{(char.IsWhiteSpace(c) ? '_' : c)}"))
            .ToList();
    }

    public void RunOperations()
    {
        try
        {
            ChainOperationContext<char> context = new();
            context.Chain.AppendRange("", "v0", GetNodesFrom(Text));

            foreach (CharChainOperation op in _operations)
            {
                op.Execute(context);
            }

            Tags = context.Chain.GetDistinctTags();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
            NotificationService.Notify(NotificationSeverity.Error, "Error",
                ex.Message);
        }
    }
}
