namespace Gve.Text.Test;

public class ReplaceVarOperationTest
{
    [Fact]
    public void Execute_EmptyString_Nope()
    {
        ReplaceVarOperation op = VarOperation.Parse("1x2:\"x\"");
    }
}