using Cadmus.Core.Layers;
using Xunit;

namespace Gve.Cadmus.Test;

public class TokenTextLocationConverterTest
{
    [Fact]
    public void LocationToOrdinal_SingleToken()
    {
        // ph
        const string text = "alpha";
        TokenTextLocation location = TokenTextLocation.Parse("1.1@3x2");
        (int n, int r) = TokenTextLocationConverter.LocationToOrdinal(
            text, location);
        Assert.Equal(3, n);
        Assert.Equal(2, r);
    }

    [Fact]
    public void LocationToOrdinal_MultipleTokens_Whole()
    {
        // beta_gamma        1234567890123456
        const string text = "alpha beta gamma";
        TokenTextLocation location = TokenTextLocation.Parse("1.2-1.3");
        (int n, int r) = TokenTextLocationConverter.LocationToOrdinal(
            text, location);
        Assert.Equal(7, n);
        Assert.Equal(10, r);
    }

    [Fact]
    public void LocationToOrdinal_MultipleTokens_Partial()
    {
        // eta_gam           1234567890123456
        const string text = "alpha beta gamma";
        TokenTextLocation location = TokenTextLocation.Parse("1.2@2x3-1.3@1x3");
        (int n, int r) = TokenTextLocationConverter.LocationToOrdinal(
            text, location);
        Assert.Equal(8, n);
        Assert.Equal(7, r);
    }

    [Fact]
    public void LocationToOrdinal_MultipleLines_WholeTokens()
    {
        // delta|epsilon_zeta
        //                   12345678901 234567890123 456789012345
        const string text = "alpha beta\ngamma delta\nepsilon zeta";
        TokenTextLocation location = TokenTextLocation.Parse("2.2-3.2");
        (int n, int r) = TokenTextLocationConverter.LocationToOrdinal(
            text, location);
        Assert.Equal(18, n);
        Assert.Equal(18, r);
    }

    [Fact]
    public void LocationToOrdinal_MultipleLines_PartialTokens()
    {
        // ta|epsilon_ze     12345678901 234567890123 456789012345
        const string text = "alpha beta\ngamma delta\nepsilon zeta";
        TokenTextLocation location = TokenTextLocation.Parse("2.2@4x2-3.2@1x2");
        (int n, int r) = TokenTextLocationConverter.LocationToOrdinal(
            text, location);
        Assert.Equal(21, n);
        Assert.Equal(13, r);
    }

    [Fact]
    public void OrdinalToLocation_SingleToken_Whole()
    {
        const string text = "alpha";
        TokenTextLocation location =
            TokenTextLocationConverter.OrdinalToLocation(text, 1, 5);
        Assert.Equal(1, location.A.Y);
        Assert.Equal(1, location.A.X);
        Assert.Equal(0, location.A.At);
        Assert.Equal(0, location.A.Run);
        Assert.Null(location.B);
    }

    [Fact]
    public void OrdinalToLocation_SingleToken_Partial()
    {
        const string text = "alpha";
        TokenTextLocation location =
            TokenTextLocationConverter.OrdinalToLocation(text, 3, 2);
        Assert.Equal(1, location.A.Y);
        Assert.Equal(1, location.A.X);
        Assert.Equal(3, location.A.At);
        Assert.Equal(2, location.A.Run);
        Assert.Null(location.B);
    }

    [Fact]
    public void OrdinalToLocation_MultipleTokens_Whole()
    {
        // beta_gamma        1234567890123456
        const string text = "alpha beta gamma";
        TokenTextLocation location =
            TokenTextLocationConverter.OrdinalToLocation(text, 7, 10);
        Assert.Equal(1, location.A.Y);
        Assert.Equal(2, location.A.X);
        Assert.Equal(0, location.A.At);
        Assert.Equal(0, location.A.Run);

        Assert.NotNull(location.B);
        Assert.Equal(1, location.B.Y);
        Assert.Equal(3, location.B.X);
        Assert.Equal(0, location.B.At);
        Assert.Equal(0, location.B.Run);
    }

    [Fact]
    public void OrdinalToLocation_MultipleTokens_Partial()
    {
        // ta_gam           1234567890123456
        const string text = "alpha beta gamma";
        TokenTextLocation location =
            TokenTextLocationConverter.OrdinalToLocation(text, 10, 6);
        Assert.Equal(1, location.A.Y);
        Assert.Equal(2, location.A.X);
        Assert.Equal(3, location.A.At);
        Assert.Equal(2, location.A.Run);

        Assert.NotNull(location.B);
        Assert.Equal(1, location.B.Y);
        Assert.Equal(3, location.B.X);
        Assert.Equal(1, location.B.At);
        Assert.Equal(3, location.B.Run);
    }

    [Fact]
    public void OrdinalToLocation_MultipleLines_WholeTokens()
    {
        // delta|epsilon_zeta
        //                   12345678901 234567890123 456789012345
        const string text = "alpha beta\ngamma delta\nepsilon zeta";
        TokenTextLocation location =
            TokenTextLocationConverter.OrdinalToLocation(text, 18, 18);
        Assert.Equal(2, location.A.Y);
        Assert.Equal(2, location.A.X);
        Assert.Equal(0, location.A.At);
        Assert.Equal(0, location.A.Run);

        Assert.NotNull(location.B);
        Assert.Equal(3, location.B.Y);
        Assert.Equal(2, location.B.X);
        Assert.Equal(0, location.B.At);
        Assert.Equal(0, location.B.Run);
    }

    [Fact]
    public void OrdinalToLocation_MultipleLines_PartialTokens()
    {
        // ta|epsilon_ze
        //                   12345678901 234567890123 456789012345
        const string text = "alpha beta\ngamma delta\nepsilon zeta";
        TokenTextLocation location =
            TokenTextLocationConverter.OrdinalToLocation(text, 21, 13);
        Assert.Equal(2, location.A.Y);
        Assert.Equal(2, location.A.X);
        Assert.Equal(4, location.A.At);
        Assert.Equal(2, location.A.Run);
        
        Assert.NotNull(location.B);
        Assert.Equal(3, location.B.Y);
        Assert.Equal(2, location.B.X);
        Assert.Equal(1, location.B.At);
        Assert.Equal(2, location.B.Run);
    }

}
