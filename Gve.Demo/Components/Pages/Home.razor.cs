using Gve.Text;
using Radzen;
using System.Diagnostics;

namespace Gve.Demo.Components.Pages;

public partial class Home
{
    public string Text { get; set; } = "ARZDC";
    public string OperationText { get; set; }
    public List<CharChainOperation> Operations { get; } = [];

    public Home()
    {
        OperationText = "3- [alpha=1 beta=1] {reason=\"don't like Z\"}\n" +
            "2=V [alpha=2]\n" +
            "@2=B [alpha=3]\n" +
            "2=P [beta=2]\n" +
            "C<>D [alpha=4 beta=3]";
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
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
            NotificationService.Notify(NotificationSeverity.Error, "Error",
                ex.Message);
        }
    }
}
