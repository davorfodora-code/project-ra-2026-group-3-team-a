using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityHub.Application;
using CommunityHub.Application.Services.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class ForumViewModel : ViewModelBase
{
    private readonly ForumService _forumService;
    private string _searchText = string.Empty;
    private ForumDto? _selectedForum;
    private string _newCommentText = string.Empty;
    private bool _isDetailView;
    private ViewModelBase? _activeDialog;

    public ForumViewModel(long coordinatorId)
    {
        CoordinatorId = coordinatorId;
        _forumService  = new ForumService(Injector.Get<IForumRepository>());

        Forums   = new ObservableCollection<ForumDto>();
        Comments = new ObservableCollection<ForumCommentDto>();

        SearchCommand    = new RelayCommand(_ => LoadForums());
        OpenForumCommand = new RelayCommand(p => { if (p is ForumDto dto) OpenForumDetail(dto); });
        BackToListCommand  = new RelayCommand(_ => BackToList());
        CloseForumCommand  = new RelayCommand(_ => DoCloseForum(),
            _ => SelectedForum?.IsOwner == true && !SelectedForum.IsClosed);
        PostCommentCommand = new RelayCommand(_ => DoPostComment(),
            _ => SelectedForum != null && !SelectedForum.IsClosed && !string.IsNullOrWhiteSpace(_newCommentText));
        LikeCommand    = new RelayCommand(p => { if (p is ForumCommentDto c) DoToggleReaction(c.Id, true); });
        DislikeCommand = new RelayCommand(p => { if (p is ForumCommentDto c) DoToggleReaction(c.Id, false); });
        OpenCreateForumCommand = new RelayCommand(_ => OpenCreateForumDialog());
    }

    public long CoordinatorId { get; }

    public ObservableCollection<ForumDto>        Forums   { get; }
    public ObservableCollection<ForumCommentDto> Comments { get; }

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public ForumDto? SelectedForum
    {
        get => _selectedForum;
        private set
        {
            if (SetProperty(ref _selectedForum, value))
            {
                OnPropertyChanged(nameof(CanClose));
                OnPropertyChanged(nameof(CanComment));
                OnPropertyChanged(nameof(IsForumClosed));
                OnPropertyChanged(nameof(ForumClosedLabel));
            }
        }
    }

    public string NewCommentText
    {
        get => _newCommentText;
        set => SetProperty(ref _newCommentText, value);
    }

    public bool IsDetailView
    {
        get => _isDetailView;
        private set { if (SetProperty(ref _isDetailView, value)) OnPropertyChanged(nameof(IsListView)); }
    }

    public bool   IsListView       => !_isDetailView;
    public bool   CanClose         => _selectedForum?.IsOwner == true && !(_selectedForum?.IsClosed ?? true);
    public bool   CanComment       => !(_selectedForum?.IsClosed ?? true);
    public bool   IsForumClosed    => _selectedForum?.IsClosed ?? false;
    public string ForumClosedLabel => IsForumClosed ? " - zaključani" : string.Empty;

    public ViewModelBase? ActiveDialog
    {
        get => _activeDialog;
        private set { if (SetProperty(ref _activeDialog, value)) OnPropertyChanged(nameof(HasDialog)); }
    }
    public bool HasDialog => _activeDialog != null;

    public void ForceCloseDialog() => ActiveDialog = null;

    public ICommand SearchCommand          { get; }
    public ICommand OpenForumCommand       { get; }
    public ICommand BackToListCommand      { get; }
    public ICommand CloseForumCommand      { get; }
    public ICommand PostCommentCommand     { get; }
    public ICommand LikeCommand            { get; }
    public ICommand DislikeCommand         { get; }
    public ICommand OpenCreateForumCommand { get; }

    public event Action<string, bool>? NotificationRequested;

    public void Initialize()
    {
        try   { LoadForums(); }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Forum] Initialize error for user {CoordinatorId}: {ex.Message}");
            NotificationRequested?.Invoke(
                $"Greška pri učitavanju foruma:\n{ex.Message}\n\nProverite da li je pokrenuta migracija (migration_forum_34.sql).", false);
        }
    }

    private void OpenCreateForumDialog()
    {
        var vm = new CreateForumDialogViewModel(CoordinatorId);
        vm.CloseRequested += success =>
        {
            ActiveDialog = null;
            if (success) LoadForums();
        };
        ActiveDialog = vm;
    }

    private void LoadForums()
    {
        List<ForumDto> forums = string.IsNullOrWhiteSpace(_searchText)
            ? _forumService.GetVisibleForums(CoordinatorId)
            : _forumService.Search(CoordinatorId, _searchText);

        Forums.Clear();
        foreach (ForumDto f in forums) Forums.Add(f);
    }

    private void OpenForumDetail(ForumDto forum)
    {
        SelectedForum = _forumService.GetById(forum.Id, CoordinatorId);
        if (SelectedForum == null) return;
        LoadComments();
        IsDetailView = true;
    }

    private void BackToList()
    {
        IsDetailView   = false;
        SelectedForum  = null;
        Comments.Clear();
        NewCommentText = string.Empty;
        LoadForums();
    }

    private void LoadComments()
    {
        if (SelectedForum == null) return;
        List<ForumCommentDto> comments = _forumService.GetComments(
            SelectedForum.Id, CoordinatorId, SelectedForum.AuthorId);
        Comments.Clear();
        foreach (ForumCommentDto c in comments) Comments.Add(c);
    }

    private void DoCloseForum()
    {
        if (SelectedForum == null) return;
        try
        {
            _forumService.CloseForum(SelectedForum.Id);
            SelectedForum = _forumService.GetById(SelectedForum.Id, CoordinatorId);
            LoadComments();
            NotificationRequested?.Invoke("Forum je uspešno zatvoren.", true);
        }
        catch (Exception ex) { NotificationRequested?.Invoke($"Greška: {ex.Message}", false); }
    }

    private void DoPostComment()
    {
        if (SelectedForum == null || string.IsNullOrWhiteSpace(_newCommentText)) return;
        try
        {
            _forumService.AddComment(SelectedForum.Id, CoordinatorId, _newCommentText.Trim());
            NewCommentText = string.Empty;
            SelectedForum  = _forumService.GetById(SelectedForum.Id, CoordinatorId);
            LoadComments();
        }
        catch (Exception ex) { NotificationRequested?.Invoke($"Greška: {ex.Message}", false); }
    }

    private void DoToggleReaction(long commentId, bool isPositive)
    {
        try
        {
            _forumService.ToggleReaction(commentId, CoordinatorId, isPositive);
            LoadComments();
        }
        catch (Exception ex) { NotificationRequested?.Invoke($"Greška: {ex.Message}", false); }
    }
}
