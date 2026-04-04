using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityHub.Application.Domain;
 
public class Address
{
    public long Id { get; private set; }
    public string Street { get; private set; }
    public string StreetNumber { get; private set; }
 
    public Address(long id, string street, string streetNumber)
    {
        Id = id;
        Street = street;
        StreetNumber = streetNumber;
    }
 
    public Address(string street, string streetNumber)
    {
        Id = 0;
        Street = street;
        StreetNumber = streetNumber;
    }
}
 