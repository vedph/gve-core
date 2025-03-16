using System.Collections.Generic;
using Xunit;

namespace Gve.Text.Test;

public sealed class ChainOperationContextFeatsTest
{
    [Fact]
    public void UpdateFeatures_Global_Add()
    {
        ChainOperationContextFeats feats = new();
        List<OperationFeature> srcFeats =
        [
            OperationFeature.Parse("*a=A"),
            OperationFeature.Parse("*u:=U"),
        ];

        feats.UpdateFeatures<char>(srcFeats, "v1");

        Assert.Equal(2, feats.Features.Count);
        Assert.Contains(feats.Features, x => x.Name == "a" && x.Value == "A");
        Assert.Contains(feats.Features, x => x.Name == "u" && x.Value == "U");
    }

    [Fact]
    public void UpdateFeatures_Global_Update()
    {
        ChainOperationContextFeats feats = new();
        List<OperationFeature> srcFeats =
        [
            OperationFeature.Parse("*a=A"),
            OperationFeature.Parse("*u:=U"),
        ];
        feats.UpdateFeatures<char>(srcFeats, "v1");
        srcFeats.Clear();
        srcFeats.Add(OperationFeature.Parse("*u:=U2"));

        feats.UpdateFeatures<char>(srcFeats, "v1");

        Assert.Equal(2, feats.Features.Count);
        Assert.Contains(feats.Features, x => x.Name == "a" && x.Value == "A");
        Assert.Contains(feats.Features, x => x.Name == "u" && x.Value == "U2");
    }

    [Fact]
    public void UpdateFeatures_Global_Remove()
    {
        ChainOperationContextFeats feats = new();
        List<OperationFeature> srcFeats =
        [
            OperationFeature.Parse("*a=A"),
            OperationFeature.Parse("*u:=U"),
        ];
        feats.UpdateFeatures<char>(srcFeats, "v1");
        srcFeats.Clear();
        srcFeats.Add(OperationFeature.Parse("!*a"));

        feats.UpdateFeatures<char>(srcFeats, "v1");

        Assert.Single(feats.Features);
        Assert.Contains(feats.Features, x => x.Name == "u" && x.Value == "U");
    }

    [Fact]
    public void UpdateFeatures_Global_Doomed()
    {
        ChainOperationContextFeats feats = new();
        List<OperationFeature> srcFeats =
        [
            OperationFeature.Parse("*a=A"),
            OperationFeature.Parse("*u^:=U"),
        ];
        feats.UpdateFeatures<char>(srcFeats, "v1");

        feats.UpdateFeatures<char>([], "v1");

        Assert.Single(feats.Features);
        Assert.Contains(feats.Features, x => x.Name == "a" && x.Value == "A");
    }
}
