using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityHub.Application.Domain;
 
public class Apartment
{
    public long Id { get; private set; }
    public int ApartmentNumber { get; private set; }
    public Floor? Floor { get; internal set; }
 
    public Apartment(long id, int apartmentNumber)
    {
        Id = id;
        ApartmentNumber = apartmentNumber;
        Floor = null;
    }
 
    public Apartment(int apartmentNumber)
    {
        Id = 0;
        ApartmentNumber = apartmentNumber;
        Floor = null;
    }
}