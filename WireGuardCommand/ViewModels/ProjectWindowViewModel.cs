using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using WireGuardCommand.Models.Project;

namespace WireGuardCommand.ViewModels
{
    public partial class ProjectWindowViewModel : ViewModel
    {
        [ObservableProperty]
        private WGCProject? project;

        private readonly RootViewModel rootViewModel;

        public ProjectWindowViewModel(RootViewModel rootViewModel) 
        {
            this.rootViewModel = rootViewModel;
        }

        [RelayCommand]
        private void CloseProject()
        {
            rootViewModel.ChangeViewModel(rootViewModel.ProjectNavigatorViewModel);
        }
    }
}
