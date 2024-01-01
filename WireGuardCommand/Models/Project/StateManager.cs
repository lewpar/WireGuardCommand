namespace WireGuardCommand.Models.Project
{
    public class StateManager
    {
        private static StateManager? instance;
        public static StateManager Instance 
        { 
            get
            {
                if(instance is null)
                {
                    instance = new StateManager();
                }

                return instance;
            }
        }

        public WGCProject? CurrentProject { get; set; }

        public void SetProject(WGCProject project)
        {
            CurrentProject = project;
        }
    }
}
