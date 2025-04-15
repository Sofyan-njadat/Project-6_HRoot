using System;
using System.Collections.Generic;

namespace BIGMVC_project.Models;

public partial class Mission
{
    public int Id { get; set; }

    public string? TaskName { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? TasksStatusEnum { get; set; }

    public int EmployeeId { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
