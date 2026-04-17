using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Database.Repositories.BuildingManager;
using CommunityHub.Application.Domain;
using Microsoft.Win32;

namespace CommunityHub.Ui.Views.BuildingManager;

public partial class BuildingRegistrationWindow : Window
{
    private readonly long _managerId;
    private readonly BuildingDbRepository _buildingRepository;
    private readonly List<string> _selectedImagePaths;

    public BuildingRegistrationWindow(long managerId)
    {
        InitializeComponent();
        _managerId = managerId;
        _buildingRepository = new BuildingDbRepository();
        _selectedImagePaths = new List<string>();
    }

    // Kada se promeni broj spratova, dinamički generiši polja za unos stanova
    private void NumberOfFloorsTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        FloorsPanel.Children.Clear();

        if (!int.TryParse(NumberOfFloorsTextBox.Text, out int numberOfFloors) || numberOfFloors <= 0)
            return;

        for (int i = 1; i <= numberOfFloors; i++)
        {
            StackPanel floorPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };

            TextBlock label = new TextBlock
            {
                Text = $"Sprat {i}:",
                Width = 60,
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBox textBox = new TextBox
            {
                Name = $"Floor{i}TextBox",
                Width = 300,
                Tag = i // čuvamo broj sprata u Tag-u
            };

            floorPanel.Children.Add(label);
            floorPanel.Children.Add(textBox);
            FloorsPanel.Children.Add(floorPanel);
        }
    }

    // Otvori file dialog za odabir slika
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

        List<Floor>? floors = ParseFloors();
        if (floors == null) return;

        Building building = BuildBuilding(floors);

        try
        {
            _buildingRepository.Create(building);
            MessageBox.Show("Zgrada je uspešno registrovana!", "Uspeh", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }
        catch (Exception ex)
        {
            ShowError($"Greška prilikom registracije: {ex.Message}");
        }
    }

    // Proverava da li su sva osnovna polja popunjena i ispravna
    private bool ValidateBasicFields()
    {
        if (string.IsNullOrWhiteSpace(StreetTextBox.Text) ||
            string.IsNullOrWhiteSpace(StreetNumberTextBox.Text) ||
            string.IsNullOrWhiteSpace(DistrictTextBox.Text) ||
            string.IsNullOrWhiteSpace(CityTextBox.Text) ||
            string.IsNullOrWhiteSpace(CountryTextBox.Text))
        {
            ShowError("Sva polja moraju biti popunjena.");
            return false;
        }

        if (!int.TryParse(NumberOfFloorsTextBox.Text, out int numberOfFloors) || numberOfFloors <= 0)
        {
            ShowError("Broj spratova mora biti pozitivan broj.");
            return false;
        }

        if (_selectedImagePaths.Count == 0)
        {
            ShowError("Morate odabrati bar jednu sliku.");
            return false;
        }

        return true;
    }

    // Parsira stanove po spratovima iz dinamički generisanih polja; vraća null ako validacija ne prođe
    private List<Floor>? ParseFloors()
    {
        List<Floor> floors = new List<Floor>();

        foreach (StackPanel floorPanel in FloorsPanel.Children)
        {
            TextBox textBox = (TextBox)floorPanel.Children[1];
            int floorNumber = (int)textBox.Tag;

            string[] apartmentParts = textBox.Text.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (apartmentParts.Length == 0)
            {
                ShowError($"Sprat {floorNumber} mora imati bar jedan stan.");
                return null;
            }

            Floor floor = new Floor(floorNumber);
            foreach (string part in apartmentParts)
            {
                if (!int.TryParse(part.Trim(), out int aptNumber))
                {
                    ShowError($"Neispravan broj stana na spratu {floorNumber}: '{part.Trim()}'");
                    return null;
                }
                floor.AddApartment(new Apartment(aptNumber));
            }

            floors.Add(floor);
        }

        return floors;
    }

    // Sastavlja Building objekat od unesenih podataka i spratova
    private Building BuildBuilding(List<Floor> floors)
    {
        Address address = new Address(StreetTextBox.Text.Trim(), StreetNumberTextBox.Text.Trim());
        string neighborhood = DistrictTextBox.Text.Trim();
        Location location = new Location(CityTextBox.Text.Trim(), CountryTextBox.Text.Trim());
        int numberOfFloors = int.Parse(NumberOfFloorsTextBox.Text);

        Building building = new Building(address, neighborhood, location, numberOfFloors, _managerId);

        foreach (Floor floor in floors)
            building.AddFloor(floor);

        foreach (string path in _selectedImagePaths)
            building.AddImage(new BuildingImage(path));

        return building;
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
