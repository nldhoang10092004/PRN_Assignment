using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class Apikey
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? DeepgramKey { get; set; }

    public string? ChatGptkey { get; set; }

    public virtual Account User { get; set; } = null!;
}
