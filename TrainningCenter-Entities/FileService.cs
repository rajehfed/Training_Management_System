using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace TrainningCenter_Entities
{
    public class ExportConfig<T> where T : class
    {
        public string SheetName { get; set; } = "Reports";
        public Dictionary<string, Func<T, object>> ColumnMappings { get; set; }
        public IEnumerable<string> ExcludeProperties { get; set; } = new List<string>();
    }
    public class FileService
    {
        public FileService()
        {
            // Ensure the license is set before any PDF generation happens
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        }
        #region EXCEL OPERATIONS

        public void ExportToExcel<T>(IEnumerable<T> data, string filePath, ExportConfig<T> config = null) where T : class
        {
            if (data == null || string.IsNullOrEmpty(filePath)) return;
            if (config == null)
            {
                config = new ExportConfig<T>();
            }

            try
            {
                EnsureDirectoryExists(filePath);
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add(config.SheetName);

                    // 1. Prepare Property List (Optimized: Get outside the loop)
                    var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => !config.ExcludeProperties.Contains(p.Name))
                        .ToList();

                    // 2. Write Headers
                    for (int i = 0; i < properties.Count; i++)
                    {
                        var cell = worksheet.Cell(1, i + 1);
                        cell.Value = properties[i].Name;
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    }

                    // 3. Write Data Rows
                    int currentRow = 2;
                    foreach (var item in data)
                    {
                        for (int i = 0; i < properties.Count; i++)
                        {
                            var prop = properties[i];
                            object value = config.ColumnMappings != null && config.ColumnMappings.ContainsKey(prop.Name)
                            ? config.ColumnMappings[prop.Name](item)
                            : prop.GetValue(item);

                            worksheet.Cell(currentRow, i + 1).Value = value?.ToString() ?? string.Empty;
                        }

                        currentRow++;
                    }

                    worksheet.Columns().AdjustToContents();
                    workbook.SaveAs(filePath);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public IEnumerable<T> ImportFromExcel<T>(string filePath, Func<IXLRangeRow, T> customMapper = null) where T : class, new()
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);

            var resultList = new List<T>();
            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

                    var properties = typeof(T).GetProperties().Where(p => p.CanWrite).ToList();

                    foreach (var row in rows)
                    {
                        if (customMapper != null)
                        {
                            resultList.Add(customMapper(row));
                            continue;
                        }

                        var item = new T();
                        for (int i = 0; i < properties.Count && i < row.CellCount(); i++)
                        {
                            var cellValue = row.Cell(i + 1).GetValue<string>();
                            var prop = properties[i];

                            if (!string.IsNullOrWhiteSpace(cellValue))
                            {
                                var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                                var convertedValue = Convert.ChangeType(cellValue, targetType);
                                prop.SetValue(item, convertedValue);
                            }
                        }
                        resultList.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return resultList;
        }

        #endregion

        #region PDF OPERATIONS

        public void ExportToPdf<T>(IEnumerable<T> data, string title, string filePath, List<string> selectedProperties = null) where T : class
        {
            try
            {
                EnsureDirectoryExists(filePath);

                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => selectedProperties == null
                        ? (p.Name == "FullName" || p.Name == "Email" || p.Name == "PhoneNumber" || p.Name == "Status")
                        : selectedProperties.Contains(p.Name))
                    .ToList();

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        // Switch to Landscape if there are many columns
                        page.Size(properties.Count > 5 ? PageSizes.A4.Landscape() : PageSizes.A4);
                        page.Margin(1, Unit.Centimetre);

                        // Header
                        page.Header().Row(row =>
                        {
                            row.RelativeItem().Text(title).FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                            row.RelativeItem().AlignRight().Text(DateTime.Now.ToString("g")).FontSize(10);
                        });

                        // Table Content
                        page.Content().PaddingVertical(10).Table(table =>
                        {
                            // Corrected Column Definition
                            table.ColumnsDefinition(columns =>
                            {
                                foreach (var prop in properties)
                                {
                                    // Give Name and Email more space than ID or Status
                                    if (prop.Name.Contains("Name") || prop.Name.Contains("Email"))
                                        columns.RelativeColumn(3);
                                    else
                                        columns.RelativeColumn(1);
                                }
                            });

                            // Table Header Row
                            table.Header(header =>
                            {
                                foreach (var prop in properties)
                                    header.Cell().Element(HeaderStyle).Text(prop.Name);
                            });

                            // Table Data Rows
                            foreach (var item in data)
                            {
                                foreach (var prop in properties)
                                {
                                    var val = prop.GetValue(item)?.ToString() ?? "";
                                    // Use Clamp to prevent one long string from breaking the whole row
                                    table.Cell().Element(CellStyle).Text(val).FontSize(9);
                                }
                            }
                        });

                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                    });
                }).GeneratePdf(filePath);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion

        #region (Helpers)

        private void EnsureDirectoryExists(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        private static IContainer HeaderStyle(IContainer container) =>
            container.DefaultTextStyle(x => x.SemiBold())
                     .PaddingVertical(5)
                     .BorderBottom(1)
                     .BorderColor(Colors.Black);

        private static IContainer CellStyle(IContainer container) =>
            container.PaddingVertical(5)
                     .BorderBottom(1)
                     .BorderColor(Colors.Grey.Lighten2);

        #endregion
    }
}
