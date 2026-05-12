using CommunityHub.Application.Database.Repositories.Citizen;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Dto.Citizen;

namespace CommunityHub.Application.Services.Citizen;

public class CitizenAccessRequestService
{
    private readonly CitizenDistrictDbRepository _districtRepository;
    private readonly CitizenDistrictAccessRequestDbRepository _requestRepository;
    private readonly CitizenUserProfileDbRepository _userProfileRepository;
    private readonly CitizenMembershipService _membershipService;

    public CitizenAccessRequestService()
    {
        _districtRepository = new CitizenDistrictDbRepository();
        _requestRepository = new CitizenDistrictAccessRequestDbRepository();
        _userProfileRepository = new CitizenUserProfileDbRepository();
        _membershipService = new CitizenMembershipService();
    }

    public AccessResultDto RequestDistrictAccess(long userId, long districtId)
    {
        District? district = _districtRepository.GetAll().FirstOrDefault(d => d.Id == districtId);
        if (district == null)
        {
            return Error("Selektovani kvart nije pronađen.");
        }

        if (_membershipService.HasMembership(userId))
        {
            long? existingDistrictId = _membershipService.GetMembershipDistrictId(userId);
            if (existingDistrictId == districtId)
            {
                return Warning("Već ste član ovog kvarta.");
            }

            return Warning("Već ste član nekog kvarta. Ne možete tražiti pristup drugom kvartu.");
        }

        string? rawAddress = _userProfileRepository.GetAddressByUserId(userId);
        if (string.IsNullOrWhiteSpace(rawAddress))
        {
            return Error("Nije moguće pročitati adresu stanovanja iz profila.");
        }

        if (!TryParseResidenceStreetAndNumber(rawAddress, out string streetName, out int streetNumber))
        {
            return Error("Adresa stanovanja nije u očekivanom formatu (npr. 'Zemunska 10, Novi Sad').");
        }

        if (district.ContainsAddress(streetName, streetNumber))
        {
            _membershipService.CreateMembership(userId, districtId);
            return Success("Adresa se poklapa sa kvartom — automatski ste učlanjeni.");
        }

        if (_requestRepository.ExistsPending(userId, districtId))
        {
            return Warning("Već imate aktivan zahtev za ovaj kvart.");
        }

        _requestRepository.Create(userId, districtId);
        return Success("Kreiran je zahtev za pristup kvartu (status: Pending).");
    }

    public List<DistrictAccessRequestDto> GetAllByUserId(long userId)
    {
        return _requestRepository.GetAllByUserId(userId)
            .Select(MapToDto)
            .ToList();
    }

    public AccessResultDto DeleteRequest(long userId, long requestId)
    {
        DistrictAccessRequest? request = _requestRepository.GetAllByUserId(userId)
            .FirstOrDefault(r => r.Id == requestId);
        if (request == null)
        {
            return Error("Zahtev nije pronađen.");
        }

        if (!request.CanBeCancelledByCitizen())
        {
            return Warning("Možete obrisati samo zahteve u statusu Pending.");
        }

        _requestRepository.Delete(requestId);
        return Success("Zahtev je uspešno obrisan.");
    }

    private static DistrictAccessRequestDto MapToDto(DistrictAccessRequest request)
    {
        return new DistrictAccessRequestDto
        {
            RequestId = request.Id,
            DistrictName = request.District.Name,
            CreatedAt = request.CreationDate,
            Status = request.Status,
            CanDelete = request.CanBeCancelledByCitizen()
        };
    }

    private static bool TryParseResidenceStreetAndNumber(string rawAddress, out string streetName, out int streetNumber)
    {
        streetName = string.Empty;
        streetNumber = 0;

        string streetLine = TakeStreetSegmentBeforeComma(rawAddress).Trim().TrimEnd('.', ',', ';');
        int lastSpace = streetLine.LastIndexOf(' ');
        if (lastSpace <= 0)
        {
            return false;
        }

        string lastToken = streetLine[(lastSpace + 1)..].Trim();
        if (!int.TryParse(lastToken, out streetNumber) || streetNumber <= 0)
        {
            streetNumber = 0;
            return false;
        }

        streetName = streetLine[..lastSpace].Trim();
        return !string.IsNullOrWhiteSpace(streetName);
    }

    private static string TakeStreetSegmentBeforeComma(string rawAddress)
    {
        string trimmed = rawAddress.Trim();
        int commaIndex = trimmed.IndexOf(',');
        return commaIndex > 0 ? trimmed[..commaIndex].Trim() : trimmed;
    }

    private static AccessResultDto Success(string message) => new() { Type = AccessResultType.Success, Message = message };
    private static AccessResultDto Warning(string message) => new() { Type = AccessResultType.Warning, Message = message };
    private static AccessResultDto Error(string message) => new() { Type = AccessResultType.Error, Message = message };
}
