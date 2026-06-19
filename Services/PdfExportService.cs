using System.Collections.Generic;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VoiceBookStudio.Models;
using BookSectionType = VoiceBookStudio.Models.SectionType;

namespace VoiceBookStudio.Services
{
    /// <summary>
    /// Exports a VoiceBook project to a PDF manuscript file using QuestPDF.
    ///
    /// Layout:
    ///   Page 1  — Title page (book title + author, centred)
    ///   Page 2+ — All sections in document order (front → body → back)
    ///             Each section starts on a new page.
    ///             Chapter titles → bold heading; body text → 12pt, 1.5 line height.
    /// </summary>
    public class PdfExportService
    {
        // Colours
        private const string ColourHeading    = "#1F3864";
        private const string ColourBodyText   = "#111111";
        private const string ColourMuted      = "#888888";
        private const string ColourCopyright  = "#444444";

        public void Export(VoiceBookProject     project,
                           IEnumerable<BookChapter> orderedChapters,
                           string outputPath)
        {
            // Community licence — free for organisations under $1 M revenue.
            QuestPDF.Settings.License = LicenseType.Community;

            var chapters = orderedChapters.ToList();

            // Chapters with SectionType.TitlePage feed the title-page layout;
            // their content (if any) is shown beneath the auto-generated heading.
            var titlePageChapter = chapters.FirstOrDefault(
                c => c.SectionType == BookSectionType.TitlePage);

            var contentChapters = chapters
                .Where(c => c.SectionType != BookSectionType.TitlePage)
                .ToList();

            Document.Create(container =>
            {
                // ============================================================
                // PAGE 1 — Title page
                // ============================================================
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(3, Unit.Centimetre);
                    page.DefaultTextStyle(t => t.FontFamily("Times New Roman"));

                    page.Content()
                        .AlignCenter()
                        .AlignMiddle()
                        .Column(col =>
                        {
                            col.Spacing(20);

                            // Book title
                            col.Item()
                               .Text(project.Title)
                               .Bold()
                               .FontSize(36)
                               .FontColor(ColourHeading);

                            // Author
                            if (!string.IsNullOrWhiteSpace(project.Author))
                            {
                                col.Item()
                                   .Text(project.Author)
                                   .FontSize(20)
                                   .FontColor(ColourMuted);
                            }

                            // Extra content from a TitlePage chapter (series, edition, etc.)
                            if (titlePageChapter != null &&
                                !string.IsNullOrWhiteSpace(titlePageChapter.Content))
                            {
                                col.Item().PaddingTop(20);
                                foreach (string para in SplitContent(titlePageChapter.Content))
                                {
                                    col.Item()
                                       .Text(para)
                                       .FontSize(13)
                                       .FontColor(ColourMuted);
                                }
                            }
                        });
                });

                // ============================================================
                // PAGES 2+ — All other sections
                // ============================================================
                if (contentChapters.Count == 0) return;

                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.MarginHorizontal(3,    Unit.Centimetre);
                    page.MarginVertical(2.5f,   Unit.Centimetre);
                    page.DefaultTextStyle(t => t.FontFamily("Times New Roman").FontSize(12));

                    // Running header: book title
                    page.Header()
                        .PaddingBottom(6)
                        .BorderBottom(0.5f)
                        .BorderColor(ColourMuted)
                        .Text(project.Title)
                        .FontSize(9)
                        .FontColor(ColourMuted)
                        .Italic();

                    // Page numbers
                    page.Footer()
                        .AlignCenter()
                        .PaddingTop(6)
                        .Text(x =>
                        {
                            x.Span("— ").FontSize(9).FontColor(ColourMuted);
                            x.CurrentPageNumber().FontSize(9).FontColor(ColourMuted);
                            x.Span(" —").FontSize(9).FontColor(ColourMuted);
                        });

                    page.Content().Column(col =>
                    {
                        bool firstSection = true;

                        foreach (var chapter in contentChapters)
                        {
                            if (!firstSection)
                                col.Item().PageBreak();

                            firstSection = false;

                            // Section heading
                            float headingSize  = GetHeadingSize(chapter.SectionType);
                            string headingColor = chapter.SectionType == BookSectionType.Copyright
                                ? ColourCopyright
                                : ColourHeading;

                            col.Item()
                               .PaddingBottom(14)
                               .Text(chapter.Title)
                               .Bold()
                               .FontSize(headingSize)
                               .FontColor(headingColor);

                            // Body paragraphs
                            if (!string.IsNullOrWhiteSpace(chapter.Content))
                            {
                                bool centred = IsCentred(chapter.SectionType);

                                foreach (string para in SplitContent(chapter.Content))
                                {
                                    var textEl = col.Item()
                                                    .PaddingBottom(8)
                                                    .Text(para)
                                                    .FontSize(chapter.SectionType == BookSectionType.Copyright ? 10 : 12)
                                                    .LineHeight(1.5f)
                                                    .FontColor(ColourBodyText);

                                    if (centred) textEl.AlignCenter();
                                }
                            }
                        }
                    });
                });
            })
            .GeneratePdf(outputPath);
        }

        // ----------------------------------------------------------------
        // Helpers
        // ----------------------------------------------------------------

        private static float GetHeadingSize(BookSectionType t) => t switch
        {
            BookSectionType.Chapter  => 18f,
            BookSectionType.Prologue or
            BookSectionType.Epilogue => 16f,
            _                        => 14f    // front/back matter
        };

        private static bool IsCentred(BookSectionType t) =>
            t is BookSectionType.Dedication or BookSectionType.Epigraph;

        private static IEnumerable<string> SplitContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                yield break;

            foreach (string block in content.Split(
                new[] { "\r\n\r\n", "\n\n" },
                System.StringSplitOptions.RemoveEmptyEntries))
            {
                string clean = block.Replace("\r\n", " ").Replace('\n', ' ').Trim();
                if (!string.IsNullOrWhiteSpace(clean))
                    yield return clean;
            }
        }
    }
}
