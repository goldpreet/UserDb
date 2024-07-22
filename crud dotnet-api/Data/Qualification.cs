using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace crud_dotnet_api.Data
{

    public class Qualification
    {
        [Key]
        public Guid Id { get; set; }
        public string QualificationName { get; set; } = string.Empty;
        public int Experience { get; set; }
        public string Institution { get; set; } = string.Empty ;

        [ForeignKey("EmployeeGuidId")]
        public Guid EmployeeGuidId { get; set; }

       
        //public Employee? Employee { get; set; }
    } 
}
