using Fusi.Tools.Data;
using Gve.Text;
using Gve.Text.DTO;
using System.Collections.Generic;
using System.Linq;

namespace Gve.Migration.Test;

public class TreeBuilderTest
{
    private static List<ChainNode<char>> GetNodesFrom(string text)
    {
        int n = 1;
        return [.. text.Select(c => new ChainNode<char>(c, new string(c, 1), n++))];
    }

    private static List<CharChainOperation> GetOperations()
    {
        // operations
        const string opText =
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

        List<CharChainOperation> operations = [];
        foreach (string line in opText.Split('\n'))
        {
            CharChainOperation op = CharChainOperation
                .Parse<CharChainOperation>(line);
            operations.Add(op);
        }

        return operations;
    }

    [Fact]
    public void Build_Arzdc()
    {
        // prepare context
        ChainOperationContext<char> context = new();
        context.Chain.Reset(GetNodesFrom("ARZDC"));
        context.LastTag = "v0";

        // execute operations on it
        List<string> tags = [];
        foreach (CharChainOperation op in GetOperations())
        {
            op.Execute(context);
            tags.Add(context.LastTag);
        }

        // build tree
        // v0: ARZDC
        // v1: ARDC
        // v2: AVDC
        // v3: ABDC
        // v4: APDC
        // v5: APCD
        // v6: ABCD
        TreeNode<CharChainNode> tree = TreeBuilder.Build(context, false);

        Assert.NotNull(tree);
        // tree:
        // - A (2.1, v6 v5 v4 v3 v2 v1 v0)
        //   - B (3.1, v6, v3)
        //     - C (4.1, v6)
        //       - D (5.1, v6)
        //     - D (4.2, v3)
        //       - C (5.1, v3)
        //   - P (3.2, v5, v4)
        //     - C (4.1, v5)
        //       - D (5.1, v5)
        //     - D (4.2, v4)
        //       - C (5.1, v4)
        //   - V (3.3, v2)
        //     - D (4.1, v2)
        //       - C (5.1, v2)
        //   - R (3.4, v1, v0)
        //     - D (4.1, v1)
        //       - C (5.1, v1)
        //     - Z (4.2, v0)
        //       - D (5.1, v0)
        //         - C (6.1, v0)

        // root is empty and has child A
        Assert.Single(tree.Children);
        TreeNode<CharChainNode> a = tree.Children[0];
        Assert.Equal('A', a.Data!.Data);
        (int y, int x) = a.GetYX();
        Assert.Equal(2, y);
        Assert.Equal(1, x);

        // A has 4 children: B, P, V, R
        Assert.Equal(4, a.Children.Count);
        TreeNode<CharChainNode> b = a.Children[0];
        Assert.Equal('B', b.Data!.Data);
        TreeNode<CharChainNode> p = a.Children[1];
        Assert.Equal('P', p.Data!.Data);
        TreeNode<CharChainNode> v = a.Children[2];
        Assert.Equal('V', v.Data!.Data);
        TreeNode<CharChainNode> r = a.Children[3];
        Assert.Equal('R', r.Data!.Data);

        // B has children C6, D3
        Assert.Equal(2, b.Children.Count);
        TreeNode<CharChainNode> c6 = b.Children[0];
        Assert.Equal('C', c6.Data!.Data);
        TreeNode<CharChainNode> d3 = b.Children[1];
        Assert.Equal('D', d3.Data!.Data);

        // C6 has child D6 with no children
        Assert.Single(c6.Children);
        TreeNode<CharChainNode> d6 = c6.Children[0];
        Assert.Equal('D', d6.Data!.Data);
        Assert.False(d6.HasChildren);

        // D3 has child C3 with no children
        Assert.Single(d3.Children);
        TreeNode<CharChainNode> c3 = d3.Children[0];
        Assert.Equal('C', c3.Data!.Data);
        Assert.False(c3.HasChildren);

        // P has children C5, D4
        Assert.Equal(2, p.Children.Count);
        TreeNode<CharChainNode> c5 = p.Children[0];
        Assert.Equal('C', c5.Data!.Data);
        TreeNode<CharChainNode> d4 = p.Children[1];
        Assert.Equal('D', d4.Data!.Data);

        // C5 has child D5 with no children
        Assert.Single(c5.Children);
        TreeNode<CharChainNode> d5 = c5.Children[0];
        Assert.Equal('D', d5.Data!.Data);
        Assert.False(d5.HasChildren);

        // D4 has child C4 with no children
        Assert.Single(d4.Children);
        TreeNode<CharChainNode> c4 = d4.Children[0];
        Assert.Equal('C', c4.Data!.Data);
        Assert.False(c4.HasChildren);

        // V2 has child D2
        Assert.Single(v.Children);
        TreeNode<CharChainNode> d2 = v.Children[0];
        Assert.Equal('D', d2.Data!.Data);

        // D2 has child C2 with no children
        Assert.Single(d2.Children);
        TreeNode<CharChainNode> c2 = d2.Children[0];
        Assert.Equal('C', c2.Data!.Data);
        Assert.False(c2.HasChildren);

        // R has children D1, Z0
        Assert.Equal(2, r.Children.Count);
        TreeNode<CharChainNode> d1 = r.Children[0];
        Assert.Equal('D', d1.Data!.Data);
        TreeNode<CharChainNode> z0 = r.Children[1];
        Assert.Equal('Z', z0.Data!.Data);

        // D1 has child C1 with no children
        Assert.Single(d1.Children);
        TreeNode<CharChainNode> c1 = d1.Children[0];
        Assert.Equal('C', c1.Data!.Data);
        Assert.False(c1.HasChildren);

        // Z0 has child D0 with child C0 with no children
        Assert.Single(z0.Children);
        TreeNode<CharChainNode> d0 = z0.Children[0];
        Assert.Equal('D', d0.Data!.Data);
        Assert.Single(d0.Children);
        TreeNode<CharChainNode> c0 = d0.Children[0];
        Assert.Equal('C', c0.Data!.Data);
        Assert.False(c0.HasChildren);
    }

