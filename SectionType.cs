namespace VoiceBookStudio.Models
{
    /// <summary>
    /// The 14 recognised section types for a book manuscript.
    /// Enum values encode natural document order:
    ///   0–99   = Front matter  (before the main text)
    ///   100–199 = Body          (numbered chapters / parts)
    ///   200+   = Back matter   (after the main text)
    /// </summary>
    public enum SectionType
    {
        // --- Front matter ---
        TitlePage        = 0,
        Copyright        = 1,
        Dedication       = 2,
        Epigraph         = 3,
        TableOfContents  = 4,
        Foreword         = 5,
        Preface          = 6,
        Introduction     = 7,
        Prologue         = 8,

        // --- Body ---
        Chapter          = 100,

        // --- Back matter ---
        Epilogue         = 200,
        Afterword        = 201,
        Appendix         = 202,
        AboutTheAuthor   = 203,
    }

    /// <summary>
    /// Display names, group labels, and default sort helpers for SectionType.
    /// </summary>
    public static class SectionTypeHelper
    {
        public static string GetDisplayName(SectionType t) => t switch
        {
            SectionType.TitlePage       => "Title Page",
            SectionType.Copyright       => "Copyright",
            SectionType.Dedication      => "Dedication",
            SectionType.Epigraph        => "Epigraph",
            SectionType.TableOfContents => "Table of Contents",
            SectionType.Foreword        => "Foreword",
            SectionType.Preface         => "Preface",
            SectionType.Introduction    => "Introduction",
            SectionType.Prologue        => "Prologue",
            SectionType.Chapter         => "Chapter",
            SectionType.Epilogue        => "Epilogue",
            SectionType.Afterword       => "Afterword",
            SectionType.Appendix        => "Appendix",
            SectionType.AboutTheAuthor  => "About the Author",
            _                           => t.ToString()
        };

        /// <summary>"Front Matter", "Body", or "Back Matter".</summary>
        public static string GetGroup(SectionType t) => (int)t switch
        {
            < 100  => "Front Matter",
            < 200  => "Body",
            _      => "Back Matter"
        };

        /// <summary>
        /// Sort key used when ordering chapters for display and export.
        /// Primary:   group order  (Front=0, Body=1, Back=2)
        /// Secondary: type order   (natural position within group)
        /// Tertiary:  SortOrder    (user-defined order, used for body chapters)
        /// </summary>
        public static int GetGroupOrder(SectionType t) => (int)t switch
        {
            < 100  => 0,
            < 200  => 1,
            _      => 2
        };

        /// <summary>Default title suggested when adding a new section of this type.</summary>
        public static string GetDefaultTitle(SectionType t) => t switch
        {
            SectionType.TitlePage       => "Title Page",
            SectionType.Copyright       => "Copyright",
            SectionType.Dedication      => "Dedication",
            SectionType.Epigraph        => "Epigraph",
            SectionType.TableOfContents => "Table of Contents",
            SectionType.Foreword        => "Foreword",
            SectionType.Preface         => "Preface",
            SectionType.Introduction    => "Introduction",
            SectionType.Prologue        => "Prologue",
            SectionType.Chapter         => "New Chapter",
            SectionType.Epilogue        => "Epilogue",
            SectionType.Afterword       => "Afterword",
            SectionType.Appendix        => "Appendix",
            SectionType.AboutTheAuthor  => "About the Author",
            _                           => "New Section"
        };

        /// <summary>All 14 types in document order — used to populate pickers.</summary>
        public static SectionType[] AllTypes { get; } =
        [
            SectionType.TitlePage,
            SectionType.Copyright,
            SectionType.Dedication,
            SectionType.Epigraph,
            SectionType.TableOfContents,
            SectionType.Foreword,
            SectionType.Preface,
            SectionType.Introduction,
            SectionType.Prologue,
            SectionType.Chapter,
            SectionType.Epilogue,
            SectionType.Afterword,
            SectionType.Appendix,
            SectionType.AboutTheAuthor,
        ];
    }
}
