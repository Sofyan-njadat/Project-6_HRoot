using System;
using System.Collections.Generic;

namespace BIGMVC_project.Models;

public partial class Department
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<Manager> Managers { get; set; } = new List<Manager>();
}
