using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Services.BuildingManager.Dto;
using CommunityHub.Ui.Controls;
using CommunityHub.Ui.ViewModels.BuildingManager;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class AssemblyDetailPage : Page
{
    private readonly BuildingManagerWindow   _shell;
    private readonly AssemblyDetailViewModel _vm;

    public AssemblyDetailPage(BuildingManagerWindow shell, long assemblyId)
    {
        InitializeComponent();
        _shell      = shell;
        _vm         = new AssemblyDetailViewModel(assemblyId);
        DataContext = _vm;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
    }

    // Dodaje predlozenu temu kao dodatnu temu skupstine (1.7)
    private void AddSuggestion_Click(object sender, RoutedEventArgs e)
    {
        TopicSuggestionDto suggestion = (TopicSuggestionDto)((Button)sender).Tag;
        _vm.AddSuggestedTopic(suggestion);
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        HelpDialog.Show(this,
            "Pregled detalja skupštine.\n\n" +
            "Prisustvo po stanovima prikazuje ko je potvrdio dolazak.");
    }
}
