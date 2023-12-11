using CommunityToolkit.Mvvm.ComponentModel;

namespace WireGuardCommand.ViewModels
{
    public partial class RootViewModel : ViewModel
    {
        [ObservableProperty]
        private ViewModel currentViewModel;

        public ProjectNavigatorViewModel ProjectNavigatorViewModel { get; set; }
        public ProjectWindowViewModel ProjectViewModel { get; set; }

        public RootViewModel() 
        {
            ProjectNavigatorViewModel = new ProjectNavigatorViewModel(this);
            ProjectViewModel = new ProjectWindowViewModel(this);

            CurrentViewModel = ProjectNavigatorViewModel;
        }

        public void ChangeViewModel(ViewModel viewModel)
        {
            CurrentViewModel = viewModel;
        }
    }
}
