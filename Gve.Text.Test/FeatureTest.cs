using Xunit;
using System.Collections.Generic;

namespace Gve.Text.Test;

public sealed class FeatureTest
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        Feature feature = new("name", "value", true);

        Assert.Equal("name", feature.Name);
        Assert.Equal("value", feature.Value);
        Assert.True(feature.IsUnique);
    }

    [Fact]
    public void Parse_ShouldReturnFeature()
    {
        Feature feature = Feature.Parse("name=value");

        Assert.Equal("name", feature.Name);
        Assert.Equal("value", feature.Value);
    }

    [Fact]
    public void Parse_ShouldReturnFeature_WhenInputHasNameOnly()
    {
        Feature feature = Feature.Parse("name");

        Assert.Equal("name", feature.Name);
        Assert.Null(feature.Value);
    }

    [Fact]
    public void Parse_ShouldReturnFeature_WhenInputHasNameAndValueWithColon()
    {
        Feature feature = Feature.Parse("name:=value");

        Assert.Equal("name", feature.Name);
        Assert.Equal("value", feature.Value);
    }

    [Fact]
    public void Parse_ShouldReturnFeature_WhenInputHasNameAndValueWithSpaces()
    {
        Feature feature = Feature.Parse("name=\"value with spaces\"");

        Assert.Equal("name", feature.Name);
        Assert.Equal("value with spaces", feature.Value);
    }

    [Fact]
    public void TryParse_ShouldReturnTrueAndSetOutParameter_WhenInputIsValid()
    {
        bool result = Feature.TryParse("name=value", out Feature? feature);

        Assert.True(result);
        Assert.NotNull(feature);
        Assert.Equal("name", feature!.Name);
        Assert.Equal("value", feature.Value);
    }

    [Fact]
    public void TryParse_ShouldReturnFalse_WhenInputIsInvalid()
    {
        bool result = Feature.TryParse("", out Feature? feature);

        Assert.False(result);
        Assert.Null(feature);
    }

    [Fact]
    public void UpdateFeatures_ShouldUpdateTargetList()
    {
        List<Feature> sources = [new("name", "value", true)];
        List<Feature> targets = [];

        Feature.UpdateFeatures(sources, targets);

        Assert.Single(targets);
        Assert.Equal("name", targets[0].Name);
        Assert.Equal("value", targets[0].Value);
    }

    [Fact]
    public void ToString_ShouldReturnNameAndValue()
    {
        Feature feature = new("name", "value");

        string result = feature.ToString();

        Assert.Equal("name=value", result);
    }
}
