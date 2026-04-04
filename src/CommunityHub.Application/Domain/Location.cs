using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityHub.Application.Domain;

public class Location
{
    public long Id { get; private set; }
    public string City { get; private set; }
    public string Country { get; private set; }

    public Location(long id, string city, string country)
    {
        Id = id;
        City = city;
        Country = country;
    }

    public Location(string city, string country)
    {
        Id = 0;
        City = city;
        Country = country;
    }
}
