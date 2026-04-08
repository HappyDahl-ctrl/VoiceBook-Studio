using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using VoiceBookStudio.Models;
using VoiceBookStudio.Services;
using VoiceBookStudio.Utils;

namespace VoiceBookStudio.ViewModels
{
    public class ProjectSelectionViewModel
    {
        private readonly ProjectService _projectService;
        private readonly SystemAnnouncementService _announcer;

        public ObservableCollection<ProjectInfo> RecentProjects { get; } = new();

        public ProjectInfo? SelectedProject { get; set; }

        public ICommand OpenSelectedCommand => new RelayCommand(OpenSelected, () => SelectedProject != null);
        public ICommand CreateNewCommand => new RelayCommand(CreateNew);
        public ICommand BrowseCommand => new RelayCommand(Browse);

        public ProjectSelectionViewModel(ProjectService projectService, SystemAnnouncementService announcer)
        {
            _projectService = projectService;
            _announcer = announcer;

            LoadRecentProjects();
        }

        public void SetOwner(MainViewModel owner)
        {
            // Keep a weak link back to main VM if needed
        }

        private void LoadRecentProjects()
        {
            var folder = AppSettings.ProjectsFolder;
            var list = _projectService.GetRecentProjects(folder);
            RecentProjects.Clear();
            foreach (var p in list) RecentProjects.Add(p);

            // Announce the projects
            if (RecentProjects.Count == 0)
            {
                _announcer.Speak("No recent projects found. You can create a new project or browse for one.");
            }
            else
            {
                foreach (var p in RecentProjects)
                {
                    _announcer.Speak($"{p.Name}, last modified {p.LastModified:MMMM d}");
                }
            }
        }

        private void OpenSelected()
        {
            if (SelectedProject == null) return;
            // Set a flag somewhere or open directly - the caller (App) will read SelectedProject.Path
        }

        private void CreateNew()
        {
            // No-op here: caller should handle creating new project
        }

        private void Browse()
        {
            // Caller should open file dialog; just a signal from here
        }
    }
}
