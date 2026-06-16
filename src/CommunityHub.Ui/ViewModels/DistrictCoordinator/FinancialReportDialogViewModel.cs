using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using CommunityHub.Application.Services.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class FinancialReportDialogViewModel : ViewModelBase
{
    private readonly BudgetService _budgetService;
    private DistrictDto?  _selectedDistrict;
    private DateTime      _fromDate;
    private DateTime      _toDate;
    private string        _errorMessage = string.Empty;

    public FinancialReportDialogViewModel(IEnumerable<DistrictDto> districts, BudgetService budgetService)
    {
        _budgetService    = budgetService;
        Districts         = new List<DistrictDto>(districts);
        _selectedDistrict = Districts.FirstOrDefault();

        int curQ  = (DateTime.Now.Month - 1) / 3;
        _fromDate = new DateTime(DateTime.Now.Year, curQ * 3 + 1, 1);
        _toDate   = DateTime.Now;

        CancelCommand   = new RelayCommand(_ => CancelRequested?.Invoke());
        GenerateCommand = new RelayCommand(_ => GenerateReport(), _ => _selectedDistrict != null);
    }

    public List<DistrictDto> Districts        { get; }
    public DistrictDto?      SelectedDistrict { get => _selectedDistrict; set => SetProperty(ref _selectedDistrict, value); }
    public DateTime          FromDate         { get => _fromDate;  set => SetProperty(ref _fromDate,  value); }
    public DateTime          ToDate           { get => _toDate;    set => SetProperty(ref _toDate,    value); }
    public string            ErrorMessage     { get => _errorMessage; private set { SetProperty(ref _errorMessage, value); OnPropertyChanged(nameof(HasError)); } }
    public bool              HasError         => !string.IsNullOrEmpty(_errorMessage);

    public ICommand CancelCommand   { get; }
    public ICommand GenerateCommand { get; }

    public event Action? CancelRequested;
    public event Action? ReportGenerated;

    private void GenerateReport()
    {
        if (_selectedDistrict == null) return;
        ErrorMessage = string.Empty;
        try
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Title    = "Sacuvaj PDF izvestaj",
                Filter   = "PDF dokument (*.pdf)|*.pdf",
                FileName = $"Finansijski_izvestaj_{_selectedDistrict.Name}_{_fromDate:yyyyMMdd}_{_toDate:yyyyMMdd}.pdf"
            };
            if (dlg.ShowDialog() != true) return;

            var summary = _budgetService.GetSummary(_selectedDistrict.Id, _fromDate, _toDate);
            var cats    = _budgetService.GetCategoryBudgets(_selectedDistrict.Id, _fromDate, _toDate);

            PdfReportGenerator.Generate(dlg.FileName, _selectedDistrict.Name, _fromDate, _toDate, summary, cats);

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
            ReportGenerated?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Greska pri generisanju: {ex.Message}";
        }
    }
}
