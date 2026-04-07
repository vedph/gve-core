using DiffPlex.DiffBuilder.Model;
using DiffPlex.DiffBuilder;
using DiffPlex;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.IO;
using DiffPlex.Chunkers;
using System.Text;
using System.Threading;

namespace Gve.Cli.Commands;

internal sealed class DiffCommand : AsyncCommand<DiffCommandSettings>
{
    private static void ShowSettings(DiffCommandSettings settings)
    {
        AnsiConsole.MarkupLine("[green underline]DIFF[/]");
        AnsiConsole.MarkupLine("A path: [cyan]{0}[/]", settings.APath);
        AnsiConsole.MarkupLine("B path: [cyan]{0}[/]", settings.BPath);
        if (settings.OutputPath is not null)
        {
            AnsiConsole.MarkupLine("Output path: [cyan]{0}[/]",
                settings.OutputPath);
        }
    }

    private static string SanitizeText(string text)
    {
        // replace each character code less than 32 with its ASCII name
        string[] names = [
            "NUL",  // 00
            "SOH",  // 01
            "STX",  // 02
            "ETX",  // 03
            "EOT",  // 04
            "ENQ",  // 05
            "ACK",  // 06
            "BEL",  // 07
            "BS",   // 08
            "HT",   // 09
            "LF",   // 0A
            "VT",   // 0B
            "FF",   // 0C
            "CR",   // 0D
            "SO",   // 0E
            "SI",   // 0F
            "DLE",  // 10
            "DC1",  // 11
            "DC2",  // 12
            "DC3",  // 13
            "DC4",  // 14
            "NAK",  // 15
            "SYN",  // 16
            "ETB",  // 17
            "CAN",  // 18
            "EM",   // 19
            "SUB",  // 1A
            "ESC",  // 1B
            "FS",   // 1C
            "GS",   // 1D
            "RS",   // 1E
            "US"    // 1F
        ];

        StringBuilder sb = new(text);
        for (int i = 0; i < sb.Length; i++)
        {
            if (sb[i] < 32)
            {
                int code = sb[i];
                sb.Remove(i, 1);
                sb.Insert(i, '<');
                sb.Insert(i + 1, names[code]);
                sb.Insert(i + 1 + names[code].Length, '>');
            }
        }
        return sb.ToString();
    }

    private static string LFToUnix(string text)
    {
        StringBuilder sb = new(text);

        // replace CR LF (Windows) with LF
        sb.Replace("\r\n", "\n");

        // replace CR (old Mac OS) with LF
        sb.Replace("\r", "\n");

        return sb.ToString();
    }

    private static void WriteResult(DiffPaneModel model, StreamWriter writer)
    {
        foreach (DiffPiece? line in model.Lines)
        {
            if (line.Position.HasValue) writer.Write(line.Position.Value);
            writer.Write('\t');
            switch (line.Type)
            {
                case ChangeType.Inserted:
                    writer.Write("+ ");
                    break;
                case ChangeType.Deleted:
                    writer.Write("- ");
                    break;
                default:
                    writer.Write("  ");
                    break;
            }

            writer.WriteLine(SanitizeText(line.Text));
        }
    }

    protected override Task<int> ExecuteAsync(CommandContext context,
        DiffCommandSettings settings, CancellationToken cancel)
    {
        ShowSettings(settings);

        try
        {
            InlineDiffBuilder diffBuilder = new(new Differ());
            DiffPaneModel diff = diffBuilder.BuildDiffModel(
                LFToUnix(File.ReadAllText(settings.APath)),
                LFToUnix(File.ReadAllText(settings.BPath)),
                true,
                false,
                new WordChunker());

            foreach (DiffPiece? line in diff.Lines)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                if (line.Position.HasValue) Console.Write(line.Position.Value);
                Console.Write('\t');
                switch (line.Type)
                {
                    case ChangeType.Inserted:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("+ ");
                        break;
                    case ChangeType.Deleted:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("- ");
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("  ");
                        break;
                }

                Console.WriteLine(SanitizeText(line.Text));
            }

            if (settings.OutputPath != null)
            {
                using StreamWriter writer = new(settings.OutputPath,
                    false, Encoding.UTF8);
                WriteResult(diff, writer);
            }

            return Task.FromResult(0);
        }
        catch (Exception ex)
        {
            CliHelper.DisplayException(ex);
            return Task.FromResult(2);
        }
    }
}

public class DiffCommandSettings : CommandSettings
{
    [CommandArgument(0, "<A_PATH>")]
    [Description("The path to the A file")]
    public required string APath { get; set; }

    [CommandArgument(1, "<B_PATH>")]
    [Description("The path to the B file")]
    public required string BPath { get; set; }

    [CommandOption("-o|--output <OUT_PATH>")]
    [Description("The output file path")]
    public string? OutputPath { get; set; }
}