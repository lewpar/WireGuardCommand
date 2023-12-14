using CommunityToolkit.Mvvm.ComponentModel;

namespace WireGuardCommand.ViewModels
{
    public abstract class ViewModel : ObservableObject
    {
        public virtual void Load() { }
    }
}
