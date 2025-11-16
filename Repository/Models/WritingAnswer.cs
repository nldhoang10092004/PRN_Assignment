using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class WritingAnswer
{
    public int Id { get; set; }

    public int QuestionId { get; set; }

    public string? Content { get; set; }

    public decimal? Grade { get; set; }

    public string? Feedback { get; set; }

    public virtual WritingQuestion Question { get; set; } = null!;
}
