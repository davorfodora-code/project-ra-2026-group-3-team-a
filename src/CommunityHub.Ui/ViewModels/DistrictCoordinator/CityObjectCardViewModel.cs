using System;
using System.Windows.Input;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class CityObjectCardViewModel : ViewModelBase
{
    public CityObjectCardViewModel(CityObjectDto dto, Action<CityObjectCardViewModel> onReserve)
    {
        Id                      = dto.Id;
        Name                    = dto.Name;
        LocationCapacityDisplay = dto.LocationCapacityDisplay;
        VoteCount               = dto.VoteCount;
        ReserveCommand          = new RelayCommand(_ => onReserve(this));
    }

    public long   Id                      { get; }
    public string Name                    { get; }
    public string LocationCapacityDisplay { get; }
    public int    VoteCount               { get; }

    public ICommand ReserveCommand { get; }
}
