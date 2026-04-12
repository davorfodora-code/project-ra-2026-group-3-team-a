using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Database.Repositories.DistrictCoordinator;
using CommunityHub.Application.Domain;
using Microsoft.Win32;
using DomainImage = CommunityHub.Application.Domain.Image;

namespace CommunityHub.Ui.Views.DistrictCoordinator;

public partial class DistrictRegWindow : Window
{
    private readonly long _coordinatorId;
    private readonly DistrictDbRepository _districtRepository;
    private readonly List<string> _selectedImagePaths;

    public DistrictRegWindow(long coordinatorId)
    {
        InitializeComponent();
        _coordinatorId = coordinatorId;
        _districtRepository = new DistrictDbRepository();
        _selectedImagePaths = new List<string>();
    }

    private void NumberOfStreetsTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        StreetsPanel.Children.Clear();

        if (!int.TryParse(NumberOfStreetsTextBox.Text, out int numberOfStreets) || numberOfStreets <= 0)
            return;

        for (int i = 1; i <= numberOfStreets; i++)
        {
            Border border = new Border
            {
                BorderBrush = System.Windows.Media.Brushes.LightGray,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(8),
                Margin = new Thickness(0, 0, 0, 5),
                CornerRadius = new CornerRadius(3)
            };

            StackPanel streetPanel = new StackPanel { Tag = i };

            TextBlock header = new TextBlock
            {
                Text = $"Ulica {i}:",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 4)
            };
            streetPanel.Children.Add(header);

            StackPanel nameRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
            nameRow.Children.Add(new TextBlock { Text = "Naziv:", Width = 80, VerticalAlignment = VerticalAlignment.Center });
            nameRow.Children.Add(new TextBox { Name = $"StreetName{i}", Width = 250 });
            streetPanel.Children.Add(nameRow);

            StackPanel startRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
            startRow.Children.Add(new TextBlock { Text = "Od broja:", Width = 80, VerticalAlignment = VerticalAlignment.Center });
            startRow.Children.Add(new TextBox { Name = $"StartNumber{i}", Width = 80 });
            streetPanel.Children.Add(startRow);

            StackPanel endRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
            endRow.Children.Add(new TextBlock { Text = "Do broja:", Width = 80, VerticalAlignment = VerticalAlignment.Center });
            endRow.Children.Add(new TextBox { Name = $"EndNumber{i}", Width = 80 });
            streetPanel.Children.Add(endRow);

            border.Child = streetPanel;
            StreetsPanel.Children.Add(border);
        }
    }

    private void SelectImagesButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog dialog = new OpenFileDialog
        {
            Multiselect = true,
            Filter = "Slike|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
        };

        if (dialog.ShowDialog() == true)
        {
            _selectedImagePaths.Clear();
            _selectedImagePaths.AddRange(dialog.FileNames);

            ImagesListBox.Items.Clear();
            foreach (string path in _selectedImagePaths)
            {
                ImagesListBox.Items.Add(System.IO.Path.GetFileName(path));
            }
        }
    }

    private void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorTextBlock.Visibility = Visibility.Collapsed;

        if (string.IsNullOrWhiteSpace(NameTextBox.Text) ||
            string.IsNullOrWhiteSpace(DescriptionTextBox.Text) ||
            string.IsNullOrWhiteSpace(CityTextBox.Text) ||
            string.IsNullOrWhiteSpace(CountryTextBox.Text))
        {
            ShowError("Sva polja moraju biti popunjena.");
            return;
        }

        if (!int.TryParse(NumberOfStreetsTextBox.Text, out int numberOfStreets) || numberOfStreets <= 0)
        {
            ShowError("Broj ulica mora biti pozitivan broj.");
            return;
        }

        List<DistrictStreet> streets = new List<DistrictStreet>();
        foreach (Border border in StreetsPanel.Children)
        {
            StackPanel streetPanel = (StackPanel)border.Child;

            StackPanel nameRow = (StackPanel)streetPanel.Children[1];
            TextBox nameBox = (TextBox)nameRow.Children[1];

            StackPanel startRow = (StackPanel)streetPanel.Children[2];
            TextBox startBox = (TextBox)startRow.Children[1];

            StackPanel endRow = (StackPanel)streetPanel.Children[3];
            TextBox endBox = (TextBox)endRow.Children[1];

            string streetName = nameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(streetName))
            {
                ShowError("Naziv ulice ne sme biti prazan.");
                return;
            }

            if (!int.TryParse(startBox.Text.Trim(), out int startNumber) || startNumber <= 0)
            {
                ShowError($"Neispravan pocetni broj za ulicu '{streetName}'.");
                return;
            }

            if (!int.TryParse(endBox.Text.Trim(), out int endNumber) || endNumber <= 0)
            {
                ShowError($"Neispravan krajnji broj za ulicu '{streetName}'.");
                return;
            }

            if (endNumber < startNumber)
            {
                ShowError($"Krajnji broj mora biti veci ili jednak pocetnom za ulicu '{streetName}'.");
                return;
            }

            streets.Add(new DistrictStreet(streetName, startNumber, endNumber));
        }

        if (_selectedImagePaths.Count == 0)
        {
            ShowError("Morate odabrati bar jednu sliku.");
            return;
        }

        Location location = new Location(CityTextBox.Text.Trim(), CountryTextBox.Text.Trim());
        User coordinator = new User(_coordinatorId, "", "", "", "", DateTime.MinValue, "DistrictCoordinator");

        District district = new District(
            NameTextBox.Text.Trim(),
            DescriptionTextBox.Text.Trim(),
            location,
            0,
            coordinator);

        foreach (DistrictStreet street in streets)
        {
            district.AddStreet(street);
        }

        foreach (string path in _selectedImagePaths)
        {
            district.AddImage(new DomainImage(path));
        }

        try
        {
            _districtRepository.Create(district);
            MessageBox.Show("Kvart je uspesno registrovan!", "Uspeh", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }
        catch (Exception ex)
        {
            ShowError($"Greska prilikom registracije: {ex.Message}");
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void ShowError(string message)
    {
        ErrorTextBlock.Text = message;
        ErrorTextBlock.Visibility = Visibility.Visible;
    }
}
