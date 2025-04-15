using System;
using System.Collections.Generic;

namespace BIGMVC_project.Models;

public partial class Manager
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? PasswordHash { get; set; }

    public int? DepartmentId { get; set; }

    public string? Image { get; set; }

    public virtual Department? Department { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
}
