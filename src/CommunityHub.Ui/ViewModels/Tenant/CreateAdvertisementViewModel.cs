using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Ui.ViewModels.Tenant;

public class CreateAdvertisementViewModel : BaseViewModel
{
    private string _errorMessage      = string.Empty;
    private string _descriptionError  = string.Empty;

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public string DescriptionError
    {
        get => _descriptionError;
        private set
        {
            if (SetProperty(ref _descriptionError, value))
                OnPropertyChanged(nameof(HasDescriptionError));
        }
    }
    public bool HasDescriptionError => !string.IsNullOrEmpty(_descriptionError);

    public CreateAdvertisementDto? TryBuildDto(
        long tenantId,
        long buildingId,
        bool isOffering,
        DateTime? startDate,
        DateTime? endDate,
        string description,
        string category)
    {
        ErrorMessage     = string.Empty;
        DescriptionError = string.Empty;

        string? descError = ValidateDescription(description);
        if (descError != null) { DescriptionError = descError; return null; }

        string? dateError = ValidateDateRange(startDate, endDate);
        if (dateError != null) { ErrorMessage = dateError; return null; }
        return new CreateAdvertisementDto
        {
            TenantId    = tenantId,
            BuildingId  = buildingId,
            Description = description.Trim(),
            IsOffering  = isOffering,
            StartDate   = startDate!.Value,
            EndDate     = endDate!.Value,
            Category    = category
        };
    }

    private static string? ValidateDateRange(DateTime? startDate, DateTime? endDate)
    {
        if (startDate == null || endDate == null)
            return "Molimo izaberite opseg datuma.";
        if (startDate.Value.Date < DateTime.Today)
            return "Datum početka ne može biti u prošlosti.";
        if (endDate.Value.Date < DateTime.Today)
            return "Datum kraja ne može biti u prošlosti.";
        if (endDate.Value.Date < startDate.Value.Date)
            return "Datum kraja mora biti posle datuma početka.";
        return null;
    }

    private static string? ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return "Molimo unesite opis.";
        return null;
    }
}
