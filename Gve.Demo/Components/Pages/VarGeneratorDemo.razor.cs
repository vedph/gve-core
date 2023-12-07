namespace Gve.Demo.Components.Pages;

public partial class VarGeneratorDemo
{
    public string Text { get; set; }

    public VarGeneratorDemo()
    {
        Text = "ARZDC";
    }

    public void ParseText()
    {
        Text = Text.Trim();
    }
}
