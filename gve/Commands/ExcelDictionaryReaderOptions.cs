using System.Collections.Generic;
using System.Text;

namespace Gve.Cli.Commands;

/// <summary>
/// Configuration options for reading Excel data as key-value pairs.
/// </summary>
public class ExcelDictionaryReaderOptions
{
    /// <summary>
    /// Gets or sets the name of the Excel sheet to read from.
    /// </summary>
    public string SheetName { get; set; } = "";

    /// <summary>
    /// Gets or sets the column mappings where key is Excel column (A, B, C, etc.) 
    /// and value is the property name in the resulting dictionary.
    /// </summary>
    public Dictionary<string, string> ColumnMappings { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of columns to check for end markers or empty values.
    /// </summary>
    public IList<string>? EndColumns { get; set; }

    /// <summary>
    /// Gets or sets the marker value that indicates the end of data. 
    /// If null or empty, empty cells in EndColumns will be used as end markers.
    /// </summary>
    public string? EndMarker { get; set; }

    /// <summary>
    /// Gets or sets the starting row number (1-based). Default is 1.
    /// </summary>
    public int StartRow { get; set; } = 1;

    /// <summary>
    /// Gets or sets the maximum number of rows to read. 
    /// When 0 (default), all rows will be read until end condition is met.
    /// </summary>
    public int Limit { get; set; }

    /// <summary>
    /// Returns a string representation of the options for diagnostic purposes.
    /// </summary>
    /// <returns>A formatted string containing all option values.</returns>
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.AppendLine($"SheetName: {SheetName ?? "null"}");
        sb.AppendLine($"StartRow: {StartRow}");
        sb.AppendLine($"Limit: {Limit}");
        sb.AppendLine($"EndMarker: {EndMarker ?? "null"}");

        if (ColumnMappings != null)
        {
            sb.AppendLine("ColumnMappings:");
            foreach (KeyValuePair<string, string> mapping in ColumnMappings)
            {
                sb.AppendLine($"  {mapping.Key} -> {mapping.Value}");
            }
        }
        else
        {
            sb.AppendLine("ColumnMappings: null");
        }

        if (EndColumns != null)
        {
            sb.AppendLine($"EndColumns: [{string.Join(", ", EndColumns)}]");
        }
        else
        {
            sb.AppendLine("EndColumns: null");
        }

        return sb.ToString();
    }
}
