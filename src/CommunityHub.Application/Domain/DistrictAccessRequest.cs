using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CommunityHub.Application.Domain
{
    public class DistrictAccessRequest
    {
        public long Id { get; private set; }
        public User User { get; private set; }
        public District District { get; private set; }
        public DateTime CreationDate { get; private set; }
        public DistrictAccessRequestStatus Status { get; private set; }

        public DistrictAccessRequest(long id, User user, District district, DateTime creationDate, DistrictAccessRequestStatus status)
        {
            Id = id;
            User = user;
            District = district;
            CreationDate = creationDate;
            Status = status;
        }

        public DistrictAccessRequest(User user, District district)
        {
            Id = 0;
            User = user;
            District = district;
            CreationDate = DateTime.Now;
            Status = DistrictAccessRequestStatus.Pending;
        }

        public void Accept()
        {
            Status = DistrictAccessRequestStatus.Accepted;
        }
        
        public void Reject()
        {
            Status = DistrictAccessRequestStatus.Rejected;
        }
    }
}
