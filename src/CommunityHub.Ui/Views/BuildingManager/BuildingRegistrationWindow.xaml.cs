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

        // Validacija osnovnih polja
        if (string.IsNullOrWhiteSpace(StreetTextBox.Text) ||
            string.IsNullOrWhiteSpace(StreetNumberTextBox.Text) ||
            string.IsNullOrWhiteSpace(DistrictTextBox.Text) ||
            string.IsNullOrWhiteSpace(CityTextBox.Text) ||
            string.IsNullOrWhiteSpace(CountryTextBox.Text))
        {
            ShowError("Sva polja moraju biti popunjena.");
            return;
        }

        if (!int.TryParse(NumberOfFloorsTextBox.Text, out int numberOfFloors) || numberOfFloors <= 0)
        {
            ShowError("Broj spratova mora biti pozitivan broj.");
            return;
        }

        // Validacija stanova po spratu
        List<Floor> floors = new List<Floor>();
        foreach (StackPanel floorPanel in FloorsPanel.Children)
        {
            TextBox textBox = (TextBox)floorPanel.Children[1];
            int floorNumber = (int)textBox.Tag;

            string[] apartmentParts = textBox.Text.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (apartmentParts.Length == 0)
            {
                ShowError($"Sprat {floorNumber} mora imati bar jedan stan.");
                return;
            }

            Floor floor = new Floor(floorNumber);
            foreach (string part in apartmentParts)
            {
                if (!int.TryParse(part.Trim(), out int aptNumber))
                {
                    ShowError($"Neispravan broj stana na spratu {floorNumber}: '{part.Trim()}'");
                    return;
                }
                floor.AddApartment(new Apartment(aptNumber));
            }
            floors.Add(floor);
        }

        // Validacija slika
        if (_selectedImagePaths.Count == 0)
        {
            ShowError("Morate odabrati bar jednu sliku.");
            return;
        }

        // Kreiranje Building objekta — naselje je obican tekst, nije zaseban entitet
        Address address = new Address(StreetTextBox.Text.Trim(), StreetNumberTextBox.Text.Trim());
        string neighborhood = DistrictTextBox.Text.Trim();
        Location location = new Location(CityTextBox.Text.Trim(), CountryTextBox.Text.Trim());

        Building building = new Building(address, neighborhood, location, numberOfFloors, _managerId);

        foreach (Floor floor in floors)
        {
            building.AddFloor(floor);
        }

        foreach (string path in _selectedImagePaths)
        {
            building.AddImage(new BuildingImage(path));
        }

        // Upis u bazu
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
