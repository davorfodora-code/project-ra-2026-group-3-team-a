using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Database.Repositories.DistrictCoordinator;

internal static class BudgetCategoryCatalog
{
    private static readonly (long Id, string Name)[] Categories =
    [
        (1, "Uredjenje ulica"),
        (2, "Uredjenje parkova"),
        (3, "Rasveta"),
        (4, "Cistoca"),
        (5, "Zelenilo"),
        (6, "Hitne popravke")
    ];

    public static IReadOnlyList<BudgetCategory> All =>
        Categories.Select(c => new BudgetCategory(c.Id, c.Name)).ToList();

    public static IReadOnlyList<long> AllIds =>
        Categories.Select(c => c.Id).ToList();

    public static string? ResolveName(long categoryId) =>
        Categories.FirstOrDefault(c => c.Id == categoryId).Name;

    public static string CategoriesSqlValues => string.Join(" UNION ALL ", Categories.Select(c =>
        $"SELECT {c.Id} AS id, '{c.Name}' AS name"));
}
