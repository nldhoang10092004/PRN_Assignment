using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class SpeakingAnswer
{
    public int Id { get; set; }

    public int QuestionId { get; set; }

    public string? Transcript { get; set; }

    public string? AudioUrl { get; set; }

    public decimal? Grade { get; set; }

    public string? Feedback { get; set; }

    public virtual SpeakingQuestion Question { get; set; } = null!;
}
