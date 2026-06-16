using System;
using System.Collections.Generic;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

// Stavka u dropdown-u meseci. Null Month = "Svi meseci" (cela godina).
public record MonthOption(string Label, int? Month)
{
    public static List<MonthOption> All()
    {
        string[] meseci = ["Januar", "Februar", "Mart", "April", "Maj", "Jun",
                           "Jul", "Avgust", "Septembar", "Oktobar", "Novembar", "Decembar"];
        var list = new List<MonthOption> { new MonthOption("Svi meseci", null) };
        for (int m = 1; m <= 12; m++)
            list.Add(new MonthOption(meseci[m - 1], m));
        return list;
    }
}

// Lista godina za dropdown — tekuca + 2 prethodne
public static class YearOptions
{
    public static List<int> Recent(int howMany = 3)
    {
        var list = new List<int>();
        int year = DateTime.Today.Year;
        for (int i = 0; i < howMany; i++) list.Add(year - i);
        return list;
    }
}
