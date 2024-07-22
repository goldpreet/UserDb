using crud_dotnet_api.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace crud_dotnet_api
{
    public class QualificationDto
    {
        [Key]
        public Guid Id { get; set; }
        public string QualificationName { get; set; }
        public int Experience { get; set; }
        public string Institution { get; set; }


        public Guid EmployeeGuidId { get; set; }

        [ForeignKey("EmployeeGuidId")]
        public Employee? Employee { get; set; }
    }
}