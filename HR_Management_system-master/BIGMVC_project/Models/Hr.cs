using System;
using System.Collections.Generic;

namespace BIGMVC_project.Models;

public partial class Hr
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? PasswordHash { get; set; }

    public string? Image { get; set; }
}
