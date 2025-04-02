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
}
