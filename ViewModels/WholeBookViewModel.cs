using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Text;

namespace VoiceBookStudio.ViewModels
{
    /// <summary>
    /// Sentinel view-model representing the pinned "Whole Book" entry at the top of the
    /// chapter list. Not a chapter entity — never exported, counted, or reordered.
    /// Content is rebuilt on demand by calling Refresh().
    /// </summary>
    public partial class WholeBookViewModel : ObservableObject
    {
        public string ChapterListLabel => "Whole Book";
        public string SectionGroup     => "Full manuscript — read only";

        [ObservableProperty]
        private string _content = string.Empty;

        /// <summary>
        /// Rebuilds Content by concatenating every chapter's text in list order,
        /// preceded by the chapter title as a heading line.
        /// </summary>
        public void Refresh(IEnumerable<ChapterViewModel> chapters)
        {
            var sb = new StringBuilder();
            bool first = true;
            foreach (var c in chapters)
            {
                if (string.IsNullOrEmpty(c.Content)) continue;
                if (!first) sb.Append("\n\n");
                if (!string.IsNullOrWhiteSpace(c.Title))
                {
                    sb.AppendLine(c.Title);
                    sb.AppendLine();
                }
                sb.Append(c.Content);
                first = false;
            }
            Content = sb.ToString();
        }
    }
}
