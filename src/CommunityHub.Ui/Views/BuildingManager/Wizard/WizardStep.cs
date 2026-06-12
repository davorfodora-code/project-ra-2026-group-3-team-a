namespace CommunityHub.Ui.Views.BuildingManager.Wizard;

// Jedan korak u Pokretnom vodiču. Korak može da bude:
//  • Welcome / Finish modal (TargetName = null, NavTabIndex = null) — centrirana kartica
//  • Highlight koraka (TargetName = ime XAML elementa u nav baru ili page-u)
public class WizardStep
{
    public required string Title       { get; init; }
    public required string Description { get; init; }

    // Ako je postavljeno, controller prvo navigira na taj tab pre prikaza koraka
    public int? NavTabIndex { get; init; }

    // x:Name elementa koji se highlightuje (u BuildingManagerWindow-u ili u Page-u)
    // Null = nema highlight-a (welcome / finish modal)
    public string? TargetName { get; init; }
}
