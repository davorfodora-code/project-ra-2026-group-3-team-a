namespace CommunityHub.Ui.ViewModels.BuildingManager;

public record PeriodOption(string Label, int Year, int? Month)
{
    public static List<PeriodOption> GenerateList()
    {
        var periods = new List<PeriodOption>();
        int year  = DateTime.Today.Year;
        int month = DateTime.Today.Month;
        string[] meseci = ["Januar", "Februar", "Mart", "April", "Maj", "Jun",
                           "Jul", "Avgust", "Septembar", "Oktobar", "Novembar", "Decembar"];
        for (int m = month; m >= 1; m--)
            periods.Add(new PeriodOption($"{meseci[m - 1]} {year}", year, m));
        periods.Add(new PeriodOption(year.ToString(), year, null));
        periods.Add(new PeriodOption((year - 1).ToString(), year - 1, null));
        for (int m = 12; m >= 1; m--)
            periods.Add(new PeriodOption($"{meseci[m - 1]} {year - 1}", year - 1, m));
        return periods;
    }
}
