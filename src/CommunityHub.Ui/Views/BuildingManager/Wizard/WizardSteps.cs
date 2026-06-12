using System.Collections.Generic;

namespace CommunityHub.Ui.Views.BuildingManager.Wizard;

// Definicija svih koraka u Pokretnom vodiču za upravnika zgrade.
// Ako bude potrebno dodati/ukloniti korak — samo se menja ova lista.
public static class WizardSteps
{
    public static List<WizardStep> All => new()
    {
        new WizardStep
        {
            Title       = "Dobro došli u CommunityHub",
            Description = "Aplikacija koja vam pomaže da efikasno upravljate zgradama. " +
                          "Pokazaćemo vam najvažnije delove. " +
                          "Vodič možete preskočiti u svakom trenutku.",
            // welcome modal — bez highlight-a
        },
        new WizardStep
        {
            Title       = "Zgrade",
            Description = "Ovde se nalaze sve zgrade kojima upravljate. " +
                          "Možete videti detalje, stanare, prostorije i probleme za svaku. " +
                          "Dugme + dodaje novu zgradu.",
            NavTabIndex = 1,
            TargetName  = "NavZgrade",
        },
        new WizardStep
        {
            Title       = "Zahtevi",
            Description = "Pregled zahteva stanara za pristup zgradama " +
                          "i zahteva za iznajmljivanje zajedničkih prostorija. " +
                          "Možete ih odobriti ili odbiti.",
            NavTabIndex = 2,
            TargetName  = "NavZahtevi",
        },
        new WizardStep
        {
            Title       = "Skupštine",
            Description = "Zakazujete skupštine stanara i upravljate temama dnevnog reda. " +
                          "Stanari mogu da predlažu dodatne teme do 24h pre skupštine.",
            NavTabIndex = 3,
            TargetName  = "NavSkupstine",
        },
        new WizardStep
        {
            Title       = "Više",
            Description = "Dodatne opcije: podešavanja aplikacije, " +
                          "ponovo pokretanje ovog vodiča i odjava.",
            NavTabIndex = 4,
            TargetName  = "NavVise",
        },
        new WizardStep
        {
            Title       = "Početna",
            Description = "Brzi pregled — predstojeća skupština, broj zahteva, " +
                          "nerešenih problema i aktivnih oglasa. " +
                          "Odavde brzo navigirate do svih delova aplikacije.",
            NavTabIndex = 0,
            TargetName  = "NavPocetna",
        },
        new WizardStep
        {
            Title       = "Spreman za rad!",
            Description = "Vodič možete ponovo pokrenuti u svakom trenutku iz tab-a " +
                          "\"Više\" → Pokretni vodič. Srećan rad!",
            // finish modal
        },
    };
}
