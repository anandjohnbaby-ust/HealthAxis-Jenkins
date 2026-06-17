using AutoMapper;
using HealthAxis.API.DTOs.AppointmentDtos;
using HealthAxis.API.DTOs.DoctorDtos;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Interfaces;

namespace HealthAxis.API.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IMapper _mapper;

        public AdminService(
            IDoctorRepository doctorRepository,
            IAppointmentRepository appointmentRepository,
            IMapper mapper)
        {
            _doctorRepository = doctorRepository;
            _appointmentRepository = appointmentRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DoctorDto>> GetDoctors()
        {
            var doctors =
                await _doctorRepository.GetAllAsync();

            return _mapper.Map<IEnumerable<DoctorDto>>(doctors);
        }

        public async Task<DoctorDto> CreateDoctor(
            CreateDoctorDto dto)
        {
            var doctor =
                _mapper.Map<Doctor>(dto);

            await _doctorRepository.AddAsync(doctor);

            return _mapper.Map<DoctorDto>(doctor);
        }

        public async Task<DoctorDto> UpdateDoctor(
            int doctorId,
            UpdateDoctorDto dto)
        {
            var doctor =
                await _doctorRepository.GetByIdAsync(doctorId);

            if (doctor is null)
            {
                throw new NotFoundException(
                    $"Doctor with id {doctorId} not found");
            }

            _mapper.Map(dto, doctor);

            await _doctorRepository.UpdateAsync(
                doctorId,
                doctor);

            return _mapper.Map<DoctorDto>(doctor);
        }

        public async Task<IEnumerable<AppointmentDto>>
            GetAppointmentReport()
        {
            var appointments =
                await _appointmentRepository.GetAllAsync();

            return _mapper.Map<IEnumerable<AppointmentDto>>(
                appointments);
        }
    }

}
