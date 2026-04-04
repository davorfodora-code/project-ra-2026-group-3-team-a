using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityHub.Application.Domain;

public class BuildingImage
{
    public long Id { get; private set; }
    public string ImagePath { get; private set; }
    public Building? Building { get; internal set; }

    public BuildingImage(long id, string imagePath)
    {
        Id = id;
        ImagePath = imagePath;
        Building = null;
    }

    public BuildingImage(string imagePath)
    {
        Id = 0;
        ImagePath = imagePath;
        Building = null;
    }
}
