using System.Windows;
using CommunityHub.Application.Domain;

namespace CommunityHub.Ui.Views.Citizen;

public class DistrictAccessRequestRowViewModel
{
    public long RequestId { get; }
    public string DistrictName { get; }
    public DateTime CreatedAt { get; }
    public string CreatedAtFormatted => CreatedAt.ToString("dd.MM.yyyy. HH:mm");
    public string Status { get; }
    public Visibility CancelButtonVisibility { get; }

    public DistrictAccessRequestRowViewModel(DistrictAccessRequest request)
    {
        RequestId = request.Id;
        DistrictName = request.District.Name;
        CreatedAt = request.CreationDate;
        Status = request.Status.ToString();
        CancelButtonVisibility = request.Status == DistrictAccessRequestStatus.Pending
            ? Visibility.Visible
            : Visibility.Collapsed;
    }
}
