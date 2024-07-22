using AutoMapper;
using crud_dotnet_api.Data;

namespace crud_dotnet_api
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Map Employee to EmployeeDto and reverse
            CreateMap<Employee, EmployeeDto>().ReverseMap();

            // Map Qualification to QualificationDto and reverse
            CreateMap<Qualification, QualificationDto>().ReverseMap();
        }
    }
}
