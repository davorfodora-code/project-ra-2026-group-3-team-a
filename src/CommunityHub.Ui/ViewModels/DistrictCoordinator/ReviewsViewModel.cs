using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityHub.Application;
using CommunityHub.Application.Services.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class ReviewsViewModel : ViewModelBase
{
    private readonly ReviewService              _reviewService;
    private readonly DistrictAccessRequestService _districtService;
    private readonly long _coordinatorId;
    private DistrictDto? _selectedDistrict;
    private string _averageDisplay  = "—";
    private string _averageStars    = "☆☆☆☆☆";
    private int    _totalCount;

    public ReviewsViewModel(long coordinatorId)
    {
        _coordinatorId  = coordinatorId;
        _reviewService  = new ReviewService(Injector.Get<IReviewRepository>());
        _districtService = new DistrictAccessRequestService(
            Injector.Get<IDistrictAccessRequestRepository>(),
            Injector.Get<IDistrictRepository>());

        Districts = new ObservableCollection<DistrictDto>();
        Reviews   = new ObservableCollection<ReviewDto>();
    }

    public ObservableCollection<DistrictDto> Districts { get; }
    public ObservableCollection<ReviewDto>   Reviews   { get; }

    public DistrictDto? SelectedDistrict
    {
        get => _selectedDistrict;
        set
        {
            if (SetProperty(ref _selectedDistrict, value) && value != null)
                LoadReviews(value.Id);
        }
    }

    public string AverageDisplay
    {
        get => _averageDisplay;
        private set => SetProperty(ref _averageDisplay, value);
    }

    public string AverageStars
    {
        get => _averageStars;
        private set => SetProperty(ref _averageStars, value);
    }

    public int TotalCount
    {
        get => _totalCount;
        private set => SetProperty(ref _totalCount, value);
    }

    public void EnsureLoaded()
    {
        if (Districts.Count > 0) return;
        try
        {
            var districts = _districtService.GetDistrictsByCoordinatorId(_coordinatorId);
            foreach (var d in districts)
                Districts.Add(d);

            if (Districts.Count > 0)
                SelectedDistrict = Districts[0];
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Reviews] Load error: {ex.Message}");
        }
    }

    private void LoadReviews(long districtId)
    {
        try
        {
            var list = _reviewService.GetReviewsForLastYear(districtId);

            Reviews.Clear();
            foreach (var r in list)
                Reviews.Add(r);

            TotalCount = list.Count;

            if (list.Count > 0)
            {
                double avg = list.Average(r => r.Rating);
                int    rounded = (int)Math.Round(avg);
                AverageDisplay = $"{avg:0.0}/5";
                AverageStars   = new string('★', rounded) + new string('☆', 5 - rounded);
            }
            else
            {
                AverageDisplay = "—";
                AverageStars   = "☆☆☆☆☆";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Reviews] LoadReviews error: {ex.Message}");
        }
    }
}
