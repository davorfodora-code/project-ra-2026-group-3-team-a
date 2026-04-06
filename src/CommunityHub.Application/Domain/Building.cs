using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityHub.Application.Domain;

public class Building
{
    public long Id { get; private set; }
    public Address Address { get; private set; }
    public District District { get; private set; }
    public Location Location { get; private set; }
    public int NumberOfFloors { get; private set; }
    public long ManagerId { get; private set; }
    public List<Floor> Floors { get; private set; }
    public List<BuildingImage> Images { get; private set; }
    public int TotalApartments { get; internal set; }

    public Building(long id, Address address, District district, Location location, int numberOfFloors, long managerId)
    {
        Id = id;
        Address = address;
        District = district;
        Location = location;
        NumberOfFloors = numberOfFloors;
        ManagerId = managerId;
        Floors = new List<Floor>();
        Images = new List<BuildingImage>();
    }

    public Building(Address address, District district, Location location, int numberOfFloors, long managerId)
    {
        Id = 0;
        Address = address;
        District = district;
        Location = location;
        NumberOfFloors = numberOfFloors;
        ManagerId = managerId;
        Floors = new List<Floor>();
        Images = new List<BuildingImage>();
    }

    public void AddFloor(Floor floor)
    {
        Floors.Add(floor);
        floor.Building = this;
    }

    public void AddImage(BuildingImage image)
    {
        Images.Add(image);
        image.Building = this;
    }
}