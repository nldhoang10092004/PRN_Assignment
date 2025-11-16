using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class UserDetail
{
    public int UserId { get; set; }

    public string? FullName { get; set; }

    public DateOnly? Dob { get; set; }

    public string? Address { get; set; }

    public string? AvatarUrl { get; set; }

    public virtual Account User { get; set; } = null!;
}
