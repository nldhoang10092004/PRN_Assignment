using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class WritingQuestion
{
    public int Id { get; set; }

    public string Content { get; set; } = null!;

    public virtual ICollection<WritingAnswer> WritingAnswers { get; set; } = new List<WritingAnswer>();
}
