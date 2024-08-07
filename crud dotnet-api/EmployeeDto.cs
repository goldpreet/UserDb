﻿using crud_dotnet_api.Data;
using System.ComponentModel.DataAnnotations;

public class EmployeeDto
{


    [Key]
    public Guid GuidId { get; set; }

    public string? Role {  get; set; }
    public string? Name { get; set; }
    public int? Salary { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Gender { get; set; }
    public string? Status { get; set; }
    public int? Age { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? Password { get; set; }

    public List<Qualification>  Qualifications { get; set; }


    public  EmployeeDto()
    {
        Qualifications = new List<Qualification>();
    }

}
