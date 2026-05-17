using System;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

internal sealed class RegistrationOverlayController
{
    private readonly long _coordinatorId;
    private readonly Action<ViewModelBase?> _setActive;
    private readonly Action _onRefresh;
    private readonly Action<string> _onNotify;
    private DistrictRegistrationViewModel? _current;

    internal RegistrationOverlayController(
        long coordinatorId,
        Action<ViewModelBase?> setActive,
        Action onRefresh,
        Action<string> onNotify)
    {
        _coordinatorId = coordinatorId;
        _setActive     = setActive;
        _onRefresh     = onRefresh;
        _onNotify      = onNotify;
    }

    internal void Open()
    {
        _current = new DistrictRegistrationViewModel(_coordinatorId);
        _current.RegistrationCompleted += OnCompleted;
        _current.CancelRequested       += Close;
        _current.NotificationRequested += (m, _) => _onNotify(m);
        _setActive(_current);
    }

    internal void Close()
    {
        if (_current == null) return;
        _current.RegistrationCompleted -= OnCompleted;
        _current.CancelRequested       -= Close;
        _setActive(null);
        _current = null;
    }

    private void OnCompleted()
    {
        Close();
        _onRefresh();
    }
}
