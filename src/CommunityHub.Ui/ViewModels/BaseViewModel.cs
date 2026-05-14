using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CommunityHub.Ui.ViewModels;

// Bazna klasa za sve ViewModel-e.
// Pruza INotifyPropertyChanged podrsku, tako da XAML binding moze
// da reaguje kad se property promeni (UI se automatski azurira).
public abstract class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    // Postavi vrednost polja i obavesti UI ako je vrednost stvarno promenjena.
    // [CallerMemberName] automatski hvata ime propertyja iz koga je pozvan.
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    // Rucno javi UI-u da se property promenio (kad SetProperty nije primenjiv,
    // npr. za computed property koji zavisi od drugog polja).
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
