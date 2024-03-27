using System.Collections.Generic;
using Xunit;

namespace Gve.Text.Test;

public sealed class ChainOperationContextFeatsTest
{
    [Fact]
    public void UpdateFeatures_Global_Add()
    {
        ChainOperationContextFeats feats = new();
        List<Feature> srcFeats =
        [
            new Feature("*a", "A"),
            new Feature("*u", "U", true),
        ];

        feats.UpdateFeatures<char>(srcFeats);

        Assert.Equal(2, feats.Features.Count);
        Assert.Contains(feats.Features, x => x.Name == "a" && x.Value == "A");
        Assert.Contains(feats.Features, x => x.Name == "u" && x.Value == "U");
    }

    [Fact]
    public void UpdateFeatures_Global_Update()
    {
        ChainOperationContextFeats feats = new();
        List<Feature> srcFeats =
        [
            new Feature("*a", "A"),
            new Feature("*u", "U", true),
        ];
        feats.UpdateFeatures<char>(srcFeats);
        srcFeats.Clear();
        srcFeats.Add(new Feature("*u", "U2", true));

        feats.UpdateFeatures<char>(srcFeats);

        Assert.Equal(2, feats.Features.Count);
        Assert.Contains(feats.Features, x => x.Name == "a" && x.Value == "A");
        Assert.Contains(feats.Features, x => x.Name == "u" && x.Value == "U2");
    }

    [Fact]
    public void UpdateFeatures_Global_Remove()
    {
        ChainOperationContextFeats feats = new();
        List<Feature> srcFeats =
        [
            new Feature("*a", "A"),
            new Feature("*u", "U", true),
        ];
        feats.UpdateFeatures<char>(srcFeats);
        srcFeats.Clear();
        srcFeats.Add(new Feature("*a!", null));

        feats.UpdateFeatures<char>(srcFeats);

        Assert.Single(feats.Features);
        Assert.Contains(feats.Features, x => x.Name == "u" && x.Value == "U");
    }

    [Fact]
    public void UpdateFeatures_Global_Doomed()
    {
        ChainOperationContextFeats feats = new();
        List<Feature> srcFeats =
        [
            new Feature("*a", "A"),
            new Feature("*u^", "U", true),
        ];
        feats.UpdateFeatures<char>(srcFeats);

        feats.UpdateFeatures<char>([]);

        Assert.Single(feats.Features);
        Assert.Contains(feats.Features, x => x.Name == "a" && x.Value == "A");
    }
}
