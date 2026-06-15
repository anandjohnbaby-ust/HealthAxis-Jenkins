using AutoMapper;
using HealthAxis.API.DTOs.AppointmentDtos;
using HealthAxis.API.DTOs.DoctorDtos;
using HealthAxis.API.DTOs.PatientDtos;
using HealthAxis.API.Models;

namespace HealthAxis.API.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {

            // Patient Mappings
            CreateMap<CreatePatientDto, Patient>();

            CreateMap<UpdatePatientDto, Patient>();

            CreateMap<Patient, PatientDto>()
                .ForMember(
                    dest => dest.Age,
                    opt => opt.MapFrom(src => src.GetAge()));

            // Doctor Mappings
            CreateMap<CreateDoctorDto, Doctor>();

            CreateMap<UpdateDoctorDto, Doctor>();

            CreateMap<Doctor, DoctorDto>()
                .ForMember(
                    dest => dest.UpcomingAppointmentCount,
                    opt => opt.MapFrom(
                        src => src.GetUpcomingAppointmentCount()));

            // Appointment Mappings
            CreateMap<CreateAppointmentDto, Appointment>();

            CreateMap<UpdateAppointmentStatusDto, Appointment>();

            CreateMap<Appointment, AppointmentDto>();

            // Health Record Mappings
            CreateMap<CreateHealthRecordDto,
                HealthRecord>();

            CreateMap<HealthRecord,
                HealthRecordDto>();

            CreateMap<HealthRecord, HealthRecordDto>();

            CreateMap<CreateHealthRecordDto, HealthRecord>();
        }
    }
}