using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Database.Repositories.DistrictCoordinator;
using CommunityHub.Application.Domain;

namespace CommunityHub.Ui.Views.DistrictCoordinator;

public class DistrictComboItem
{
    public long DistrictId { get; set; }
    public string DisplayText { get; set; }

    public DistrictComboItem(District district)
    {
        DistrictId = district.Id;
        DisplayText = $"{district.Name} — {district.Location.City}, {district.Location.Country}";
    }
}

public class DistrictMembershipRow
{
    public string CitizenFullName { get; set; }
    public string CitizenAddress { get; set; }
    public DateTime JoinDate { get; set; }
    public string JoinDateFormatted => JoinDate.ToString("dd.MM.yyyy.");

    public DistrictMembershipRow(DistrictMembership membership)
    {
        CitizenFullName = $"{membership.User.Name} {membership.User.Surname}";
        CitizenAddress = membership.User.Address ?? "Nije navedena";
        JoinDate = membership.JoinDate;
    }
}

public partial class MembershipsWindow : Window
{
    private readonly long _coordinatorId;
    private readonly DistrictAccessRequestDbRepository _repository;

    public MembershipsWindow(long coordinatorId)
    {
        InitializeComponent();
        _coordinatorId = coordinatorId;
        _repository = new DistrictAccessRequestDbRepository();

        LoadDistricts();
    }

    private void LoadDistricts()
    {
        List<District> districts = _repository.GetDistrictsByCoordinatorId(_coordinatorId);
        List<DistrictComboItem> items = districts.Select(d => new DistrictComboItem(d)).ToList();

        DistrictComboBox.ItemsSource = items;

        if (items.Count > 0)
        {
            DistrictComboBox.SelectedIndex = 0;
        }
    }

    private void DistrictComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DistrictComboBox.SelectedItem is DistrictComboItem selected)
        {
            List<DistrictMembership> memberships = _repository.GetMembershipsByDistrictId(selected.DistrictId);
            MembershipsDataGrid.ItemsSource = memberships.Select(m => new DistrictMembershipRow(m)).ToList();
        }
    }
}
