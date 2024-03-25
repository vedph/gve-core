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
    public IList<Tuple<string, string>> TaggedTexts { get; set; } = [];
    public IList<ChainOperationContextStep> Steps { get; set; } = [];
    public IList<Tuple<string,int>> ChainStats { get; set;} = [];
    public string? Error { get; set; }
    public RadzenDataGrid<CharChainOperation>? OperationsGrid { get; set; }
    public RadzenDataGrid<ChainOperationContextStep>? StepsGrid { get; set; }
    public bool IsBusy { get; set; }
    public Chain<char>? Chain { get; set; }
    public string? ChainDump { get; set; }
    public IList<string> ChainTags { get; set; } = [];
    public IList<string> SelectedChainTags { get; set; } = [];

    public Home()
    {
        OperationText =
            // v1 = ARDC
            "3- [*log=\"delete Z\"]\n" +
            // v2 = AVDC (alpha)
            "2=V [*log=\"replace R with V\" *version:=alpha]\n" +
            // v3 = ABDC
            "6=B [*log=\"replace V with B\"]\n" +
            // v4 = APDC
            "(v1:) 2=P [*log=\"replace R with P\" *version:=beta]\n" +
            // v5 = APCD (beta
            "4<>5 [!*version]\n" +
            // v6 = ABCD (gamma)
            "(v3:) 4<>5 [*version:=gamma]";
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

    private static string GetVersionString(IList<ChainNode<char>> nodes) =>
        new(nodes.Select(n => n.Data).ToArray());

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

            IList<string> tags = context.Chain.GetDistinctTags();
            TaggedTexts = tags.Select((t, i) => new Tuple<string, string>(
                t, GetVersionString(context.Chain.GetTagNodes(t)))).ToList();

            Steps.Clear();
            foreach (ChainOperationContextStep step in context.Steps)
            {
                Steps.Add(step);
            }

            ChainStats.Clear();
            ChainStats.Add(Tuple.Create("nodes", context.Chain.Nodes.Count));
            // get the counts of links grouped by their Tag
            var linkCounts = context.Chain.Links.GroupBy(l => l.Tag)
                .Select(g => Tuple.Create(g.Key, g.Count()));
            foreach (var linkCount in linkCounts)
            {
                ChainStats.Add(linkCount);
            }

            ChainTags = tags;
            SelectedChainTags = tags.Count > 1? [tags[0], tags[^1]] : tags;
            Chain = context.Chain;
            ChainDump = context.Chain.Dump();

            StateHasChanged();
            StepsGrid?.Reload();
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
