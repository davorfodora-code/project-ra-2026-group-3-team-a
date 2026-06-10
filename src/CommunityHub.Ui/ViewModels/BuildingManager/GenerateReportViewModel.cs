using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CommunityHub.Application;
using CommunityHub.Application.Services.BuildingManager;
using CommunityHub.Application.Services.BuildingManager.Dto;
using CommunityHub.Ui.Reports;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

// VM za formu generisanja PDF izveštaja o skupštinama u datom periodu za izabranu zgradu.
public class GenerateReportViewModel : BaseViewModel
{
    private readonly BuildingAccessRequestService _buildingService;
    private readonly AssemblyService              _assemblyService;
    private readonly long _managerId;

    private BuildingDto? _selectedBuilding;
    private DateTime?    _startDate;
    private DateTime?    _endDate;
    private string       _errorMessage = string.Empty;

    public GenerateReportViewModel(long managerId)
    {
        _managerId       = managerId;
        _buildingService = new BuildingAccessRequestService(
            Injector.Get<IBuildingAccessRequestRepository>());
        _assemblyService = new AssemblyService(
            Injector.Get<IAssemblyRepository>(),
            Injector.Get<IBuildingRepository>());

        Buildings = new ObservableCollection<BuildingDto>(
            _buildingService.GetBuildingsByManagerId(managerId));

        // Default period — od početka tekuće godine do danas
        _startDate = new DateTime(DateTime.Today.Year, 1, 1);
        _endDate   = DateTime.Today;
    }

    public ObservableCollection<BuildingDto> Buildings { get; }

    public BuildingDto? SelectedBuilding
    {
        get => _selectedBuilding;
        set => SetProperty(ref _selectedBuilding, value);
    }

    public DateTime? StartDate
    {
        get => _startDate;
        set => SetProperty(ref _startDate, value);
    }

    public DateTime? EndDate
    {
        get => _endDate;
        set => SetProperty(ref _endDate, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (SetProperty(ref _errorMessage, value))
                OnPropertyChanged(nameof(HasError));
        }
    }
    public bool HasError => !string.IsNullOrEmpty(_errorMessage);

    // Validira polja, generiše PDF u %Temp%, otvara ga sa default viewer-om
    public bool TryGenerate()
    {
        ErrorMessage = string.Empty;
        string? err = Validate();
        if (err != null) { ErrorMessage = err; return false; }

        try
        {
            List<AssemblyDetailDto> assemblies = LoadAssembliesInRange(
                _selectedBuilding!.Id, _startDate!.Value, _endDate!.Value);

            byte[] pdf = AssembliesReportGenerator.Generate(
                _selectedBuilding!, _startDate!.Value, _endDate!.Value, assemblies);

            string path = SaveToTemp(pdf, _selectedBuilding!, _startDate!.Value, _endDate!.Value);
            OpenWithDefault(path);
            return true;
        }
        catch (Exception ex)
        {
            ErrorMessage = "Greška pri generisanju izveštaja: " + ex.Message;
            return false;
        }
    }

    private string? Validate()
    {
        if (_selectedBuilding == null) return "Odaberite zgradu.";
        if (_startDate == null)        return "Odaberite početni datum.";
        if (_endDate == null)          return "Odaberite krajnji datum.";
        if (_endDate < _startDate)     return "Krajnji datum mora biti posle početnog.";
        return null;
    }

    // Dohvata sve skupstine za zgradu u periodu (sa temama i prisustvom)
    private List<AssemblyDetailDto> LoadAssembliesInRange(long buildingId, DateTime from, DateTime to)
    {
        DateTime fromD = from.Date;
        DateTime toD   = to.Date.AddDays(1).AddTicks(-1);

        List<AssemblySummaryDto> all = _assemblyService.GetByManagerId(_managerId);
        IEnumerable<AssemblySummaryDto> filtered = all
            .Where(a => a.BuildingId == buildingId)
            .Where(a => a.ScheduledAt >= fromD && a.ScheduledAt <= toD)
            .OrderBy(a => a.ScheduledAt);

        List<AssemblyDetailDto> result = new List<AssemblyDetailDto>();
        foreach (AssemblySummaryDto s in filtered)
        {
            AssemblyDetailDto? detail = _assemblyService.GetDetail(s.Id);
            if (detail != null) result.Add(detail);
        }
        return result;
    }

    private static string SaveToTemp(byte[] pdf, BuildingDto building, DateTime from, DateTime to)
    {
        string safeName = $"Izvestaj_{building.AddressDisplay}_{from:yyyyMMdd}-{to:yyyyMMdd}.pdf";
        foreach (char c in Path.GetInvalidFileNameChars())
            safeName = safeName.Replace(c, '_');
        string path = Path.Combine(Path.GetTempPath(), safeName);
        File.WriteAllBytes(path, pdf);
        return path;
    }

    private static void OpenWithDefault(string path)
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName        = path,
            UseShellExecute = true   // otvara sa default app-om za .pdf
        };
        Process.Start(psi);
    }
}
