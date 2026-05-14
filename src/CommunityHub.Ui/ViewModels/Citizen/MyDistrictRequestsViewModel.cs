using CommunityHub.Application;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Application.Services.Citizen;
using CommunityHub.Ui.Views.Citizen;

namespace CommunityHub.Ui.ViewModels.Citizen;

public class MyDistrictRequestsViewModel
{
    private readonly long _userId;
    private readonly ICitizenAccessRequestService _service;
    private List<DistrictAccessRequestDto> _allRequests = new();

    public MyDistrictRequestsViewModel(long userId)
    {
        _userId = userId;
        _service = Injector.Get<ICitizenAccessRequestService>();
    }

    public void LoadRequests()
    {
        _allRequests = _service.GetAllByUserId(_userId);
    }

    public List<DistrictAccessRequestRowViewModel> GetFiltered(DistrictAccessRequestStatus? status, bool newestFirst)
    {
        IEnumerable<DistrictAccessRequestDto> filtered = _allRequests;

        if (status.HasValue)
        {
            filtered = filtered.Where(r => r.Status == status.Value);
        }

        filtered = newestFirst
            ? filtered.OrderByDescending(r => r.CreatedAt)
            : filtered.OrderBy(r => r.CreatedAt);

        return filtered.Select(r => new DistrictAccessRequestRowViewModel(r)).ToList();
    }

    public AccessResultDto Delete(long requestId)
    {
        return _service.DeleteRequest(_userId, requestId);
    }
}
