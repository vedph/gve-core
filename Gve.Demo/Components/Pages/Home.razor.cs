using Gve.Text;
using Radzen;
using Radzen.Blazor;
using System.Diagnostics;

namespace Gve.Demo.Components.Pages;

public partial class Home
{
    public string Text { get; set; } = "ARZDC";
    public string OperationText { get; set; }
    public IList<CharChainOperation> Operations { get; } = [];
    public IList<string> Tags { get; set; } = [];
    public string? Error { get; set; }
    public RadzenDataGrid<CharChainOperation>? OperationsGrid { get; set; }
    public bool IsBusy { get; set; }

    public Home()
    {
        OperationText = "3- [alpha=1 beta=1] {reason=\"don't like Z\"}\n" +
            "2=V [alpha=2]\n";
            //"@2=B [alpha=3]\n" +
            //"2=P [beta=2]\n" +
            //"4<>5 [alpha=4 beta=3]";
    }

    public void ParseOperationText()
    {
        if (string.IsNullOrWhiteSpace(OperationText)) return;

        try
        {
            Operations.Clear();

            foreach (string line in OperationText.Split('\n',
                StringSplitOptions.RemoveEmptyEntries))
            {
                CharChainOperation op = CharChainOperation
                    .Parse<CharChainOperation>(line);
                Operations.Add(op);
            }

            StateHasChanged();
            OperationsGrid?.Reload();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
            Error = ex.Message;
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
        if (Operations.Count == 0) return;

        try
        {
            IsBusy = true;
            ChainOperationContext<char> context = new();
            context.Chain.AppendRange("", "v0", GetNodesFrom(Text));
            context.LastTag = "v0";

            foreach (CharChainOperation op in Operations)
            {
                op.Execute(context);
            }

            Tags = context.Chain.GetDistinctTags();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
            Error = ex.Message;
            NotificationService.Notify(NotificationSeverity.Error, "Error",
                ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
