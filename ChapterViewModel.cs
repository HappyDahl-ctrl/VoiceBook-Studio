using CommunityToolkit.Mvvm.ComponentModel;
using VoiceBookStudio.Models;

namespace VoiceBookStudio.ViewModels
{
    /// <summary>
    /// Observable wrapper around BookChapter for use in the chapters list.
    /// </summary>
    public partial class ChapterViewModel : ObservableObject
    {
        private readonly BookChapter _chapter;

        public ChapterViewModel(BookChapter chapter)
        {
            _chapter = chapter;
        }

        public BookChapter Model => _chapter;

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _content = string.Empty;

        [ObservableProperty]
        private int _wordCount;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SectionTypeDisplay))]
        [NotifyPropertyChangedFor(nameof(SectionGroup))]
        [NotifyPropertyChangedFor(nameof(ChapterListLabel))]
        private SectionType _sectionType = SectionType.Chapter;

        // ----------------------------------------------------------------
        // Derived display helpers
        // ----------------------------------------------------------------

        /// <summary>"Chapter", "Dedication", "About the Author", etc.</summary>
        public string SectionTypeDisplay => SectionTypeHelper.GetDisplayName(SectionType);

        /// <summary>"Front Matter", "Body", or "Back Matter".</summary>
        public string SectionGroup => SectionTypeHelper.GetGroup(SectionType);

        /// <summary>Label shown in the chapter list: "[Dedication]  My Dedication".</summary>
        public string ChapterListLabel =>
            SectionType == SectionType.Chapter
                ? Title
                : $"[{SectionTypeDisplay}]  {Title}";

        // ----------------------------------------------------------------
        // Sync with model
        // ----------------------------------------------------------------

        public void LoadFromModel()
        {
            Title       = _chapter.Title;
            Content     = _chapter.Content;
            WordCount   = _chapter.WordCount;
            SectionType = _chapter.SectionType;
        }

        public void FlushToModel()
        {
            _chapter.Title       = Title;
            _chapter.Content     = Content;
            _chapter.SectionType = SectionType;
            _chapter.MarkModified();
        }

        public override string ToString() => Title;
    }
}
