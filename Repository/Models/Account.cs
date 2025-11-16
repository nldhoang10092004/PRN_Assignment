using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class Account
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string HashPass { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Apikey> Apikeys { get; set; } = new List<Apikey>();

    public virtual UserDetail? UserDetail { get; set; }
}
