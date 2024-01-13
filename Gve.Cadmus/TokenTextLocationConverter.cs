using Cadmus.Core.Layers;
using System;

namespace Gve.Cadmus;

/// <summary>
/// Converter between token text locations and ordinals with runs. This is used
/// to adapt Cadmus coordinates to GVE coordinates, and vice versa.
/// </summary>
/// <remarks>The converter assumes that whitespaces in text are normalized into
/// a single space and newlines are represented by a single LF character.
/// </remarks>
public static class TokenTextLocationConverter
{
    private static int GetTokenLength(string text, int index)
    {
        int i = index;
        while (i < text.Length && text[i] != ' ' && text[i] != '\n')
        {
            i++;
        }
        return i - index;
    }

    private static int GetTokenIndex(string text, int tokenY, int tokenX)
    {
        int y = 1, x = 1;

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                y++;
                x = 1;
            }
            else if (text[i] == ' ')
            {
                x++;
            }
            if (y == tokenY && x == tokenX)
            {
                // skip the whitespace
                return i + 1;
            }
        }
        return -1;
    }

    /// <summary>
    /// Convert the specified token-based text location into ordinal and run.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="location">The location.</param>
    /// <returns>Character-based ordinal and run.</returns>
    /// <exception cref="ArgumentNullException">text or location</exception>
    public static (int n, int r) LocationToOrdinal(string text,
        TokenTextLocation location)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(location);

        if (location.IsRange)
        {
            int start = GetTokenIndex(text, location.A.Y, location.A.X);
            if (location.A.At > 0) start += location.A.At - 1;

            int end = GetTokenIndex(text, location.B!.Y, location.B.X);

            // being a range, B.At can be only 1 or 0
            if (location.B.At == 1)
            {
                end += location.B.Run;
            }
            else
            {
                end += GetTokenLength(text, end);
            }

            return (start + 1, end - start);
        }
        else
        {
            int i = (GetTokenIndex(text, location.A.Y, location.A.X));
            int r;
            if (location.A.At > 0)
            {
                i += location.A.At - 1;
                r = location.A.Run;
            }
            else
            {
                r = GetTokenLength(text, i - 1);
            }
            return (i + 1, r);
        }
    }

    public static TokenTextLocation OrdinalToLocation(string text, int n, int r)
    {
        ArgumentNullException.ThrowIfNull(text);

        TokenTextLocation location = new()
        {
            A = new TokenTextPoint()
        };
        int y = 1, x = 1, at = 0;

        for (int i = 0; i < text.Length; i++)
        {
            // newline
            if (text[i] == '\n')
            {
                y++;
                x = 1;
                at = 0;
            }
            // space
            else if (text[i] == ' ')
            {
                x++;
                at = 0;
            }
            else
            {
                at++;
            }
            if (i == n - 1)
            {
                location.A.Y = y;
                location.A.X = x;
                location.A.At = (short)at;
            }
            if (i == n + r - 1)
            {
                location.B = new TokenTextPoint
                {
                    Y = y,
                    X = x,
                    At = (short)at,
                    Run = 0
                };
                break;
            }
        }
        location.A.Run = (short)(location.B!.At - location.A.At);
        return location;
    }
}
