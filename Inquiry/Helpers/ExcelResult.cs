using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace DcmsMobile.Inquiry.Helpers
{
    /// <summary>
    /// Generates an Excel file which the user is prompted to download
    /// </summary>
    /// <remarks>
    /// Sample Code:
    /// <code>
    ///     if (exportExcel)
    ///     {
    ///         var result = new ExcelResult("BoxPallet_" + model.PalletId);
    ///         result.AddWorkSheet(model.AllBoxes, "Boxes", "List of Boxes on Pallet " + model.PalletId);
    ///         result.AddWorkSheet(model.AllSku, "SKU", "List of SKUs on Pallet " + model.PalletId);
    ///         result.AddWorkSheet(model.PalletHistory, "History", "Audit Entries for Pallet " + model.PalletId);
    ///         return result;
    ///     }
    /// </code>
    /// </remarks>
    public class ExcelResult : ActionResult
    {
        #region Construction
        /// <summary>
        /// Data supplied by the caller for each worksheet
        /// </summary>
        private class ExcelWorkSheet
        {
            /// <summary>
            /// Element type which represents each row
            /// </summary>
            public Type ElementType { get; set; }

            /// <summary>
            /// Rows to display
            /// </summary>
            public IEnumerable Rows { get; set; }

            /// <summary>
            /// The text displayed on the worksheet tab
            /// </summary>
            public string TabName { get; set; }

            /// <summary>
            /// Heading displayed in the first row of the worksheet
            /// </summary>
            public string Heading { get; set; }
        }

        /// <summary>
        /// List of sheets in the excel workbook
        /// </summary>
        private readonly IList<ExcelWorkSheet> _sheets;

        /// <summary>
        /// Name of the excel file
        /// </summary>
        private readonly string _fileName;

        /// <summary>
        /// Constructor supplies the file name
        /// </summary>
        /// <param name="fileName"></param>
        public ExcelResult(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException("fileName");
            }
            _fileName = fileName;
            _sheets = new List<ExcelWorkSheet>(4);
        }

        /// <summary>
        /// Adds a worksheet to the excel workbook. Call is ignored if there are no rows.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rows"></param>
        /// <param name="tabName"></param>
        /// <param name="heading"></param>
        public void AddWorkSheet<T>(IList<T> rows, string tabName, string heading)
        {
            //if (rows == null || rows.Count == 0)
            //{
            //    return;
            //}
            _sheets.Add(new ExcelWorkSheet
            {
                ElementType = typeof(T),
                Rows = rows,
                TabName = tabName,
                Heading = heading
            });
        }
        #endregion

        #region Helpers
        private DataType GetDataType(ModelMetadata meta)
        {
            DataType result;
            if (!string.IsNullOrWhiteSpace(meta.DataTypeName) && Enum.TryParse<DataType>(meta.DataTypeName, out result))
            {
                return result;
            }
            var colType = meta.IsNullableValueType ? Nullable.GetUnderlyingType(meta.ModelType) : meta.ModelType;
            if (colType == typeof(DateTime) || colType == typeof(DateTimeOffset))
            {
                return DataType.Date;
            }

            if (colType == typeof(int) || colType == typeof(long) || colType == typeof(short))
            {
                return DataType.Currency;  // Simply means numeric to us
            }


            return DataType.Text;
        }

        private DateTime? GetDateValue(Type modelType, object model, DataType dataType)
        {
            if (model == null)
            {
                return null;
            }
            switch (dataType)
            {
                case DataType.Date:
                case DataType.DateTime:
                case DataType.Time:
                    // This is possibly a date
                    break;

                default:
                    // Do not treat this as a date since this what the DataTypeAttribute is commanding
                    return null;
            }
            if (modelType == typeof(DateTime))
            {
                return (DateTime)model;
            }
            if (modelType == typeof(DateTime?))
            {
                return ((DateTime?)model).Value;
            }
            if (modelType == typeof(DateTimeOffset))
            {
                return ((DateTimeOffset)model).DateTime;
            }
            if (modelType == typeof(DateTimeOffset?))
            {
                return ((DateTimeOffset?)model).Value.DateTime;
            }
            return null;
        }

        #endregion

        public override void ExecuteResult(ControllerContext context)
        {
            const int ROW_NUMBER_COL_HEADING = 2;

            using (var pkg = new ExcelPackage())
            {
                pkg.Workbook.Properties.Author = context.HttpContext.User.Identity.Name ?? "Anonymous";
                pkg.Workbook.Properties.Comments = "This file was automatically generated by DCMS Connect - Inquiry";
                pkg.Workbook.Properties.Company = "Eclipse Systems";
                var name = Assembly.GetExecutingAssembly().GetName();
                pkg.Workbook.Properties.Status = string.Format("{0} {1}", name.Name, name.Version);

                if (_sheets.Count == 0)
                {
                    throw new NotImplementedException("This should never happen. Did you forget to add sheets?");
                }

                foreach (var sheet in _sheets)
                {
                    var wks = pkg.Workbook.Worksheets.Add(sheet.TabName);
                    // 0 based Index of the last column in this sheet
                    int maxColIndex = -1;
                    foreach (var row in sheet.Rows.Cast<object>().Where(p => p != null).Select((p, i) => new
                    {
                        Index = i,
                        Value = p
                    }))
                    {
                        var columns = ModelMetadataProviders.Current.GetMetadataForType(() => row.Value, sheet.ElementType).Properties
                            .Where(p => p.ShowForDisplay && !p.IsComplexType)
                            .OrderBy(p => p.Order)
                            .Select((p, i) =>
                            {
                                var dtype = GetDataType(p);
                                return new
                                {
                                    Index = i,
                                    ShortDisplayName = p.ShortDisplayName ?? p.PropertyName,
                                    ColumnDataType = dtype,
                                    Model = p.Model,
                                    DateValue = GetDateValue(p.ModelType, p.Model, dtype)
                                };
                            });

                        foreach (var col in columns)
                        {
                            maxColIndex = Math.Max(maxColIndex, col.Index);
                            if (row.Index == 0)
                            {
                                // First Row. Generate headings
                                wks.Cells[ROW_NUMBER_COL_HEADING, col.Index + 1].Value = col.ShortDisplayName;
                                var excelCol = wks.Column(col.Index + 1);
                                switch (col.ColumnDataType)
                                {
                                    case DataType.Date:
                                        excelCol.Style.Numberformat.Format = "m/d/yyyy";
                                        break;

                                    case DataType.DateTime:
                                        excelCol.Style.Numberformat.Format = "m/d/yyyy h:mm";
                                        break;

                                    case DataType.Time:
                                        excelCol.Style.Numberformat.Format = "h:mm";
                                        break;

                                    case DataType.Currency:
                                        excelCol.Style.Numberformat.Format = "#,##0";
                                        break;

                                    case DataType.Text:
                                        excelCol.Style.Numberformat.Format = "@";
                                        break;

                                    case DataType.MultilineText:
                                        excelCol.Style.Numberformat.Format = "@";
                                        excelCol.Style.WrapText = true;
                                        wks.Cells[2, col.Index + 1].Style.WrapText = false;
                                        break;

                                    default:
                                        throw new NotImplementedException();
                                }
                                //rowNumber = ROW_NUMBER_COL_HEADING + 1;
                            }

                            if (col.Model != null)
                            {
                                if (col.DateValue.HasValue)
                                {
                                    try
                                    {
                                        wks.Cells[row.Index + ROW_NUMBER_COL_HEADING + 1, col.Index + 1].Value = col.DateValue.Value.ToOADate();
                                    }
                                    catch (OverflowException)
                                    {
                                        // This happens when the date is too old to be represented as OLE date e.g. 2/1/0013
                                        // In this case, show the string representation
                                        wks.Cells[row.Index + ROW_NUMBER_COL_HEADING + 1, col.Index + 1].Value = col.DateValue.Value.ToString();
                                    }
                                }
                                else
                                {
                                    wks.Cells[row.Index + ROW_NUMBER_COL_HEADING + 1, col.Index + 1].Value = col.Model;
                                }
                            }
                        }
                    }

                    if (maxColIndex == -1)
                    {
                        // Empty worksheet. Row 1 is still the heading
                        wks.Cells[1, 1].Value = sheet.Heading;
                        wks.Cells[1, 1].Style.Font.Bold = true;
                        wks.Cells[ROW_NUMBER_COL_HEADING, 1].Value = "There are no rows in this list";
                        wks.Cells[ROW_NUMBER_COL_HEADING, 1].Style.Font.Italic = true;
                    }
                    else
                    {
                        // Row 2 Column headers must be bold. colNumber now represents the number of columns
                        var header = wks.Cells[2, 1, 2, maxColIndex + 1];
                        header.Style.Font.Bold = true;

                        // Row 1: Worksheet heading
                        header = wks.Cells[1, 1, 1, maxColIndex + 1];
                        header.Merge = true;
                        header.Value = sheet.Heading;
                        header.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        header.Style.Font.Bold = true;
                    }
                    wks.Cells.AutoFitColumns();
                }

                // Render XML file contents instead of the view
                var result = new FileContentResult(pkg.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = string.Format("{0}.xlsx", _fileName)
                };
                result.ExecuteResult(context);
            }
        }
    }
}