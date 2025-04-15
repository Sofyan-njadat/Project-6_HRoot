using System;
using System.Collections.Generic;

namespace BIGMVC_project.Models;

public partial class Employee
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? PasswordHash { get; set; }

    public string? ImagePath { get; set; }

    public string? Address { get; set; }

    public string? Position { get; set; }

    public int? ManagerId { get; set; }

    public int? DepartmentId { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Department? Department { get; set; }

    public virtual ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();

    public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();

    public virtual Manager? Manager { get; set; }

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
