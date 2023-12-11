using CommunityToolkit.Mvvm.Input;

namespace WireGuardCommand.ViewModels
{
    public partial class ProjectNewViewModel : ViewModel
    {
        private readonly RootViewModel rootViewModel;

        public ProjectNewViewModel(RootViewModel rootViewModel) 
        {
            this.rootViewModel = rootViewModel;
        }

        [RelayCommand]
        private void GoBack()
        {
            rootViewModel.ChangeViewModel(rootViewModel.ProjectNavigatorViewModel);
        }
    }
}
