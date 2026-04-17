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
            StreetsPanel.Children.Add(CreateStreetInputBorder(i));
        }
    }

    private static Border CreateStreetInputBorder(int index)
    {
        StackPanel streetPanel = new StackPanel { Tag = index };
        streetPanel.Children.Add(CreateStreetHeader(index));
        streetPanel.Children.Add(CreateLabeledRow("Naziv:", $"StreetName{index}", 250));
        streetPanel.Children.Add(CreateLabeledRow("Od broja:", $"StartNumber{index}", 80));
        streetPanel.Children.Add(CreateLabeledRow("Do broja:", $"EndNumber{index}", 80));

        return new Border
        {
            BorderBrush = System.Windows.Media.Brushes.LightGray,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(8),
            Margin = new Thickness(0, 0, 0, 5),
            CornerRadius = new CornerRadius(3),
            Child = streetPanel
        };
    }

    private static TextBlock CreateStreetHeader(int index)
    {
        return new TextBlock
        {
            Text = $"Ulica {index}:",
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 4)
        };
    }

    private static StackPanel CreateLabeledRow(string label, string textBoxName, double textBoxWidth)
    {
        StackPanel row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 2, 0, 2)
        };
        row.Children.Add(new TextBlock { Text = label, Width = 80, VerticalAlignment = VerticalAlignment.Center });
        row.Children.Add(new TextBox { Name = textBoxName, Width = textBoxWidth });
        return row;
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

        if (!ValidateBasicFields()) return;
        if (!TryParseStreets(out List<DistrictStreet> streets)) return;
        if (!ValidateImagesSelected()) return;

        District district = BuildDistrict(streets);
        TryPersistDistrict(district);
    }

    private bool ValidateBasicFields()
    {
        bool allFilled = !string.IsNullOrWhiteSpace(NameTextBox.Text)
                         && !string.IsNullOrWhiteSpace(DescriptionTextBox.Text)
                         && !string.IsNullOrWhiteSpace(CityTextBox.Text)
                         && !string.IsNullOrWhiteSpace(CountryTextBox.Text);

        if (!allFilled)
        {
            ShowError("Sva polja moraju biti popunjena.");
            return false;
        }
        return true;
    }

    private bool TryParseStreets(out List<DistrictStreet> streets)
    {
        streets = new List<DistrictStreet>();

        if (!TryParsePositive(NumberOfStreetsTextBox.Text, out _))
        {
            ShowError("Broj ulica mora biti pozitivan broj.");
            return false;
        }

        foreach (Border border in StreetsPanel.Children)
        {
            if (!TryParseSingleStreet(border, out DistrictStreet? street))
                return false;
            streets.Add(street!);
        }

        return true;
    }

    private bool TryParseSingleStreet(Border border, out DistrictStreet? street)
    {
        street = null;
        (string name, TextBox startBox, TextBox endBox) = ExtractStreetInputs(border);

        if (string.IsNullOrWhiteSpace(name))
            return FailStreet("Naziv ulice ne sme biti prazan.");

        if (!TryParsePositive(startBox.Text, out int startNumber))
            return FailStreet($"Neispravan pocetni broj za ulicu '{name}'.");

        if (!TryParsePositive(endBox.Text, out int endNumber))
            return FailStreet($"Neispravan krajnji broj za ulicu '{name}'.");

        if (endNumber < startNumber)
            return FailStreet($"Krajnji broj mora biti veci ili jednak pocetnom za ulicu '{name}'.");

        street = new DistrictStreet(name, startNumber, endNumber);
        return true;
    }

    private static (string name, TextBox startBox, TextBox endBox) ExtractStreetInputs(Border border)
    {
        StackPanel streetPanel = (StackPanel)border.Child;
        TextBox nameBox = (TextBox)((StackPanel)streetPanel.Children[1]).Children[1];
        TextBox startBox = (TextBox)((StackPanel)streetPanel.Children[2]).Children[1];
        TextBox endBox = (TextBox)((StackPanel)streetPanel.Children[3]).Children[1];
        return (nameBox.Text.Trim(), startBox, endBox);
    }

    private static bool TryParsePositive(string text, out int value)
    {
        return int.TryParse(text.Trim(), out value) && value > 0;
    }

    private bool FailStreet(string message)
    {
        ShowError(message);
        return false;
    }

    private bool ValidateImagesSelected()
    {
        if (_selectedImagePaths.Count == 0)
        {
            ShowError("Morate odabrati bar jednu sliku.");
            return false;
        }
        return true;
    }

    private District BuildDistrict(List<DistrictStreet> streets)
    {
        Location location = new Location(CityTextBox.Text.Trim(), CountryTextBox.Text.Trim());
        User coordinator = new User(_coordinatorId, "", "", "", "", DateTime.MinValue, "DistrictCoordinator");

        District district = new District(
            NameTextBox.Text.Trim(),
            DescriptionTextBox.Text.Trim(),
            location, 0, coordinator);

        AttachStreetsAndImages(district, streets);
        return district;
    }

    private void AttachStreetsAndImages(District district, List<DistrictStreet> streets)
    {
        foreach (DistrictStreet street in streets)
            district.AddStreet(street);

        foreach (string path in _selectedImagePaths)
            district.AddImage(new DomainImage(path));
    }

    private void TryPersistDistrict(District district)
    {
        try
        {
            _districtRepository.Create(district);
            MessageBox.Show("Kvart je uspesno registrovan!", "Uspeh",
                MessageBoxButton.OK, MessageBoxImage.Information);
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
