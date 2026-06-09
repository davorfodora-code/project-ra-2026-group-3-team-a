using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using CommunityHub.Ui.Settings;

namespace CommunityHub.Ui.Views.BuildingManager.Wizard;

// Orkestrator wizard-a — drzi listu koraka, navigira na odgovarajuci tab
// pre prikaza highlight koraka, i pamti zavrsetak u AppSettings.
public class WizardController
{
    private readonly BuildingManagerWindow _window;
    private readonly WizardOverlay _overlay;
    private readonly List<WizardStep> _steps;
    private int _currentIndex;

    public WizardController(BuildingManagerWindow window, WizardOverlay overlay)
    {
        _window  = window;
        _overlay = overlay;
        _steps   = WizardSteps.All;

        _overlay.OnNext     = NextStep;
        _overlay.OnPrevious = PreviousStep;
        _overlay.OnSkip     = SkipTour;
    }

    public void Start()
    {
        _currentIndex = 0;
        _overlay.Visibility = Visibility.Visible;
        ShowCurrentStep();
    }

    private async void ShowCurrentStep()
    {
        WizardStep step = _steps[_currentIndex];
        bool isFinal    = _currentIndex == _steps.Count - 1;

        // Welcome i Finish — centriran modal bez highlight-a
        if (step.TargetName == null)
        {
            _overlay.ShowCenterModal(step, _currentIndex, _steps.Count, isFinal);
            return;
        }

        // Ako je potrebno, prebaci se na odgovarajuci tab pre prikaza highlight-a
        if (step.NavTabIndex.HasValue)
            _window.NavigateToTab(step.NavTabIndex.Value);

        // Kratko sacekaj da se layout updateuje (page se ucita, nav se osvezi)
        await Task.Delay(120);

        FrameworkElement? target = _window.FindName(step.TargetName) as FrameworkElement;
        _overlay.ShowHighlight(target!, step, _currentIndex, _steps.Count);
    }

    private void NextStep()
    {
        if (_currentIndex >= _steps.Count - 1)
        {
            FinishTour();
            return;
        }
        _currentIndex++;
        ShowCurrentStep();
    }

    private void PreviousStep()
    {
        if (_currentIndex == 0) return;   // sigurnosna provera — Back je sakriven na welcome-u
        _currentIndex--;
        ShowCurrentStep();
    }

    private void SkipTour()
    {
        AppSettings.WizardCompleted = true;
        AppSettings.Save();
        Hide();
    }

    private void FinishTour()
    {
        AppSettings.WizardCompleted = true;
        AppSettings.Save();
        Hide();
    }

    private void Hide()
    {
        _overlay.Visibility = Visibility.Collapsed;
    }
}
