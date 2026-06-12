using System.Collections.Generic;
using System.Linq;

namespace Gve.Text.Test;

internal static class TestHelper
{
    public static string GetVersionString(IList<ChainNode<char>> nodes) =>
        new([.. nodes.Select(n => n.Data)]);

    public static List<ChainNode<char>> GetNodesFrom(string text)
    {
        return [.. text.Select((c, _) =>
            new ChainNode<char>(c, $"{(char.IsWhiteSpace(c) ? '_' : c)}"))];
    }

    public static ChainOperationContext<char> GetContext(string text)
    {
        ChainOperationContext<char> context = new();
        context.Chain.Reset(GetNodesFrom(text));
        context.LastTag = "v0";

        // record v0 global features (empty by definition for the base text)
        context.Chain.GlobalFeatures.Set("v0", []);

        // populate each chain node's per-version features
        context.SyncNodeFeaturesToChain();

        return context;
    }
}
