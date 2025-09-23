using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Gve.Cli.Commands;

/// <summary>
/// Reads data from Excel files (XLS/XLSX) row by row and converts each row
/// to key-value pairs using NPOI library.
/// Implements IDisposable to properly manage Excel file resources.
/// </summary>
public sealed class ExcelDictionaryReader : IDisposable
{
    public const string DATA_ROW_NUMBER = "_row-number";

    private readonly string _filePath;
    private readonly ExcelDictionaryReaderOptions _options;
    private IWorkbook _workbook;
    private ISheet _worksheet;
    private int _currentRowIndex;
    private int _rowsRead;
    private bool _disposed;

    /// <summary>
    /// The current row number being processed (1-N).
    /// </summary>
    public int CurrentRowNumber => _currentRowIndex + 1; // 1-based index

    /// <summary>
    /// Converts Excel column letters to zero-based column index.
    /// </summary>
    /// <param name="columnLetter">The Excel column letter (A, B, AA, etc.)</param>
    /// <returns>Zero-based column index</returns>
    /// <exception cref="ArgumentException">Thrown when columnLetter is null,
    /// empty, or contains invalid characters.</exception>
    private static int ConvertColumnLetterToIndex(string columnLetter)
    {
        if (string.IsNullOrWhiteSpace(columnLetter))
        {
            throw new ArgumentException("Column letter cannot be null or empty.",
                nameof(columnLetter));
        }

        columnLetter = columnLetter.ToUpperInvariant();
        int result = 0;

        for (int i = 0; i < columnLetter.Length; i++)
        {
            char c = columnLetter[i];
            if (c < 'A' || c > 'Z')
            {
                throw new ArgumentException($"Invalid column letter: " +
                    $"{columnLetter}. Only letters A-Z are allowed.", 
                    nameof(columnLetter));
            }

            result = result * 26 + (c - 'A' + 1);
        }

        return result - 1; // Convert to zero-based index
    }

