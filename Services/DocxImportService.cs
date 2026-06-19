using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using VoiceBookStudio.Models;
using System.Threading.Tasks;

namespace VoiceBookStudio.Services
{
    /// <summary>
    /// Extracts plain text from a Word .docx file using DocumentFormat.OpenXml.
    /// Paragraph breaks are preserved as newlines.
    /// </summary>
    public class DocxImportService
    {
        /// <summary>
        /// Opens a .docx file and returns its body text as plain paragraphs.
        /// </summary>
        public string ExtractText(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            using var doc = WordprocessingDocument.Open(filePath, isEditable: false);
            var body = doc.MainDocumentPart?.Document?.Body;
            if (body == null) return string.Empty;

            var sb = new StringBuilder();
            foreach (var para in body.Elements<Paragraph>())
            {
                // Preserve blank lines between paragraphs by always appending a newline
                sb.AppendLine(para.InnerText);
            }
            return sb.ToString().Trim();
        }

        /// <summary>
        /// Extracts paragraphs with their style names (if available) from a .docx file.
        /// Returns a list of ParagraphData preserving order.
        /// </summary>
        public List<ParagraphData> ExtractParagraphs(string filePath)
        {
            var list = new List<ParagraphData>();
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            using var doc = WordprocessingDocument.Open(filePath, isEditable: false);
            var body = doc.MainDocumentPart?.Document?.Body;
            if (body == null) return list;

            foreach (var para in body.Elements<Paragraph>())
            {
                string text = para.InnerText ?? string.Empty;
                string? style = null;
                var pPr = para.Elements<ParagraphProperties>().FirstOrDefault();
                if (pPr != null)
                {
                    var pStyle = pPr.ParagraphStyleId?.Val?.Value;
                    if (!string.IsNullOrWhiteSpace(pStyle)) style = pStyle;
                }

                list.Add(new ParagraphData { Text = text, Style = style });
            }

            return list;
        }
    }
}
