using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityHub.Application.Domain;

public class District
{
    public long Id { get; private set; }
    public string Name { get; private set; }

    public District(long id, string name)
    {
        Id = id;
        Name = name;
    }

    public District(string name)
    {
        Id = 0;
        Name = name;
    }
}