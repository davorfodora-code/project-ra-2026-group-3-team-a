using System;

namespace CommunityHub.Application.Domain;

public class DistrictMembership
{
    public long Id { get; private set; }
    public District District { get; private set; }
    public User User { get; private set; }
    public DateTime JoinDate { get; private set; }

    public DistrictMembership(long id, District district, User user, DateTime joinDate)
    {
        Id = id;
        District = district;
        User = user;
        JoinDate = joinDate;
    }

    public DistrictMembership(District district, User user)
    {
        Id = 0;
        District = district;
        User = user;
        JoinDate = DateTime.Now;
    }
}
