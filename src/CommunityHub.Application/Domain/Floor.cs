using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityHub.Application.Domain;

public class Floor
{
    public long Id { get; private set; }
    public int FloorNumber { get; private set; }
    public Building? Building { get; internal set; }
    public List<Apartment> Apartments { get; private set; }

    public Floor(long id, int floorNumber)
    {
        Id = id;
        FloorNumber = floorNumber;
        Building = null;
        Apartments = new List<Apartment>();
    }

    public Floor(int floorNumber)
    {
        Id = 0;
        FloorNumber = floorNumber;
        Building = null;
        Apartments = new List<Apartment>();
    }

    public void AddApartment(Apartment apartment)
    {
        Apartments.Add(apartment);
        apartment.Floor = this;
    }
}