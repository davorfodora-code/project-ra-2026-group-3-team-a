using System;
using CommunityHub.Application.Services.DistrictCoordinator;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public partial class FinancesViewModel
{
    public void EnsureLoaded()
    {
        if (Districts.Count > 0) return;
        try
        {
            var districts = _districtService.GetDistrictsByCoordinatorId(_coordinatorId);
            foreach (var d in districts) Districts.Add(d);
            if (Districts.Count > 0) SelectedDistrict = Districts[0];
        }
        catch (Exception ex)
        {
            LoadError = $"Greška pri učitavanju kvartova: {ex.Message}";
        }
    }

    public void RefreshDistricts()
    {
        if (Districts.Count == 0) return;
        var currentId = _selectedDistrict?.Id;
        Districts.Clear();
        try
        {
            var districts = _districtService.GetDistrictsByCoordinatorId(_coordinatorId);
            foreach (var d in districts) Districts.Add(d);
            if (currentId.HasValue)
                foreach (var d in Districts)
                    if (d.Id == currentId.Value) { SelectedDistrict = d; return; }
            if (Districts.Count > 0) SelectedDistrict = Districts[0];
        }
        catch (Exception ex)
        {
            LoadError = $"Greška pri osvežavanju kvartova: {ex.Message}";
        }
    }

    public void PreSelectDistrict(long districtId)
    {
        EnsureLoaded();
        foreach (var d in Districts)
            if (d.Id == districtId) { SelectedDistrict = d; break; }
    }

    private void LoadGridData()
    {
        if (_selectedDistrict == null || _selectedPeriod == null) return;
        try
        {
            LoadError = string.Empty;
            var summary = _budgetService.GetSummary(
                _selectedDistrict.Id, _selectedPeriod.Start, _selectedPeriod.End);

            SaldoDisplay     = BudgetService.FormatRsd(summary.Saldo);
            DonationsDisplay = BudgetService.FormatRsd(summary.TotalDonations);
            ExpensesDisplay  = BudgetService.FormatRsd(summary.TotalExpenses);

            var cats = _budgetService.GetCategoryBudgets(
                _selectedDistrict.Id, _selectedPeriod.Start, _selectedPeriod.End);
            CategoryCards.Clear();
            foreach (var cat in cats)
                CategoryCards.Add(new BudgetCategoryCardViewModel(cat, OpenCategoryDetail));
        }
        catch (Exception ex)
        {
            LoadError = $"Greška: {ex.Message}\n\nProvjerite da li je pokrenuta migracija migration_budget.sql.";
        }
    }

    private void BuildPeriods()
    {
        int year = DateTime.Now.Year;
        for (int y = year; y >= year - 1; y--)
        {
            for (int q = 4; q >= 1; q--)
            {
                int m = (q - 1) * 3 + 1;
                Periods.Add(new PeriodOption
                {
                    Label = $"Q{q} {y}",
                    Start = new DateTime(y, m, 1),
                    End   = new DateTime(y, m + 2, DateTime.DaysInMonth(y, m + 2))
                });
            }
        }
        int curQ = (DateTime.Now.Month - 1) / 3 + 1;
        foreach (var p in Periods)
            if (p.Label == $"Q{curQ} {year}") { _selectedPeriod = p; break; }
        OnPropertyChanged(nameof(SelectedPeriod));
    }
}
