using Xunit;
using System.Collections.Generic;

namespace Gve.Text.Test;

public sealed class OperationFeatureTest
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        VarFeature feature = new("name", "value",
            FeatureSetPolicy.Single);

        Assert.Equal("name", feature.Name);
        Assert.Equal("value", feature.Value);
        Assert.Equal(FeatureSetPolicy.Single, feature.SetPolicy);
    }

    [Fact]
    public void Parse_Name()
    {
        VarFeature feature = VarFeature.Parse("name");

        Assert.False(feature.IsNegated);
        Assert.False(feature.IsGlobal);
        Assert.Equal("name", feature.Name);
        Assert.Equal(FeatureSetPolicy.Multiple, feature.SetPolicy);
        Assert.Null(feature.Value);
    }

    [Fact]
    public void Parse_GlobalName()
    {
        VarFeature feature = VarFeature.Parse("*name");

        Assert.False(feature.IsNegated);
        Assert.True(feature.IsGlobal);
        Assert.Equal("name", feature.Name);
        Assert.Equal(FeatureSetPolicy.Multiple, feature.SetPolicy);
        Assert.Null(feature.Value);
    }

    [Fact]
    public void Parse_NegatedName()
    {
        VarFeature feature = VarFeature.Parse("!name");

        Assert.True(feature.IsNegated);
        Assert.False(feature.IsGlobal);
        Assert.Equal("name", feature.Name);
        Assert.Equal(FeatureSetPolicy.Multiple, feature.SetPolicy);
        Assert.Null(feature.Value);
    }

    [Fact]
    public void Parse_NegatedGlobalName()
    {
        VarFeature feature = VarFeature.Parse("!*name");

        Assert.True(feature.IsNegated);
        Assert.True(feature.IsGlobal);
        Assert.Equal("name", feature.Name);
        Assert.Equal(FeatureSetPolicy.Multiple, feature.SetPolicy);
        Assert.Null(feature.Value);
    }

    [Fact]
    public void Parse_NameEqValue()
    {
        VarFeature feature = VarFeature.Parse("name=value");

        Assert.False(feature.IsNegated);
        Assert.False(feature.IsGlobal);
        Assert.Equal("name", feature.Name);
        Assert.Equal(FeatureSetPolicy.Multiple, feature.SetPolicy);
        Assert.Equal("value", feature.Value);
    }

    [Fact]
    public void Parse_NameEqValueWithColon()
    {
        VarFeature feature = VarFeature.Parse("name:=value");

        Assert.False(feature.IsNegated);
        Assert.False(feature.IsGlobal);
        Assert.Equal("name", feature.Name);
        Assert.Equal(FeatureSetPolicy.Single, feature.SetPolicy);
        Assert.Equal("value", feature.Value);
    }

    [Fact]
    public void Parse_NameEqValueWithEqual()
    {
        VarFeature feature = VarFeature.Parse("name==value");

        Assert.False(feature.IsNegated);
        Assert.False(feature.IsGlobal);
        Assert.Equal("name", feature.Name);
        Assert.Equal(FeatureSetPolicy.SingleFirst, feature.SetPolicy);
        Assert.Equal("value", feature.Value);
    }

    [Fact]
    public void Parse_NameEqValueWithSpaces()
    {
        VarFeature feature = VarFeature.Parse("name=\"value with spaces\"");

        Assert.False(feature.IsNegated);
        Assert.False(feature.IsGlobal);
        Assert.Equal("name", feature.Name);
        Assert.Equal(FeatureSetPolicy.Multiple, feature.SetPolicy);
        Assert.Equal("value with spaces", feature.Value);
    }

    [Fact]
    public void TryParse_ShouldReturnTrueAndSetOutParameter_WhenInputIsValid()
    {
        bool result = VarFeature.TryParse("name=value",
            out VarFeature? feature);

        Assert.True(result);
        Assert.NotNull(feature);
        Assert.Equal("name", feature!.Name);
        Assert.Equal("value", feature.Value);
    }

    [Fact]
    public void TryParse_ShouldReturnFalse_WhenInputIsInvalid()
    {
        bool result = VarFeature.TryParse("", out VarFeature? feature);

        Assert.False(result);
        Assert.Null(feature);
    }

    [Fact]
    public void UpdateFeature_NotExisting_Added()
    {
        VarFeature OperationFeature = new("A", "1");
        List<VarFeature> features = [];

        VarFeature.UpdateFeature(OperationFeature, features, false);

        Assert.Single(features);
        Assert.Equal("A", features[0].Name);
        Assert.Equal("1", features[0].Value);
    }

    [Fact]
    public void UpdateFeature_NotExistingShortLived_Added()
    {
        VarFeature feature = VarFeature.Parse("A^=1");
        List<VarFeature> features = [];

        VarFeature.UpdateFeature(feature, features, false);

        Assert.Single(features);
        Assert.True(features[0].IsShortLived);
        Assert.Equal("A", features[0].Name);
        Assert.Equal("1", features[0].Value);
    }

    [Fact]
    public void UpdateFeature_RemoveNotExisting_Unchanged()
    {
        List<VarFeature> features = [];

        VarFeature.UpdateFeature(VarFeature.Parse("!A"), features,
            false);

        Assert.Empty(features);
    }

    [Fact]
    public void UpdateFeature_RemoveExisting_Removed()
    {
        List<VarFeature> features = [new("A", "1"), new("A", "2")];

        VarFeature.UpdateFeature(
            VarFeature.Parse("!A"), features, false);

        Assert.Empty(features);
    }

    [Fact]
    public void UpdateFeature_ExistingNotUnique_Added()
    {
        List<VarFeature> features = [new("A", "1")];

        VarFeature.UpdateFeature(new("A", "2"), features, false);

        Assert.Equal(2, features.Count);
        Assert.Contains(features, x => x.Name == "A" && x.Value == "1");
        Assert.Contains(features, x => x.Name == "A" && x.Value == "2");
    }

    [Fact]
    public void UpdateFeature_ExistingUnique_Replaced()
    {
        List<VarFeature> features = [new("A", "1"), new("A", "2")];

        VarFeature.UpdateFeature(new("A", "3", FeatureSetPolicy.Single),
            features, false);

        Assert.Single(features);
        Assert.Equal("A", features[0].Name);
        Assert.Equal("3", features[0].Value);
    }

    [Fact]
    public void ToString_ShouldReturnNameAndValue()
    {
        VarFeature OperationFeature = new("name", "value");

        string result = OperationFeature.ToString();

        Assert.Equal("name=value", result);
    }
}
