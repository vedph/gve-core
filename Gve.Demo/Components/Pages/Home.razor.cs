using Gve.Text;
using System.Diagnostics;

namespace Gve.Demo.Components.Pages;

public partial class Home
{
    public string Text { get; set; } = "ARZDC";
    public string OperationText { get; set; } = "3- {reason=\"don't like Z\"}";
    public List<CharChainOperation> Operations { get; } = [];

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
            //NotificationService.Notify(NotificationSeverity.Error, "Error",
            //    ex.Message);
        }
    }
}