    [Fact]
    public void Build_Arzdc_Binary()
    {
        // prepare context
        ChainOperationContext<char> context = new();
        context.Chain.Reset(GetNodesFrom("ARZDC"));
        context.LastTag = "v0";

        // execute operations on it
        List<string> tags = [];
        foreach (CharChainOperation op in GetOperations())
        {
            op.Execute(context);
            tags.Add(context.LastTag);
        }

        // build tree
        // v0: ARZDC
        // v1: ARDC
        // v2: AVDC
        // v3: ABDC
        // v4: APDC
        // v5: APCD
        // v6: ABCD
        TreeNode<CharChainNode> tree = TreeBuilder.Build(context, true);

        // tree:
        Assert.NotNull(tree);
        // + [1.1] >| 
        //  + [2.1] >| #1=A (_tag=v6, _tag=v5, _tag=v4, _tag=v3, _tag=v2, _tag=v1, _tag=v0)
        //   + [3.1] >| 
        //    + [4.1] >| #7=B (opid:=cc56956ebf, _tag=v6, _tag=v3)
        //     + [5.1] >| 
        //      + [6.1] >| #5=C (_tag=v6)
        //       - [7.1] #4=D (_tag=v6)
        //      + [6.2] >| #4=D (_tag=v3)
        //       - [7.1] #5=C (_tag=v3)
        //    + [4.2] >| 
        //     + [5.1] >| 
        //      + [6.1] >| #8=P (opid:=ead2a22c8b, _tag=v5, _tag=v4)
        //       + [7.1] >| 
        //        + [8.1] >| #5=C (_tag=v5)
        //         - [9.1] #4=D (_tag=v5)
        //        + [8.2] >| #4=D (_tag=v4)
        //         - [9.1] #5=C (_tag=v4)
        //      + [6.2] >| #6=V (opid:=8546a03814, _tag=v2)
        //       + [7.1] >| #4=D (_tag=v2)
        //        - [8.1] #5=C (_tag=v2)
        //     + [5.2] >| #2=R (_tag=v1, _tag=v0)
        //      + [6.1] >| 
        //       + [7.1] >| #4=D (_tag=v1)
        //        - [8.1] #5=C (_tag=v1)
        //       + [7.2] >| #3=Z (_tag=v0)
        //        + [8.1] >| #4=D (_tag=v0)
        //         - [9.1] #5=C (_tag=v0)

        // 1.1 root is empty and has child A
        Assert.Single(tree.Children);

        // 2.1 A
        TreeNode<CharChainNode> a = tree.Children[0];
        Assert.Equal('A', a.Data!.Data);
        (int y, int x) = a.GetYX();
        Assert.Equal(2, y);
        Assert.Equal(1, x);
        // A has all versions
        Assert.True(TreeBuilder.GetTagFeatures(a.Data!).Any(f => f.Value == "v6"));
        Assert.True(TreeBuilder.GetTagFeatures(a.Data!).Any(f => f.Value == "v5"));
        Assert.True(TreeBuilder.GetTagFeatures(a.Data!).Any(f => f.Value == "v4"));
        Assert.True(TreeBuilder.GetTagFeatures(a.Data!).Any(f => f.Value == "v3"));
        Assert.True(TreeBuilder.GetTagFeatures(a.Data!).Any(f => f.Value == "v2"));
        Assert.True(TreeBuilder.GetTagFeatures(a.Data!).Any(f => f.Value == "v1"));
        Assert.True(TreeBuilder.GetTagFeatures(a.Data!).Any(f => f.Value == "v0"));

        // 2.1 A has 1 child x (level 3)
        Assert.Single(a.Children);
        TreeNode<CharChainNode> x3 = a.Children[0];
        Assert.Null(x3.Data);
        Assert.Equal(3, x3.GetY());

        // 3.1 x (level 3) has 2 children: B, x (level 4)
        Assert.Equal(2, x3.Children.Count);
        TreeNode<CharChainNode> b = x3.Children[0];
        Assert.Equal('B', b.Data!.Data);
        TreeNode<CharChainNode> x4 = x3.Children[1];
        Assert.Null(x4.Data);
        Assert.Equal(4, x4.GetY());
        // 4.1 B has v6 v3
        Assert.True(TreeBuilder.GetTagFeatures(b.Data!).Any(f => f.Value == "v6"));
        Assert.True(TreeBuilder.GetTagFeatures(b.Data!).Any(f => f.Value == "v3"));

        // 4.1 B has child x (level 5)
        Assert.Single(b.Children);
        TreeNode<CharChainNode> x5 = b.Children[0];
        Assert.Null(x5.Data);
        Assert.Equal(5, x5.GetY());

        // x (level 5) has children C6, D3
        Assert.Equal(2, x5.Children.Count);
        TreeNode<CharChainNode> c6 = x5.Children[0];
        Assert.Equal('C', c6.Data!.Data);
        TreeNode<CharChainNode> d3 = x5.Children[1];
        Assert.Equal('D', d3.Data!.Data);

        // 6.1 C6 has child D6 with no children
        Assert.Single(c6.Children);
        TreeNode<CharChainNode> d6 = c6.Children[0];
        Assert.Equal('D', d6.Data!.Data);
        Assert.False(d6.HasChildren);
        // C6 has v6
        Assert.True(TreeBuilder.GetTagFeatures(c6.Data!).Any(f => f.Value == "v6"));
        // D6 has v6
        Assert.True(TreeBuilder.GetTagFeatures(d6.Data!).Any(f => f.Value == "v6"));

        // 6.2 D3 has child C3 with no children
        Assert.Single(d3.Children);
        TreeNode<CharChainNode> c3 = d3.Children[0];
        Assert.Equal('C', c3.Data!.Data);
        Assert.False(c3.HasChildren);
        // D3 has v3
        Assert.True(TreeBuilder.GetTagFeatures(d3.Data!).Any(f => f.Value == "v3"));
        // C3 has v3
        Assert.True(TreeBuilder.GetTagFeatures(c3.Data!).Any(f => f.Value == "v3"));

        // 4.2 x (level 4) has 2 children: x (level 5), R
        Assert.Equal(2, x4.Children.Count);
        TreeNode<CharChainNode> x5b = x4.Children[0];
        Assert.Null(x5b.Data);
        Assert.Equal(5, x5b.GetY());
        TreeNode<CharChainNode> r = x4.Children[1];
        Assert.Equal('R', r.Data!.Data);

        // 5.2 R has V1 v0
        Assert.True(TreeBuilder.GetTagFeatures(r.Data!).Any(f => f.Value == "v1"));
        Assert.True(TreeBuilder.GetTagFeatures(r.Data!).Any(f => f.Value == "v0"));

        // R has child x (level 6)
        Assert.Single(r.Children);
        TreeNode<CharChainNode> x6 = r.Children[0];
        Assert.Null(x6.Data);
        Assert.Equal(6, x6.GetY());

        // x (level 6) has children D1 Z0
        Assert.Equal(2, x6.Children.Count);
        TreeNode<CharChainNode> d1 = x6.Children[0];
        Assert.Equal('D', d1.Data!.Data);
        TreeNode<CharChainNode> z0 = x6.Children[1];
        Assert.Equal('Z', z0.Data!.Data);

        // 7.1 D1 has child C1 with no children
        Assert.Single(d1.Children);
        TreeNode<CharChainNode> c1 = d1.Children[0];
        Assert.Equal('C', c1.Data!.Data);
        Assert.False(c1.HasChildren);

        // 7.2 Z0 has child D0 with child C0 with no children
        Assert.Single(z0.Children);
        TreeNode<CharChainNode> d0 = z0.Children[0];
        Assert.Equal('D', d0.Data!.Data);
        Assert.Single(d0.Children);
        TreeNode<CharChainNode> c0 = d0.Children[0];
        Assert.Equal('C', c0.Data!.Data);
        Assert.False(c0.HasChildren);
    }
}
