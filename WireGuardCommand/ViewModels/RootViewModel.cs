namespace WireGuardCommand.ViewModels
{
    public class RootViewModel : ViewModel
    {
        public ViewModel CurrentViewModel { get; set; }

        public ProjectNavigatorViewModel ProjectNavigatorViewModel { get; set; }
        public ProjectWindowViewModel ProjectViewModel { get; set; }

        public RootViewModel() 
        {
            ProjectNavigatorViewModel = new ProjectNavigatorViewModel();
            ProjectViewModel = new ProjectWindowViewModel();

            CurrentViewModel = ProjectNavigatorViewModel;
        }
    }
}
