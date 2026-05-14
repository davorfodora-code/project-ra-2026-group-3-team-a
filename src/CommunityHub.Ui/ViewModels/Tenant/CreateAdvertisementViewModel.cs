using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Ui.ViewModels.Tenant;

public class CreateAdvertisementViewModel : BaseViewModel
{
    private string _errorMessage = string.Empty;

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public CreateAdvertisementDto? TryBuildDto(
        long tenantId,
        long buildingId,
        bool isOffering,
        DateTime? startDate,
        DateTime? endDate,
        string description,
        string category)
    {
        ErrorMessage = string.Empty;

        if (startDate == null || endDate == null)
        {
            ErrorMessage = "Molimo izaberite opseg datuma.";
            return null;
        }

        if (startDate.Value.Date < DateTime.Today)
        {
            ErrorMessage = "Datum početka ne može biti u prošlosti.";
            return null;
        }

        if (endDate.Value.Date < DateTime.Today)
        {
            ErrorMessage = "Datum kraja ne može biti u prošlosti.";
            return null;
        }

        if (endDate.Value.Date < startDate.Value.Date)
        {
            ErrorMessage = "Datum kraja mora biti posle datuma početka.";
            return null;
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            ErrorMessage = "Molimo unesite opis.";
            return null;
        }

        return new CreateAdvertisementDto
        {
            TenantId = tenantId,
            BuildingId = buildingId,
            Description = description.Trim(),
            IsOffering = isOffering,
            StartDate = startDate.Value,
            EndDate = endDate.Value,
            Category = category
        };
    }
}
