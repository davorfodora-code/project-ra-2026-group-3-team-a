using System.Windows;
using System.Windows.Controls;
using CommunityHub.Ui.Controls;
using CommunityHub.Ui.ViewModels.BuildingManager;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class NewAssemblyPage : Page
{
    private readonly BuildingManagerWindow _shell;
    private readonly NewAssemblyViewModel  _vm;
    private readonly Action?               _onCreated;

    public NewAssemblyPage(BuildingManagerWindow shell, Action? onCreated = null)
    {
        InitializeComponent();
        _shell     = shell;
        _vm        = new NewAssemblyViewModel(shell.UserId);
        DataContext = _vm;
        _onCreated  = onCreated;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
    }

    private void AddTopic_Click(object sender, RoutedEventArgs e)
    {
        _vm.AddTopic();
    }

    private void RemoveTopic_Click(object sender, RoutedEventArgs e)
    {
        TopicEntryViewModel topic = (TopicEntryViewModel)((Button)sender).Tag;
        _vm.RemoveTopic(topic);
    }

    private void Submit_Click(object sender, RoutedEventArgs e)
    {
        if (!_vm.TrySubmit()) return;
        _onCreated?.Invoke();
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
    }

    // Dozvoljava samo cifre i dvotacku (format HH:mm)
    private void TimeInput_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        e.Handled = e.Text != ":" && !char.IsDigit(e.Text[0]);
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        HelpDialog.Show(this,
            "Odaberite zgradu, datum i vreme skupštine.\n\n" +
            "Dodajte bar jednu temu.\n\n" +
            "Sistem ne dozvoljava zakazivanje u terminu koji je već zauzet.");
    }
}
