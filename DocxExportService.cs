using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using VoiceBookStudio.Models;
using BookSectionType = VoiceBookStudio.Models.SectionType;

namespace VoiceBookStudio.Services
{
    /// <summary>
    /// Exports a VoiceBook project to a .docx manuscript file.
    ///
    /// Structure produced:
    ///   • Front matter (Title Page, Copyright, Dedication, etc.)
    ///   • Body chapters
    ///   • Back matter (Epilogue, Afterword, Appendix, About the Author)
    ///
    /// Each section begins on a new page. Chapter titles are styled as Heading 1
    /// so Word's built-in navigation pane and screen readers can jump between them.
    /// </summary>
    public class DocxExportService
    {
        public void Export(VoiceBookProject project,
                           IEnumerable<BookChapter> orderedChapters,
                           string outputPath)
        {
            using var doc = WordprocessingDocument.Create(
                outputPath, WordprocessingDocumentType.Document);

            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            // Add a style definitions part so Heading 1 / Title styles resolve.
            AddStyles(mainPart);

            var chapters = orderedChapters.ToList();
            bool first   = true;

            foreach (var chapter in chapters)
            {
                if (!first)
                    AppendPageBreak(body);

                first = false;

                AppendChapter(body, chapter, project);
            }

            // Required: document must end with SectionProperties or Word will complain.
            body.AppendChild(new Paragraph(new ParagraphProperties(
                new SectionProperties())));

            mainPart.Document.Save();
        }

        // ----------------------------------------------------------------
        // Per-chapter rendering
        // ----------------------------------------------------------------

        private static void AppendChapter(Body body, BookChapter chapter, VoiceBookProject project)
        {
            switch (chapter.SectionType)
            {
                case BookSectionType.TitlePage:
                    AppendTitlePage(body, project, chapter);
                    break;

                case BookSectionType.Copyright:
                    AppendSimpleSection(body, chapter, "CopyrightText", centered: false);
                    break;

                case BookSectionType.Dedication:
                case BookSectionType.Epigraph:
                    AppendSimpleSection(body, chapter, styleName: null, centered: true);
                    break;

                default:
                    AppendStandardSection(body, chapter);
                    break;
            }
        }

        private static void AppendTitlePage(Body body, VoiceBookProject project, BookChapter chapter)
        {
            // Book title
            body.AppendChild(StyledParagraph(project.Title, "Title", centered: true));

            // Author
            if (!string.IsNullOrWhiteSpace(project.Author))
                body.AppendChild(StyledParagraph(project.Author, "Subtitle", centered: true));

            // Any extra content in the TitlePage chapter (e.g. series info)
            if (!string.IsNullOrWhiteSpace(chapter.Content))
            {
                foreach (string para in SplitContent(chapter.Content))
                    body.AppendChild(StyledParagraph(para, "Normal", centered: true));
            }
        }

        private static void AppendSimpleSection(Body body, BookChapter chapter,
                                                 string? styleName, bool centered)
        {
            // Section heading (if the title is meaningful, not just the type name)
            body.AppendChild(StyledParagraph(chapter.Title, "Heading1", centered: false));

            foreach (string para in SplitContent(chapter.Content))
                body.AppendChild(StyledParagraph(para, styleName ?? "Normal", centered));
        }

        private static void AppendStandardSection(Body body, BookChapter chapter)
        {
            // Heading 1 = chapter title (navigable in Word, announced by JAWS)
            body.AppendChild(StyledParagraph(chapter.Title, "Heading1", centered: false));

            foreach (string para in SplitContent(chapter.Content))
                body.AppendChild(NormalParagraph(para));
        }

        // ----------------------------------------------------------------
        // Paragraph factories
        // ----------------------------------------------------------------

        private static Paragraph StyledParagraph(string text, string styleId, bool centered)
        {
            var props = new ParagraphProperties(
                new ParagraphStyleId { Val = styleId });

            if (centered)
                props.AppendChild(new Justification { Val = JustificationValues.Center });

            var para = new Paragraph(props);
            para.AppendChild(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
            return para;
        }

        private static Paragraph NormalParagraph(string text)
        {
            return new Paragraph(
                new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
        }

        private static void AppendPageBreak(Body body)
        {
            body.AppendChild(new Paragraph(
                new Run(new Break { Type = BreakValues.Page })));
        }

        // ----------------------------------------------------------------
        // Content splitting
        // ----------------------------------------------------------------

        /// <summary>
        /// Split chapter content into paragraphs on blank lines.
        /// Single newlines are treated as line breaks within the same paragraph.
        /// </summary>
        private static IEnumerable<string> SplitContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                yield break;

            // Split on double newline (blank line = paragraph boundary)
            string[] blocks = content.Split(
                new[] { "\r\n\r\n", "\n\n" },
                StringSplitOptions.RemoveEmptyEntries);

            foreach (string block in blocks)
            {
                string clean = block.Replace("\r\n", " ").Replace('\n', ' ').Trim();
                if (!string.IsNullOrWhiteSpace(clean))
                    yield return clean;
            }
        }

        // ----------------------------------------------------------------
        // Minimal style definitions
        // ----------------------------------------------------------------

        private static void AddStyles(MainDocumentPart mainPart)
        {
            var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
            stylesPart.Styles = new Styles();

            stylesPart.Styles.AppendChild(BuildStyle("Normal",     "Normal",     false, "000000", 24));
            stylesPart.Styles.AppendChild(BuildStyle("Heading1",   "Heading 1",  true,  "1F3864", 32));
            stylesPart.Styles.AppendChild(BuildStyle("Title",      "Title",      true,  "1F3864", 52));
            stylesPart.Styles.AppendChild(BuildStyle("Subtitle",   "Subtitle",   false, "404040", 28));
            stylesPart.Styles.AppendChild(BuildStyle("CopyrightText", "Copyright Text", false, "666666", 20));

            stylesPart.Styles.Save();
        }

        private static Style BuildStyle(string styleId, string styleName,
                                         bool bold, string hexColor, int halfPointSize)
        {
            var style = new Style
            {
                Type    = StyleValues.Paragraph,
                StyleId = styleId
            };
            style.AppendChild(new StyleName { Val = styleName });

            var rpr = new StyleRunProperties();
            if (bold) rpr.AppendChild(new Bold());
            rpr.AppendChild(new Color { Val = hexColor });
            rpr.AppendChild(new FontSize { Val = halfPointSize.ToString() });
            style.AppendChild(rpr);

            return style;
        }
    }
}
