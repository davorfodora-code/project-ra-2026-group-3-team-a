using System;
using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Services.BuildingManager.Dto;
using CommunityHub.Ui.Controls;
using CommunityHub.Ui.ViewModels.BuildingManager;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class NewCommonRoomPage : Page
{
    private readonly BuildingManagerWindow _shell;
    private readonly NewCommonRoomViewModel _vm;
    private readonly Action? _onRoomAdded;

    public NewCommonRoomPage(BuildingManagerWindow shell, BuildingDto building, Action? onRoomAdded = null)
    {
        InitializeComponent();
        _shell = shell;
        _vm = new NewCommonRoomViewModel(building);
        DataContext = _vm;
        _onRoomAdded = onRoomAdded;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        HelpDialog.Show(this,
            "Unesite naziv i opis prostorije, odaberite sprat i tip iznajmljivanja.");
    }

    private void Submit_Click(object sender, RoutedEventArgs e)
    {
        if (!_vm.TrySubmit()) return;
        _onRoomAdded?.Invoke();
        _shell.ShowToast("Prostorija je uspešno dodata.");
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
    }
}
