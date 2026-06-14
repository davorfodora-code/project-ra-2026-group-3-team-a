using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CommunityHub.Application;
using CommunityHub.Application.Services.Tenant;
using CommunityHub.Application.Services.Tenant.Dto;
using CommunityHub.Ui.Reports;

namespace CommunityHub.Ui.ViewModels.Tenant;

public class HelpSchedulingReportViewModel : BaseViewModel
{
    private readonly SchedulingService _service;
    private readonly long _tenantId;

    private DateTime? _startDate;
    private DateTime? _endDate;
    private string    _selectedType = "Sve";
    private string    _errorMessage = string.Empty;

    public IReadOnlyList<string> TypeOptions { get; } = ["Sve", "Nudim pomoć", "Tražim pomoć"];

    public HelpSchedulingReportViewModel(long tenantId)
    {
        _tenantId  = tenantId;
        _service   = new SchedulingService(Injector.Get<ISchedulingRepository>());
        _startDate = new DateTime(DateTime.Today.Year, 1, 1);
        _endDate   = DateTime.Today;
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

    public string SelectedType
    {
        get => _selectedType;
        set => SetProperty(ref _selectedType, value);
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

    public bool TryGenerate()
    {
        ErrorMessage = string.Empty;
        string? err = Validate();
        if (err != null) { ErrorMessage = err; return false; }

        try
        {
            DateTime from = _startDate!.Value.Date;
            DateTime to   = _endDate!.Value.Date.AddDays(1).AddTicks(-1);

            List<ReportAppointmentDto> all = _service.GetReportAppointments(_tenantId, from, to);
            List<ReportAppointmentDto> filtered = _selectedType switch
            {
                "Nudim pomoć"  => all.Where(a =>  a.IAmOffering).ToList(),
                "Tražim pomoć" => all.Where(a => !a.IAmOffering).ToList(),
                _              => all,
            };

            byte[] pdf = HelpSchedulingReportGenerator.Generate(
                _startDate!.Value, _endDate!.Value, _selectedType, filtered);

            string path = SaveToTemp(pdf, _startDate!.Value, _endDate!.Value);
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
        if (_startDate == null)        return "Odaberite početni datum.";
        if (_endDate == null)          return "Odaberite krajnji datum.";
        if (_endDate < _startDate)     return "Krajnji datum mora biti posle početnog.";
        return null;
    }

    private static string SaveToTemp(byte[] pdf, DateTime from, DateTime to)
    {
        string name = $"Izvestaj_Termini_{from:yyyyMMdd}-{to:yyyyMMdd}.pdf";
        string path = Path.Combine(Path.GetTempPath(), name);
        File.WriteAllBytes(path, pdf);
        return path;
    }

    private static void OpenWithDefault(string path)
    {
        Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
    }
}
