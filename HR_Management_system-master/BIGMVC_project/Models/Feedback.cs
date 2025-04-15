using System;
using System.Collections.Generic;

namespace BIGMVC_project.Models;

public partial class Feedback
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Message { get; set; }

    public DateTime? SubmittedAt { get; set; }
}
