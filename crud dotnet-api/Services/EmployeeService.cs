#pragma warning disable

using AutoMapper;
using crud_dotnet_api.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace crud_dotnet_api.Services
{
    public class EmployeeService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public EmployeeService(AppDbContext appDbContext, IConfiguration configuration, IMapper mapper, IWebHostEnvironment environment)
        {
            _appDbContext = appDbContext;
            _configuration = configuration;
            _mapper = mapper;
            _environment = environment;
        }

        // Log In
        public async Task<Employee?> GetPersonByIdAndName(string name, string password)
        {
            return await _appDbContext.Employees
                .Include(e => e.Qualifications) // Include qualifications if needed
                .SingleOrDefaultAsync(x => x.Name == name && x.Password == password);
        }

        private string GenerateJwtToken(Employee employee)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, employee.GuidId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, employee.Name),
                new Claim(ClaimTypes.Role, "admin")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AppSettings:Token"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Get All Employees
        public async Task<List<Employee>> GetEmployeesAsync()
        {
            return await _appDbContext.Employees
                .Include(e => e.Qualifications)
                .ToListAsync();
        }

        // Get Employee By Id
        public async Task<Employee?> GetEmployeeAsync(Guid id)
        {
            return await _appDbContext.Employees
                .Include(e => e.Qualifications)
                .FirstOrDefaultAsync(e => e.GuidId == id);
        }

        // Update Image
        public async Task<(bool Success, string? ImageUrl)> UpdateEmployeePhotoAsync(Guid id, IFormFile imageFile)
        {
            var employee = await _appDbContext.Employees.FirstOrDefaultAsync(e => e.GuidId == id);
            if (employee == null)
            {
                return (false, null);
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "Upload");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            var imageUrl = $"/Upload/{fileName}";
            employee.ImageUrl = imageUrl;
            _appDbContext.Employees.Update(employee);
            await _appDbContext.SaveChangesAsync();

            return (true, imageUrl);
        }

        // Create Employee
        public async Task<EmployeeDto> CreateEmployeeAsync(Employee employee, IFormFile? imageFile, List<Qualification> qualifications)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "Upload");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }
                employee.ImageUrl = $"/Upload/{fileName}";
            }

            employee.Qualifications = qualifications;
            foreach (var qualification in qualifications)
            {
                qualification.EmployeeGuidId = employee.GuidId;
            }

            _appDbContext.Employees.Add(employee);
            await _appDbContext.SaveChangesAsync();

            return _mapper.Map<EmployeeDto>(employee);
        }

        // Update Employee
        public async Task<bool> UpdateEmployeeAsync(Guid id, Employee updatedEmployee)
        {
            var existingEmployee = await _appDbContext.Employees
                .Include(e => e.Qualifications)
                .FirstOrDefaultAsync(e => e.GuidId == id);

            if (existingEmployee == null)
                return false;

            _appDbContext.Entry(existingEmployee).CurrentValues.SetValues(updatedEmployee);

            foreach (var qualification in updatedEmployee.Qualifications)
            {
                var existingQualification = existingEmployee.Qualifications
                    .FirstOrDefault(q => q.Id == qualification.Id);

                if (existingQualification != null)
                {
                    _appDbContext.Entry(existingQualification).CurrentValues.SetValues(qualification);
                }
                else
                {
                    existingEmployee.Qualifications.Add(qualification);
                }
            }

            await _appDbContext.SaveChangesAsync();
            return true;
        }

        // Delete Employee
        public async Task<bool> DeleteEmployeeAsync(Guid id)
        {
            var employee = await _appDbContext.Employees
                .Include(e => e.Qualifications)
                .FirstOrDefaultAsync(e => e.GuidId == id);

            if (employee == null)
                return false;

            _appDbContext.Employees.Remove(employee);
            await _appDbContext.SaveChangesAsync();
            return true;
        }
    }
}
