using AutoMapper;
using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.Shared.DTOs.DoctorDtos;
using HealthAxis.Shared.DTOs.HealthRecordDtos;
using HealthAxis.Shared.DTOs.PatientDtos;
using HealthAxis.API.Models;
using HealthAxis.Shared.DTOs.AdminDtos;

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

            CreateMap<Appointment, AppointmentDto>()
                .ForMember(
                    dest => dest.DoctorName,
                    opt => opt.MapFrom(src => src.Doctor.FullName))
                .ForMember(
                    dest => dest.PatientName,
                    opt => opt.MapFrom(src => src.Patient.FullName))
                .ForMember(
                    dest => dest.HealthRecordId,
                    opt => opt.MapFrom(src =>
                        src.HealthRecord != null
                            ? src.HealthRecord.RecordId
                            : (int?)null));

            // Health Record Mappings
            CreateMap<CreateHealthRecordDto,HealthRecord>();

            CreateMap<HealthRecord, HealthRecordDto>()
            .ForMember(dest => dest.DoctorName,
                opt => opt.MapFrom(src => src.Doctor.FullName));
        }
    }
}