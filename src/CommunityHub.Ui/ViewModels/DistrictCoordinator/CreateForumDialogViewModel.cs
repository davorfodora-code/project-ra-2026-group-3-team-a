using System;
using System.Windows.Input;
using CommunityHub.Application;
using CommunityHub.Application.Services.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class CreateForumDialogViewModel : ViewModelBase
{
    private readonly ForumService _service;
    private readonly long _coordinatorId;

    private string _title = string.Empty;
    private string _content = string.Empty;
    private string _titleError = string.Empty;
    private string _contentError = string.Empty;

    public CreateForumDialogViewModel(long coordinatorId)
    {
        _coordinatorId = coordinatorId;
        _service = new ForumService(Injector.Get<IForumRepository>());

        ConfirmCommand = new RelayCommand(_ => OnConfirm());
        CancelCommand = new RelayCommand(_ => CloseRequested?.Invoke(false));
    }

    public string Title
    {
        get => _title;
        set { if (SetProperty(ref _title, value)) ValidateTitle(); }
    }

    public string Content
    {
        get => _content;
        set { if (SetProperty(ref _content, value)) ValidateContent(); }
    }

    public string TitleError
    {
        get => _titleError;
        private set { if (SetProperty(ref _titleError, value)) OnPropertyChanged(nameof(HasTitleError)); }
    }
    public bool HasTitleError => !string.IsNullOrEmpty(_titleError);

    public string ContentError
    {
        get => _contentError;
        private set { if (SetProperty(ref _contentError, value)) OnPropertyChanged(nameof(HasContentError)); }
    }
    public bool HasContentError => !string.IsNullOrEmpty(_contentError);

    public ICommand ConfirmCommand { get; }
    public ICommand CancelCommand { get; }

    public event Action<bool>? CloseRequested;

    private void ValidateTitle()
    {
        TitleError = string.IsNullOrWhiteSpace(_title) ? "Naziv ne sme biti prazan." : string.Empty;
    }

    private void ValidateContent()
    {
        string text = _content.Trim();
        ContentError = string.IsNullOrWhiteSpace(text)
            ? "Sadržaj je obavezna stavka."
            : text.Length < 30
                ? "Sadržaj mora imati najmanje 30 karaktera."
                : string.Empty;
    }

    private void OnConfirm()
    {
        ValidateTitle();
        ValidateContent();
        if (HasTitleError || HasContentError) return;

        try
        {
            _service.Create(new CreateForumDto
            {
                Title = _title.Trim(),
                Content = _content.Trim(),
                CoordinatorId = _coordinatorId
            });
            CloseRequested?.Invoke(true);
        }
        catch (Exception ex)
        {
            ContentError = $"Greška: {ex.Message}";
        }
    }
}
