using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class SpeakingQuestion
{
    public int QuestionId { get; set; }

    public string Content { get; set; } = null!;

    public virtual ICollection<SpeakingAnswer> SpeakingAnswers { get; set; } = new List<SpeakingAnswer>();
}
