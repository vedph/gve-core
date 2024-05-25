using Xunit;
using System.Collections.Generic;

namespace Gve.Text.Test;

public sealed class OperationFeatureTest
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        OperationFeature feature = new("name", "value",
            FeatureSetPolicy.Single);

        Assert.Equal("name", feature.Name);
        Assert.Equal("value", feature.Value);
        Assert.Equal(FeatureSetPolicy.Single, feature.SetPolicy);
    }

    [Fact]
    public void Parse_Name()
    {
        OperationFeature feature = OperationFeature.Parse("name");

        Assert.False(feature.IsNegated);
        Assert.False(feature.IsGlobal);
        Assert.Equal("name", feature.Name);
        Assert.Equal(FeatureSetPolicy.Multiple, feature.SetPolicy);
        Assert.Null(feature.Value);
    }

    [Fact]
    public void Parse_GlobalName()
    {
        OperationFeature feature = OperationFeature.Parse("*name");

        Assert.False(feature.IsNegated);
        Assert.True(feature.IsGlobal);
        Assert.Equal("name", feature.Name);
        Assert.Equal(FeatureSetPolicy.Multiple, feature.SetPolicy);
        Assert.Null(feature.Value);
    }

    [Fact]
    public void Parse_NegatedName()
    {
        OperationFeature feature = OperationFeature.Parse("!name");

        Assert.True(feature.IsNegated);
        Assert.False(feature.IsGlobal);
        Assert.Equal("name", feature.Name);
        Assert.Equal(FeatureSetPolicy.Multiple, feature.SetPolicy);
        Assert.Null(feature.Value);
    }

    [Fact]
    public void Parse_NegatedGlobalName()
    {
        OperationFeature feature = OperationFeature.Parse("!*name");

        Assert.True(feature.IsNegated);
        Assert.True(feature.IsGlobal);
        Assert.Equal("name", feature.Name);
        Assert.Equal(FeatureSetPolicy.Multiple, feature.SetPolicy);
        Assert.Null(feature.Value);
    }

    [Fact]
    public void Parse_NameEqValue()
    {
        OperationFeature feature = OperationFeature.Parse("name=value");

        Assert.False(feature.IsNegated);
        Assert.False(feature.IsGlobal);
        Assert.Equal("name", feature.Name);
        Assert.Equal(FeatureSetPolicy.Multiple, feature.SetPolicy);
        Assert.Equal("value", feature.Value);
    }

    [Fact]
    public void Parse_NameEqValueWithColon()
    {
        OperationFeature feature = OperationFeature.Parse("name:=value");

        Assert.False(feature.IsNegated);
        Assert.False(feature.IsGlobal);
        Assert.Equal("name", feature.Name);
        Assert.Equal(FeatureSetPolicy.Single, feature.SetPolicy);
        Assert.Equal("value", feature.Value);
    }

    [Fact]
    public void Parse_NameEqValueWithEqual()
    {
        OperationFeature feature = OperationFeature.Parse("name==value");

        Assert.False(feature.IsNegated);
        Assert.False(feature.IsGlobal);
        Assert.Equal("name", feature.Name);
        Assert.Equal(FeatureSetPolicy.SingleFirst, feature.SetPolicy);
        Assert.Equal("value", feature.Value);
    }

    [Fact]
    public void Parse_NameEqValueWithSpaces()
    {
        OperationFeature feature = OperationFeature.Parse("name=\"value with spaces\"");

        Assert.False(feature.IsNegated);
        Assert.False(feature.IsGlobal);
        Assert.Equal("name", feature.Name);
        Assert.Equal(FeatureSetPolicy.Multiple, feature.SetPolicy);
        Assert.Equal("value with spaces", feature.Value);
    }

    [Fact]
    public void TryParse_ShouldReturnTrueAndSetOutParameter_WhenInputIsValid()
    {
        bool result = OperationFeature.TryParse("name=value",
            out OperationFeature? feature);

        Assert.True(result);
        Assert.NotNull(feature);
        Assert.Equal("name", feature!.Name);
        Assert.Equal("value", feature.Value);
    }

    [Fact]
    public void TryParse_ShouldReturnFalse_WhenInputIsInvalid()
    {
        bool result = OperationFeature.TryParse("", out OperationFeature? feature);

        Assert.False(result);
        Assert.Null(feature);
    }

    [Fact]
    public void UpdateFeature_NotExisting_Added()
    {
        OperationFeature OperationFeature = new("A", "1");
        List<OperationFeature> features = [];

        OperationFeature.UpdateFeature(OperationFeature, features, false);

        Assert.Single(features);
        Assert.Equal("A", features[0].Name);
        Assert.Equal("1", features[0].Value);
    }

    [Fact]
    public void UpdateFeature_NotExistingShortLived_Added()
    {
        OperationFeature feature = OperationFeature.Parse("A^=1");
        List<OperationFeature> features = [];

        OperationFeature.UpdateFeature(feature, features, false);

        Assert.Single(features);
        Assert.True(features[0].IsShortLived);
        Assert.Equal("A", features[0].Name);
        Assert.Equal("1", features[0].Value);
    }

    [Fact]
    public void UpdateFeature_RemoveNotExisting_Unchanged()
    {
        List<OperationFeature> features = [];

        OperationFeature.UpdateFeature(OperationFeature.Parse("!A"), features,
            false);

        Assert.Empty(features);
    }

    [Fact]
    public void UpdateFeature_RemoveExisting_Removed()
    {
        List<OperationFeature> features = [new("A", "1"), new("A", "2")];

        OperationFeature.UpdateFeature(
            OperationFeature.Parse("!A"), features, false);

        Assert.Empty(features);
    }

    [Fact]
    public void UpdateFeature_ExistingNotUnique_Added()
    {
        List<OperationFeature> features = [new("A", "1")];

        OperationFeature.UpdateFeature(new("A", "2"), features, false);

        Assert.Equal(2, features.Count);
        Assert.Contains(features, x => x.Name == "A" && x.Value == "1");
        Assert.Contains(features, x => x.Name == "A" && x.Value == "2");
    }

    [Fact]
    public void UpdateFeature_ExistingUnique_Replaced()
    {
        List<OperationFeature> features = [new("A", "1"), new("A", "2")];

        OperationFeature.UpdateFeature(new("A", "3", FeatureSetPolicy.Single),
            features, false);

        Assert.Single(features);
        Assert.Equal("A", features[0].Name);
        Assert.Equal("3", features[0].Value);
    }

    [Fact]
    public void ToString_ShouldReturnNameAndValue()
    {
        OperationFeature OperationFeature = new("name", "value");

        string result = OperationFeature.ToString();

        Assert.Equal("name=value", result);
    }
}
