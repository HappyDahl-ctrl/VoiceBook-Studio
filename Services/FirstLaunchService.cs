using VoiceBookStudio.Utils;

namespace VoiceBookStudio.Services
{
    /// <summary>
    /// Tracks whether the user has completed the first-launch guided tour.
    /// Persists state through AppSettings so the tour is shown only once.
    /// </summary>
    public sealed class FirstLaunchService
    {
        /// <summary>True when the user has never completed the guided tour.</summary>
        public bool IsFirstLaunch() => !AppSettings.FirstLaunchComplete;

        /// <summary>
        /// Marks the guided tour as completed and persists the state immediately.
        /// Safe to call multiple times.
        /// </summary>
        public void MarkTutorialComplete()
        {
            AppSettings.FirstLaunchComplete = true;
            AppSettings.TutorialCompleted   = true;
            AppSettings.SaveJsonSettings();
            AppSettings.Save();
        }

        /// <summary>
        /// Resets the first-launch state so the tour will be offered again on
        /// the next startup. Intended for developer testing only.
        /// </summary>
        public void ResetTutorial()
        {
            AppSettings.FirstLaunchComplete = false;
            AppSettings.TutorialCompleted   = false;
            AppSettings.SaveJsonSettings();
            AppSettings.Save();
        }
    }
}
