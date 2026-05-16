using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityHub.Application;
using CommunityHub.Application.Services.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class ForumViewModel : ViewModelBase
{
    private readonly ForumService _forumService;
    public long CoordinatorId { get; }
    private readonly long _coordinatorId;

    private string _searchText = string.Empty;
    private ForumDto? _selectedForum;
    private string _newCommentText = string.Empty;
    private bool _isDetailView;

    public ForumViewModel(long coordinatorId)
    {
        _coordinatorId = coordinatorId;
        CoordinatorId = coordinatorId;
        _forumService = new ForumService(Injector.Get<IForumRepository>());

        Forums = new ObservableCollection<ForumDto>();
        Comments = new ObservableCollection<ForumCommentDto>();

        SearchCommand = new RelayCommand(_ => DoSearch());
        OpenForumCommand = new RelayCommand(p =>
        {
            if (p is ForumDto dto) OpenForumDetail(dto);
        });
        BackToListCommand = new RelayCommand(_ => BackToList());
        CloseForumCommand = new RelayCommand(_ => DoCloseForum(), _ => SelectedForum?.IsOwner == true && !SelectedForum.IsClosed);
        PostCommentCommand = new RelayCommand(_ => DoPostComment(), _ => CanPostComment());
        LikeCommand = new RelayCommand(p =>
        {
            if (p is ForumCommentDto c) DoToggleReaction(c.Id, true);
        });
        DislikeCommand = new RelayCommand(p =>
        {
            if (p is ForumCommentDto c) DoToggleReaction(c.Id, false);
        });
    }

    public ObservableCollection<ForumDto> Forums { get; }
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
        private set
        {
            if (SetProperty(ref _isDetailView, value))
                OnPropertyChanged(nameof(IsListView));
        }
    }

    public bool IsListView => !_isDetailView;
    public bool CanClose => _selectedForum?.IsOwner == true && !(_selectedForum?.IsClosed ?? true);
    public bool CanComment => !(_selectedForum?.IsClosed ?? true);
    public bool IsForumClosed => _selectedForum?.IsClosed ?? false;

    public ICommand SearchCommand { get; }
    public ICommand OpenForumCommand { get; }
    public ICommand BackToListCommand { get; }
    public ICommand CloseForumCommand { get; }
    public ICommand PostCommentCommand { get; }
    public ICommand LikeCommand { get; }
    public ICommand DislikeCommand { get; }

    public event Action<Action<bool>>? CreateForumDialogRequested;
    public event Action<string, bool>? NotificationRequested;

    private ICommand? _openCreateForumCommand;
    public ICommand OpenCreateForumCommand => _openCreateForumCommand ??= new RelayCommand(_ => RequestCreateForum());

    public void Initialize()
    {
        try
        {
            LoadForums();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Forum] Initialize error for user {_coordinatorId}: {ex.Message}");
            NotificationRequested?.Invoke($"Greška pri učitavanju foruma:\n{ex.Message}\n\nProverite da li je pokrenuta migracija (migration_forum_34.sql).", false);
        }
    }

    private void LoadForums()
    {
        List<ForumDto> forums = string.IsNullOrWhiteSpace(_searchText)
            ? _forumService.GetVisibleForums(_coordinatorId)
            : _forumService.Search(_coordinatorId, _searchText);

        Forums.Clear();
        foreach (ForumDto f in forums) Forums.Add(f);
    }

    private void DoSearch()
    {
        LoadForums();
    }

    private void OpenForumDetail(ForumDto forum)
    {
        SelectedForum = _forumService.GetById(forum.Id, _coordinatorId);
        if (SelectedForum == null) return;
        LoadComments();
        IsDetailView = true;
    }

    private void BackToList()
    {
        IsDetailView = false;
        SelectedForum = null;
        Comments.Clear();
        NewCommentText = string.Empty;
        LoadForums();
    }

    private void LoadComments()
    {
        if (SelectedForum == null) return;
        List<ForumCommentDto> comments = _forumService.GetComments(
            SelectedForum.Id, _coordinatorId, SelectedForum.AuthorId);
        Comments.Clear();
        foreach (ForumCommentDto c in comments) Comments.Add(c);
    }

    private void DoCloseForum()
    {
        if (SelectedForum == null) return;
        try
        {
            _forumService.CloseForum(SelectedForum.Id);
            SelectedForum = _forumService.GetById(SelectedForum.Id, _coordinatorId);
            LoadComments();
            NotificationRequested?.Invoke("Forum je uspešno zatvoren.", true);
        }
        catch (Exception ex)
        {
            NotificationRequested?.Invoke($"Greška: {ex.Message}", false);
        }
    }

    private bool CanPostComment()
    {
        return SelectedForum != null && !SelectedForum.IsClosed
               && !string.IsNullOrWhiteSpace(_newCommentText);
    }

    private void DoPostComment()
    {
        if (SelectedForum == null || string.IsNullOrWhiteSpace(_newCommentText)) return;
        try
        {
            _forumService.AddComment(SelectedForum.Id, _coordinatorId, _newCommentText.Trim());
            NewCommentText = string.Empty;
            SelectedForum = _forumService.GetById(SelectedForum.Id, _coordinatorId);
            LoadComments();
        }
        catch (Exception ex)
        {
            NotificationRequested?.Invoke($"Greška: {ex.Message}", false);
        }
    }

    private void DoToggleReaction(long commentId, bool isPositive)
    {
        try
        {
            _forumService.ToggleReaction(commentId, _coordinatorId, isPositive);
            LoadComments();
        }
        catch (Exception ex)
        {
            NotificationRequested?.Invoke($"Greška: {ex.Message}", false);
        }
    }

    private void RequestCreateForum()
    {
        CreateForumDialogRequested?.Invoke(success =>
        {
            if (success) LoadForums();
        });
    }
}
