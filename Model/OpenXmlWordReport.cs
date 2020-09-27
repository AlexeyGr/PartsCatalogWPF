using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using PartsCatalog.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartsCatalog.Model
{
    public class OpenXmlWordReport : IReport
	{
		public void CreateAndOpen(string header, string filePath, string exePath, string[][] data)
		{
            if (filePath == null)
                throw new NullReferenceException(nameof(filePath));
            try
            {
                Create(header, filePath, data);
                OpenWith(exePath, filePath);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Create(string header, string filePath, string[][] data)
        {
            using (WordprocessingDocument document =
                WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
            {
                Table table = new Table();

                MainDocumentPart mainPart = document.AddMainDocumentPart();
                mainPart.Document = new Document(new Body());
                mainPart.Document.Body.Append(new Paragraph(new Run(new Text(header))));

                TableProperties tblProp = new TableProperties(
                    new TableBorders(
                        new TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 },
                        new BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 },
                        new LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 },
                        new RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 },
                        new InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 },
                        new InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 }
                    )
                );

                table.AppendChild(tblProp);

                var columnWidth = 100.0 / data.Max(r => r.Length);
                for (int i = 0; i < data.Length; i++)
                {
                    TableRow row = new TableRow();
                    for (int j = 0; j < data[i].Length; j++)
                    {
                        TableCell cell = new TableCell();
                        cell.Append(new TableCellProperties(new TableCellWidth() { Type = TableWidthUnitValues.Pct, Width = $"{columnWidth}" }));
                        cell.Append(new TableCellProperties(new TableCellWidth() { Type = TableWidthUnitValues.Pct, Width = $"{columnWidth}" }));
                        cell.Append(new Paragraph(new Run(new Text(data[i][j]))));
                        row.Append(cell);
                    }
                    table.Append(row);
                }
                document.MainDocumentPart.Document.Body.Append(table);

                document.MainDocumentPart.Document.Save();
            }
        }

        private void OpenWith(string exePath, string filePath)
		{
            try
            {
                Process p = new Process();
                p.StartInfo.FileName = exePath;
                p.StartInfo.Arguments = filePath;
                p.Start();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
