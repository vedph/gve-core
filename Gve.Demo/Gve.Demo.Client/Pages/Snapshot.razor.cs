using Fusi.Tools.Data;
using Gve.Demo.Client.Components;
using Gve.Migration;
using Gve.Text;
using Microsoft.CodeAnalysis;
using Proteus.Rendering;
using Radzen;
using Radzen.Blazor;
using System.Diagnostics;
using System.Net;
using System.Xml.Linq;

namespace Gve.Demo.Client.Pages;

public partial class Snapshot
{
    private string _text = "ARZDC";
    private string? _selectedChainTag;

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            OnTextChanged(value);
        }
    }

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
    public string? SelectedChainTag
    {
        get => _selectedChainTag;
        set
        {
            _selectedChainTag = value;
            OnSelectedChainTagChanged(value);
        }
    }
    public IList<string> SelectedChainTags { get; set; } = [];
    public IList<CharModel> TextChars { get; set; } = [];
    public bool NoTraceFeatures { get; set; }

    public string? RenderedXml { get; set; }

    public Snapshot()
    {
        OperationText =
            // v1 = ARDC
            "3- [*log=\"delete Z\" reason=\"don't like Z\"]\n" +
            // v2 = AVDC (alpha)
            "2=V [*log=\"replace R with V\" *version^:=alpha]\n" +
            // v3 = ABDC
            "6=B [*log=\"replace V with B\"]\n" +
            // v4 = APDC
            "(v1:) 2=P [*log=\"replace R with P\" *version^:=beta]\n" +
            // v5 = APCD (beta)
            "4<>5\n" +
            // v6 = ABCD (gamma)
            "(v3:) 4<>5 [*log=\"swap D with C\" *version^:=gamma]";
        OnTextChanged(_text);
    }

    private void OnTextChanged(string value)
    {
        List<CharModel> chars = [];

        for (int i = 0; i < value.Length; i++)
        {
            chars.Add(new CharModel
            {
                Id = i + 1,
                Index = i,
                Label = value[i] == ' ' ? "_" : value[i].ToString(),
                Value = value[i],
            });
        }
        TextChars = chars;
    }

    private void OnSelectedChainTagChanged(string? value)
    {
        if (Chain == null || string.IsNullOrEmpty(value)) return;

        TextChars = Chain.GetTagNodes(value).Select((n, i) =>
            new CharModel
            {
                Id = n.Id,
                Index = i,
                Label = n.Data == ' ' ? "_" : n.Data.ToString(),
                Value = n.Data,
            }).ToList();
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
        return [.. text.Select((c, _) =>
            new ChainNode<char>(c, $"{(char.IsWhiteSpace(c) ? '_' : c)}"))];
    }

    private static string GetVersionString(IList<ChainNode<char>> nodes) =>
        new([.. nodes.Select(n => n.Data)]);

    private ChainOperationContext<char> RunOperations()
    {
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", GetNodesFrom(Text));
        context.LastTag = "v0";

        foreach (CharChainOperation op in Operations)
        {
            op.NoTraceFeatures = NoTraceFeatures;
            op.Execute(context);
        }

        ChainTags = context.Chain.GetDistinctTags();
        TaggedTexts = [.. ChainTags.Select((t, i) => new Tuple<string, string>(
            t, GetVersionString(context.Chain.GetTagNodes(t))))];

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

        SelectedChainTags = ChainTags.Count > 1
            ? [ChainTags[0], ChainTags[^1]]
            : ChainTags;
        Chain = context.Chain;
        ChainDump = context.Chain.Dump();

        SelectedChainTag = ChainTags[0];

        StateHasChanged();
        StepsGrid?.Reload();

        return context;
    }

    public void Run()
    {
        if (Operations.Count == 0) return;

        try
        {
            IsBusy = true;
            RunOperations();
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

    public void Export()
    {
        if (Operations.Count == 0) return;

        try
        {
            IsBusy = true;

            // run operations
            ChainOperationContext<char> opContext = RunOperations();

            // create tree builder
            GveTextTreeBuilder builder = new()
            {
                FeatureNames =
                [
                    OperationFeature.F_TRACE_SEG_IN,
                    OperationFeature.F_TRACE_SEG2_IN,
                    OperationFeature.F_TRACE_SEG_OUT,
                    OperationFeature.F_TRACE_SEG2_OUT,
                    OperationFeature.F_TRACE_DELETED
                ],
                TraceFeatureFlattening = true,
                IncludeDeleted = true,
                StagedOnly = true
            };

            // build version trees
            TreeNode<ExportedSegment> multi = builder.Build(opContext, Operations);

            // configure a linear merge filter to build export segments
            LinearMergeTextTreeFilter mergeFilter = new();
            mergeFilter.Configure(new MergeTextTreeFilterOptions
            {
                Features =
                [
                    OperationFeature.F_TRACE_SEG_OUT,
                    OperationFeature.F_TRACE_SEG2_OUT,
                    OperationFeature.F_TRACE_DELETED
                ],
                ValueFilters =
                [
                    new MergeTextTreeFilterRepOption
                    {
                        IsRegex = true,
                        Find = "^([^ ]+).*$",
                        Replace = "$1",
                    }
                ]
            });

            // wrap the filter in a multi-text tree filter and apply it
            CompositeTextTreeFilter filter = new()
            {
                Component = mergeFilter
            };
            multi = filter.Apply(multi, opContext);

            GveRendererContext rendContext = new()
            {
                OpContext = opContext,
                Operations = Operations.ToList()
            };

            // create tree renderer
            GveSabaCompositeTextTreeRenderer composite = new()
            {
                Component = new GveSabaTextTreeRenderer()
            };
            composite.Configure(new GveSabaCompositeTextTreeRendererOptions
            {
                PreferredTag = "$last"
            });

            // render the tree
            XElement? result = composite.Render(multi, rendContext);
            // https://github.com/Guyiming/Highlight.Blazor
            RenderedXml = WebUtility.HtmlEncode(result?.ToString());
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
