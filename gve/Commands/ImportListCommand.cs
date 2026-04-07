using Cadmus.Core;
using Cadmus.Core.Config;
using Cadmus.Mongo;
using Gve.Cli.Services;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Gve.Cli.Commands;

internal sealed class ImportListCommand : AsyncCommand<ImportListCommandSettings>
{
    private static void ShowSettings(ImportListCommandSettings settings)
    {
        AnsiConsole.MarkupLine("[green underline]IMPORT LIST[/]");
        AnsiConsole.MarkupLine("Data path: [cyan]{0}[/]", settings.DataPath);
        AnsiConsole.MarkupLine("Config path: [cyan]{0}[/]", settings.ConfigPath);
        AnsiConsole.MarkupLine("Preflight: [cyan]{0}[/]", settings.Preflight);
    }

    private static MongoDictionaryImporterOptions LoadOptions(string path)
    {
        return JsonSerializer.Deserialize<MongoDictionaryImporterOptions>(
            File.ReadAllText(path), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            })!;
    }

    protected override Task<int> ExecuteAsync(CommandContext context,
        ImportListCommandSettings settings, CancellationToken cancel)
    {
        ShowSettings(settings);

        try
        {
            // create reader
            MongoDictionaryImporterOptions options =
                LoadOptions(settings.ConfigPath);

            string cs = ConfigurationService.Configuration
                    .GetConnectionString("default") ??
                    "mongodb://localhost:27017/cadmus-gve";
            options.ConnectionString = cs;

            using ExcelDictionaryReader reader = new(
                settings.DataPath, options.Reader);

            // create Mongo repository
            MongoCadmusRepository repository = new(
                // use an empty part type provider, we won't need it here
                new StandardPartTypeProvider(new TagAttributeToTypeMap()),
                new StandardItemSortKeyBuilder());
            repository.Configure(new MongoCadmusRepositoryOptions
            {
                ConnectionString = cs
            });

            // create importer
            MongoDictionaryImporter importer = new(repository, options);

            Dictionary<string, string>? row;
            while ((row = reader.Read()) != null)
            {
                AnsiConsole.Write(reader.CurrentRowNumber);
                if (!settings.Preflight)
                {
                    IItem item = importer.Import(row);
                    AnsiConsole.WriteLine(item.ToString()!);
                }
                else AnsiConsole.WriteLine();
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

public class ImportListCommandSettings : CommandSettings
{
    [CommandArgument(0, "<DataPath>")]
    [Description("Path to the Excel data file to import")]
    public string DataPath { get; set; } = "";

    [CommandArgument(1, "<ConfigPath>")]
    [Description("Path to the configuration file")]
    public string ConfigPath { get; set; } = "";

    [CommandOption("-p|--preflight")]
    [Description("Perform a preflight check without making changes")]
    public bool Preflight { get; set; }
}
