using AutoMapper;
using crud_dotnet_api.Data;
using crud_dotnet_api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Text;
using Microsoft.Extensions.Hosting;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Update.Internal;


namespace crud_dotnet_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        //public static User user = new User();
        private readonly EmployeeService _employeeService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public EmployeeController(EmployeeService employeeService, IConfiguration configuration, IMapper mapper
            , IWebHostEnvironment environment)
        {
            _employeeService = employeeService;
            _mapper = mapper;
            _configuration = configuration;
            _environment = environment;
        }

        //Logging In

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDto userDto)
        {
            var person = await _employeeService.GetPersonByIdAndName(userDto.Username, userDto.Password);

            if (person == null)
            {
                return Unauthorized("Invalid login credentials");
            }

            var token = GenerateJwtToken(person);

            return Ok(new { token , person });
        }
        private string GenerateJwtToken(Employee employee)
        {
            var claims = new[]
            {
     new Claim(JwtRegisteredClaimNames.Sub, employee.GuidId.ToString()),
     new Claim(JwtRegisteredClaimNames.Name, employee.Name),
      new Claim(ClaimTypes.Role, employee.Role)
};

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AppSettings:Token"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
          issuer: null,
          audience: null,
          claims: claims,
          expires: DateTime.Now.AddMinutes(60),
          signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //Getting Data


        [HttpGet, Authorize]
        public async Task<ActionResult<List<EmployeeDto>>> GetEmployees()
        {
            var employees = await _employeeService.GetEmployeesAsync();
            var employeeDtos = _mapper.Map<List<EmployeeDto>>(employees);
            return Ok(employeeDtos);
        }


        //Getting data by Id


        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeDto>> GetEmployee(Guid id)
        {
            var employee = await _employeeService.GetEmployeeAsync(id);
            if (employee == null)
                return NotFound();

            var employeeDto = _mapper.Map<EmployeeDto>(employee);
            return Ok(employeeDto);
        }


       
        //Updaing Image



        [HttpPut("{id}/photo")]
        public async Task<IActionResult> UpdatePhoto(Guid id, IFormFile imageFile)
        {
            var (success, imageUrl) = await _employeeService.UpdateEmployeePhotoAsync(id, imageFile);

            if (success)
            {
                return Ok(new { success = true, imageUrl = imageUrl });
            }
            else
            {
                return BadRequest(new { success = false, message = "Failed to update photo" });
            }
        }



        //Adding Image


        [HttpPost]
public async Task<ActionResult<EmployeeDto>> CreateEmployee([FromForm] Employee employee, IFormFile imageFile, [FromForm] List<Qualification> qualifications)
{
    try
    {
        var result = await _employeeService.CreateEmployeeAsync(employee, imageFile, qualifications);
        return CreatedAtAction(nameof(GetEmployee), new { id = result.GuidId }, result);
    }
    catch (Exception)
    {
        // Log the exception
        return StatusCode(500, "An error occurred while creating the employee.");
    }
}

        //Update Data


        [HttpPut("{id}"), Authorize(Roles = "Administrator")]
     
        public async Task<IActionResult> UpdateEmployee(Guid id, EmployeeDto employeeDto)
        {
            // Check if the ID in the URL matches the ID in the DTO
            if (id != employeeDto.GuidId)
            {
                return BadRequest(new { error = "ID mismatch: The provided ID in the URL does not match the ID in the request body." });
            }

            // Map the DTO to the Employee entity
            var employee = _mapper.Map<Employee>(employeeDto);

            // Attempt to update the employee
            var result = await _employeeService.UpdateEmployeeAsync(id, employee);

            // Check if the update was successful
            if (!result)
            {
                return NotFound(new { error = "Employee not found: The specified employee ID does not exist." });
            }

            return Ok("Updated Successfully");
        }


        //Deleting Data

        [HttpDelete("{id}"), Authorize]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            var result = await _employeeService.DeleteEmployeeAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }

}
