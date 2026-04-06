using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityHub.Application.Domain;

public class Image
{
    public long Id { get; private set; }
    public string Url { get; private set; }
    public District? District { get; internal set; }

    public Image(long id, string url)
    {
        Id = id;
        Url = url;
        District = null;
    }

    public Image(string url)
    {
        Id = 0;
        Url = url;
        District = null;
    }
}
