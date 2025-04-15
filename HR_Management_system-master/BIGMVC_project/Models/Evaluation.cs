using System;
using System.Collections.Generic;

namespace BIGMVC_project.Models;

public partial class Evaluation
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int ManagerId { get; set; }

    public string? EvaluationsStatusEnum { get; set; }

    public string? Comments { get; set; }

    public DateOnly? DateEvaluated { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Manager Manager { get; set; } = null!;
}
