using System;
using System.Collections.Generic;

namespace BIGMVC_project.Models;

public partial class LeaveRequest
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string? Reason { get; set; }

    public string? LeaveRequestsStatusEnum { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public string? LeaveType { get; set; }

    public string? RequestName { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
