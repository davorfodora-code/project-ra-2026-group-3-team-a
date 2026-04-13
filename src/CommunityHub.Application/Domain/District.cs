using System;
using System.Collections.Generic;

namespace CommunityHub.Application.Domain;

public class District
{
    public long Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Location Location { get; private set; }
    public List<DistrictStreet> Streets { get; private set; }
    public List<Image> Images { get; private set; }
    public decimal Budget { get; private set; }
    public User Coordinator { get; private set; }

    public District(long id, string name)
    {
        Id = id;
        Name = name;
        Description = string.Empty;
        Location = null!;
        Streets = new List<DistrictStreet>();
        Images = new List<Image>();
        Budget = 0;
        Coordinator = null!;
    }

    public District(string name)
    {
        Id = 0;
        Name = name;
        Description = string.Empty;
        Location = null!;
        Streets = new List<DistrictStreet>();
        Images = new List<Image>();
        Budget = 0;
        Coordinator = null!;
    }

    public District(long id, string name, string description, Location location, decimal budget, User coordinator)
    {
        Id = id;
        Name = name;
        Description = description;
        Location = location;
        Streets = new List<DistrictStreet>();
        Images = new List<Image>();
        Budget = budget;
        Coordinator = coordinator;
    }

    public District(string name, string description, Location location, decimal budget, User coordinator)
    {
        Id = 0;
        Name = name;
        Description = description;
        Location = location;
        Streets = new List<DistrictStreet>();
        Images = new List<Image>();
        Budget = budget;
        Coordinator = coordinator;
    }

    public void AddStreet(DistrictStreet street)
    {
        Streets.Add(street);
        street.District = this;
    }

    public void AddImage(Image image)
    {
        Images.Add(image);
        image.District = this;
    }
}