    /// <summary>
    /// Creates an appropriate workbook instance based on the file extension.
    /// </summary>
    /// <param name="filePath">The path to the Excel file</param>
    /// <returns>An IWorkbook instance</returns>
    /// <exception cref="NotSupportedException">Thrown when file extension
    /// is not supported.</exception>
    /// <exception cref="IOException">Thrown when file cannot be opened or read.
    /// </exception>
    private static IWorkbook CreateWorkbook(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLowerInvariant();

        try
        {
            FileStream fileStream = new(filePath, FileMode.Open,
                FileAccess.Read, FileShare.Read);

            switch (extension)
            {
                case ".xls":
                    return new HSSFWorkbook(fileStream);
                case ".xlsx":
                    return new XSSFWorkbook(fileStream);
                default:
                    fileStream?.Dispose();
                    throw new NotSupportedException(
                        $"File extension '{extension}' is not supported. " +
                        $"Only .xls and .xlsx files are supported.");
            }
        }
        catch (Exception ex) when (!(ex is NotSupportedException))
        {
            throw new IOException($"Failed to open Excel file '{filePath}': " +
                $"{ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets the specified worksheet from the workbook.
    /// </summary>
    /// <param name="workbook">The workbook to search in</param>
    /// <param name="sheetName">The name of the sheet to find</param>
    /// <returns>The requested worksheet</returns>
    /// <exception cref="ArgumentException">Thrown when sheet with specified 
    /// name is not found.</exception>
    private static ISheet GetWorksheet(IWorkbook workbook, string sheetName)
    {
        ISheet sheet = workbook.GetSheet(sheetName);
        if (sheet == null)
        {
            string availableSheets = string.Join(", ",
                Enumerable.Range(0, workbook.NumberOfSheets)
                .Select(i => $"'{workbook.GetSheetName(i)}'"));
            throw new ArgumentException($"Sheet '{sheetName}' not found. " +
                $"Available sheets: {availableSheets}", nameof(sheetName));
        }
        return sheet;
    }

    /// <summary>
    /// Validates the provided options for reading Excel data.
    /// </summary>
    /// <param name="options">The options to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when options or required
    /// properties are null.</exception>
    /// <exception cref="ArgumentException">Thrown when options contain invalid
    /// values.</exception>
    private static void ValidateOptions(ExcelDictionaryReaderOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options),
                "Options cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(options.SheetName))
        {
            throw new ArgumentException("SheetName cannot be null or empty.",
                nameof(options));
        }

        if (options.ColumnMappings == null || options.ColumnMappings.Count == 0)
        {
            throw new ArgumentException("ColumnMappings cannot be null or empty.",
                nameof(options));
        }

        if (options.StartRow < 1)
        {
            throw new ArgumentException(
                "StartRow must be greater than or equal to 1.", nameof(options));
        }

        if (options.Limit < 0)
        {
            throw new ArgumentException(
                "Limit must be greater than or equal to 0.", nameof(options));
        }

        // validate column letters in ColumnMappings
        foreach (string columnLetter in options.ColumnMappings.Keys)
        {
            try
            {
                ConvertColumnLetterToIndex(columnLetter);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"Invalid column letter in " +
                    $"ColumnMappings: {ex.Message}", nameof(options));
            }
        }

        // validate column letters in EndColumns if provided
        if (options.EndColumns?.Count > 0l)
        {
            foreach (string columnLetter in options.EndColumns)
            {
                try
                {
                    ConvertColumnLetterToIndex(columnLetter);
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException(
                        $"Invalid column letter in EndColumns: {ex.Message}", 
                        nameof(options));
                }
            }
        }
    }

    /// <summary>
    /// Gets the string value from a cell, handling different cell types.
    /// </summary>
    /// <param name="cell">The cell to read from</param>
    /// <returns>The cell value as a string, or empty string if cell is
    /// null</returns>
    private static string GetCellValueAsString(ICell cell)
    {
        if (cell == null) return "";

        switch (cell.CellType)
        {
            case CellType.String: return cell.StringCellValue ?? "";
            case CellType.Numeric:
                if (DateUtil.IsCellDateFormatted(cell))
                    return cell.DateCellValue.ToString() ?? "";
                return cell.NumericCellValue.ToString();
            case CellType.Boolean:
                return cell.BooleanCellValue.ToString();
            case CellType.Formula:
                try
                {
                    return cell.StringCellValue ?? "";
                }
                catch
                {
                    return cell.NumericCellValue.ToString();
                }
            case CellType.Blank:
            case CellType.Unknown:
            default:
                return "";
        }
    }

    /// <summary>
    /// Checks if the current row should be considered the end of data based
    /// on EndColumns and EndMarker.
    /// </summary>
    /// <param name="row">The row to check</param>
    /// <param name="options">The reader options containing end conditions</param>
    /// <returns>True if this row marks the end of data, false otherwise</returns>
    private static bool IsEndOfData(IRow row, ExcelDictionaryReaderOptions options)
    {
        if (options.EndColumns == null || options.EndColumns.Count == 0)
            return false;

        foreach (string columnLetter in options.EndColumns)
        {
            int columnIndex = ConvertColumnLetterToIndex(columnLetter);
            ICell? cell = row?.GetCell(columnIndex);
            if (cell == null)
            {
                if (string.IsNullOrEmpty(options.EndMarker)) return true;
                continue;
            }

            string cellValue = GetCellValueAsString(cell);

            if (string.IsNullOrEmpty(options.EndMarker))
            {
                // if EndMarker is not specified, check for empty cells
                if (string.IsNullOrWhiteSpace(cellValue)) return true;
            }
            else
            {
                // check if cell value matches the EndMarker
                if (string.Equals(cellValue, options.EndMarker,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Initializes a new instance of the ExcelDictionaryReader class.
    /// </summary>
    /// <param name="filePath">The full path to the Excel file</param>
    /// <param name="options">Configuration options specifying how to read 
    /// the data</param>
    /// <exception cref="ArgumentNullException">Thrown when filePath is null.
    /// </exception>
    /// <exception cref="ArgumentException">Thrown when filePath is empty or 
    /// options are invalid.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the specified file 
    /// does not exist.</exception>
    /// <exception cref="IOException">Thrown when the file cannot be read.</exception>
    /// <exception cref="NotSupportedException">Thrown when the file format 
    /// is not supported.</exception>
    public ExcelDictionaryReader(string filePath,
        ExcelDictionaryReaderOptions options)
    {
        if (filePath == null)
        {
            throw new ArgumentNullException(nameof(filePath),
                "File path cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be empty or whitespace.",
                nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                $"Excel file not found: {filePath}", filePath);
        }

        ValidateOptions(options);

        _filePath = filePath;
        _options = options;
        _currentRowIndex = options.StartRow - 1; // convert to zero-based index
        _rowsRead = 0;
        _disposed = false;

        // initialize the workbook and worksheet
        _workbook = CreateWorkbook(_filePath);
        _worksheet = GetWorksheet(_workbook, _options.SheetName);
    }

    /// <summary>
    /// Reads the next available row from the Excel file and returns it as a 
    /// dictionary of key-value pairs.
    /// </summary>
    /// <returns>
    /// A dictionary containing the row data as string key-value pairs, or null 
    /// if no more rows are available or the end condition has been met.
    /// </returns>
    /// <exception cref="ObjectDisposedException">Thrown when the reader has 
    /// been disposed.</exception>
    public Dictionary<string, string>? Read()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ExcelDictionaryReader),
                "Cannot read from disposed ExcelDictionaryReader.");
        }

        // check if we've reached our limit
        if (_options.Limit > 0 && _rowsRead >= _options.Limit) return null;

        // check if we've exceeded the worksheet bounds
        if (_currentRowIndex > _worksheet.LastRowNum) return null;

        IRow row = _worksheet.GetRow(_currentRowIndex);

        // check for end of data condition
        if (IsEndOfData(row, _options)) return null;

        // move to next row for subsequent calls
        _currentRowIndex++;

        // if row is null, try the next row (skip null rows)
        if (row == null)
            return Read(); // recursive call to get the next non-null row

        // read data from the current row
        Dictionary<string, string> result = [];
        foreach (KeyValuePair<string, string> mapping in _options.ColumnMappings)
        {
            int columnIndex = ConvertColumnLetterToIndex(mapping.Key);
            ICell cell = row.GetCell(columnIndex);
            string cellValue = GetCellValueAsString(cell);
            result[mapping.Value] = cellValue;
        }
        result[DATA_ROW_NUMBER] = CurrentRowNumber.ToString();

        _rowsRead++;
        return result;
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _workbook?.Close();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
