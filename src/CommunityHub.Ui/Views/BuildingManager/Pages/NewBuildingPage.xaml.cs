using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityHub.Ui.Controls;
using CommunityHub.Ui.ViewModels.BuildingManager;
using Microsoft.Win32;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class NewBuildingPage : Page
{
    private readonly BuildingManagerWindow _shell;
    private readonly NewBuildingViewModel _vm;
    private readonly Action? _onBuildingAdded;

    public NewBuildingPage(BuildingManagerWindow shell, Action? onBuildingAdded = null)
    {
        InitializeComponent();
        _shell = shell;
        _vm = new NewBuildingViewModel(shell.UserId);
        DataContext = _vm;
        _onBuildingAdded = onBuildingAdded;
    }

    // --- Spratovi / stanovi ---

    private void AddApartment_Click(object sender, RoutedEventArgs e)
    {
        FloorInputViewModel floor = (FloorInputViewModel)((Button)sender).DataContext;
        floor.StartAddingApartment();
    }

    private void ApartmentInput_Loaded(object sender, RoutedEventArgs e)
    {
        // Kad se inline TextBox pojavi, fokusiraj ga
        ((TextBox)sender).Focus();
    }

    private void ApartmentInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        FloorInputViewModel floor = (FloorInputViewModel)((TextBox)sender).DataContext;
        floor.ConfirmApartment();
    }

    private void ApartmentInput_LostFocus(object sender, RoutedEventArgs e)
    {
        FloorInputViewModel floor = (FloorInputViewModel)((TextBox)sender).DataContext;
        floor.ConfirmApartment();
    }

    private void RemoveApartment_Click(object sender, RoutedEventArgs e)
    {
        Button btn = (Button)sender;
        int aptNumber = (int)btn.DataContext;

        // Hodaj uz vizuelno stablo dok ne nadjes element ciji je DataContext FloorInputViewModel
        FrameworkElement? el = btn;
        while (el != null && el.DataContext is not FloorInputViewModel)
            el = VisualTreeHelper.GetParent(el) as FrameworkElement;

        if (el?.DataContext is FloorInputViewModel floor)
            floor.RemoveApartment(aptNumber);
    }

    // --- Slike ---

    private void AddImages_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog dialog = new OpenFileDialog
        {
            Multiselect = true,
            Filter = "Slike|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
        };

        if (dialog.ShowDialog() != true) return;

        foreach (string path in dialog.FileNames)
            _vm.AddImage(path);
    }

    private void RemoveImage_Click(object sender, RoutedEventArgs e)
    {
        string path = (string)((Button)sender).DataContext;
        _vm.RemoveImage(path);
    }

    // --- Navigacija ---

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            _shell.MainFrame.Navigate(new BuildingsPage(_shell));
    }

    private void Submit_Click(object sender, RoutedEventArgs e)
    {
        if (!_vm.TrySubmit()) return;
        _onBuildingAdded?.Invoke();
        _shell.ShowToast("Zgrada je uspešno dodata.");
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        HelpDialog.Show(this, "Pomoć - Nova zgrada",
            "Na ovoj stranici registrujete novu zgradu. " +
            "Popunite adresu, naselje, lokaciju i broj spratova. " +
            "Za svaki sprat unesite brojeve stanova. Na kraju dodajte slike zgrade.\n\n" +
            "*Tooltipovi se mogu uključiti/isključiti u Podešavanjima.");
    }
}
