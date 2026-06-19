using System.Windows;

namespace VoiceBookStudio.Services
{
    /// <summary>
    /// Implemented by MainWindow so that TutorialService can move keyboard focus
    /// between the three main panels and obtain UIElement references for UIA
    /// notification events, without creating a hard dependency on MainWindow.
    /// </summary>
    public interface ITutorialPresenter
    {
        /// <summary>UIElement for the chapter list panel — passed to UiaAnnouncer in JAWS mode.</summary>
        UIElement ChapterListElement { get; }

        /// <summary>UIElement for the writing editor panel — passed to UiaAnnouncer in JAWS mode.</summary>
        UIElement EditorElement { get; }

        /// <summary>UIElement for the AI assistant panel — passed to UiaAnnouncer in JAWS mode.</summary>
        UIElement AssistantElement { get; }

        void FocusChapterList();
        void FocusEditor();
        void FocusAssistant();

        /// <summary>
        /// Called when the five-step guided tour ends. The implementor should
        /// deregister tour commands from the voice router and return focus to
        /// the chapter list.
        /// </summary>
        void NotifyTourComplete();
    }
}
