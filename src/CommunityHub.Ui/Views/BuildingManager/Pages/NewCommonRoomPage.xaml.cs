using System;
using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Services.BuildingManager.Dto;
using CommunityHub.Ui.ViewModels.BuildingManager;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class NewCommonRoomPage : Page
{
    private readonly BuildingManagerWindow _shell;
    private readonly CommonRoomsViewModel _vm;
    private readonly BuildingDto _building;
    private readonly Action? _onRoomAdded;

    public NewCommonRoomPage(BuildingManagerWindow shell, BuildingDto building, Action? onRoomAdded = null)
    {
        InitializeComponent();
        _shell = shell;
        _building = building;
        _vm = new CommonRoomsViewModel();
        _onRoomAdded = onRoomAdded;

        BuildingLabel.Text = $"Zgrada: {_building.AddressDisplay}";
        FloorHint.Text = $"(dostupni spratovi: 0-{_building.NumberOfFloors})";

        for (int i = 0; i <= _building.NumberOfFloors; i++)
            FloorCombo.Items.Add(i);
        FloorCombo.SelectedIndex = 0;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Unesite naziv i opis prostorije, odaberite sprat i tip iznajmljivanja.",
            "Pomoc", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Submit_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Visibility = Visibility.Collapsed;

        string name = NameBox.Text.Trim();
        string description = DescriptionBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            ShowError("Unesite naziv prostorije.");
            return;
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            ShowError("Unesite opis prostorije.");
            return;
        }

        int floor = (int)FloorCombo.SelectedItem;

        CreateCommonRoomDto dto = new CreateCommonRoomDto
        {
            BuildingId = _building.Id,
            Name = name,
            Description = description,
            FloorNumber = floor,
            IsMultiDay = MultiDayRadio.IsChecked == true
        };

        try
        {
            _vm.Add(dto);
            _onRoomAdded?.Invoke();
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }
        catch (Exception ex)
        {
            ShowError($"Greska: {ex.Message}");
        }
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }
}
