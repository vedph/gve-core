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
        // find start
        while (index > 0 && text[index - 1] != ' ' && text[index - 1] != '\n')
        {
            index--;
        }

        // find end
        int end = index;
        while (end < text.Length && text[end] != ' ' && text[end] != '\n')
        {
            end++;
        }
        return end - index;
    }

    private static int GetTokenIndex(string text, int tokenY, int tokenX)
    {
        if (!text.Contains(' ') && !text.Contains('\n'))
            return tokenY == 1 && tokenX == 1 ? 0 : -1;

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
            int i = GetTokenIndex(text, location.A.Y, location.A.X);
            int r;
            if (location.A.At > 0)
            {
                i += location.A.At - 1;
                r = location.A.Run;
            }
            else
            {
                r = GetTokenLength(text, i);
            }
            return (i + 1, r);
        }
    }

    private static TokenTextLocation SetTextLocationEnd(string text,
        int i, int y, int x, int at, TokenTextLocation location)
    {
        int tokenLength = GetTokenLength(text, i);

        // if in same token, set run if required and exit
        if (location.A.Y == y && location.A.X == x)
        {
            // set run only when token is not fully selected
            if (location.A.At > 1 || at < tokenLength)
            {
                if (location.A.At == 0) location.A.At = 1;
                location.A.Run = (short)(i - (location.A.At - 1));
            }
            return location;
        }

        // otherwise, set B and exit
        location.B = new TokenTextPoint
        {
            Y = y,
            X = x,
        };
        // if token is not fully selected, set at=1 and run
        if (at > 1 || at < tokenLength)
        {
            location.B.At = 1;
            location.B.Run = (short)(i - (location.A.At - 1));
        }

        return location;
    }

    /// <summary>
    /// Convert the specified character-based ordinals and run into a token-based
    /// text location.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="n">The ordinal.</param>
    /// <param name="r">The run.</param>
    /// <returns>The location.</returns>
    /// <exception cref="ArgumentNullException">text</exception>
    /// <exception cref="ArgumentOutOfRangeException">n or r less than 0, or
    /// n + r greater than text length</exception>
    public static TokenTextLocation OrdinalToLocation(string text, int n, int r)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(n, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(r, 1);
        if (n + r - 1 > text.Length)
            throw new ArgumentOutOfRangeException(nameof(r));
        ArgumentNullException.ThrowIfNull(text);

        TokenTextLocation location = new();
        int y = 1, x = 1, at = 0, startIndex = n - 1, endIndex = n + r - 1;

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

            // if we got to selection start, set A and continue
            if (i == startIndex)
            {
                location.A.Y = y;
                location.A.X = x;
                if (at > 1) location.A.At = (short)at;
            }

            // if we got to selection end, set A/B and exit
            if (i == endIndex)
            {
                return SetTextLocationEnd(text, i, y, x, at, location);
            }
        }

        return SetTextLocationEnd(text, text.Length, y, x, at, location);
    }
}
