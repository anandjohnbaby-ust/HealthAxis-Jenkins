using AutoMapper;
using HealthAxis.API.Data;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Interfaces;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AdminDtos;
using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.Shared.DTOs.DoctorDtos;
using HealthAxis.Shared.DTOs.HealthRecordDtos;
using HealthAxis.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace HealthAxis.API.Services.Implementations
{
    public class DoctorService : IDoctorService 
    {

        private const string DoctorNotFoundMessage = "Doctor not found.";
        private const string DoctorProfileNotFoundMessage = "Doctor profile not found.";

        private readonly IDistributedCache _cache;
        private readonly ILogger<DoctorService> _logger;
        private readonly IDoctorRepository _doctorRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IHealthRecordRepository _healthRecordRepository;

        public DoctorService(
            IDoctorRepository repository,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            IAppointmentRepository appointmentRepository,
            IHealthRecordRepository healthRecordRepository,
            IDistributedCache cache,
            ILogger<DoctorService> logger)
        {
            _doctorRepository = repository;
            _userManager = userManager;
            _mapper = mapper;
            _appointmentRepository = appointmentRepository;
            _healthRecordRepository = healthRecordRepository;
            _cache = cache;
            _logger = logger;
        }

        // Get the Doctor By ID
        public async Task<DoctorDto> GetDoctorById(int id)
        {
            var doctor = await _doctorRepository.GetByIdAsync(id);

            if (doctor == null)
                throw new NotFoundException(DoctorNotFoundMessage);

            return _mapper.Map<DoctorDto>(doctor);
        }

        // Get All Doctors(search by Id,Name,Email, Filter By Specialization)
        public async Task<PagedResult<DoctorDto>> GetDoctorsAsync(
            PaginationRequest request,
            Specialisation? specialisation,
            string? search,
            bool? isActive,
            CancellationToken ct = default)
        {
            var pagedDoctors = await _doctorRepository.GetDoctorsAsync(
                request,
                specialisation,
                search,
                isActive,
                ct);

            return new PagedResult<DoctorDto>
            {
                Items = _mapper.Map<IEnumerable<DoctorDto>>(pagedDoctors.Items),
                TotalCount = pagedDoctors.TotalCount,
                PageNumber = pagedDoctors.PageNumber,
                PageSize = pagedDoctors.PageSize
            };
        }

        // Create Doctor
        public async Task<DoctorDto> CreateDoctor(CreateDoctorDto dto)
        {
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                throw new BusinessRuleException("Email already exists.");

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                throw new BusinessRuleException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, "Doctor");

            var doctor = new Doctor
            {
                UserId = user.Id,
                FullName = dto.FullName,
                Specialisation = dto.Specialisation,
                YearsOfExperience = dto.YearsOfExperience,
                ConsultationFee = dto.ConsultationFee,
                IsActive = true
            };

            await _doctorRepository.CreateDoctorAsync(doctor);

            return _mapper.Map<DoctorDto>(doctor);
        }

        // Update Doctor
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


        public async Task<PagedResult<DoctorDto>> GetAvailableDoctorsAsync(
            Specialisation? specialisation,
            string? search,
            PaginationRequest request,
            CancellationToken ct = default)
        {
            var result = await _doctorRepository.GetAvailableDoctorsAsync(
                specialisation,
                search,
                request,
                ct);

            return new PagedResult<DoctorDto>
            {
                Items = _mapper.Map<List<DoctorDto>>(result.Items),
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
        }

        // Doctor portal
        public async Task<PagedResult<AppointmentDto>> GetAppointmentsAsync(
            string userId,
            PaginationRequest request,
            string? search = null,
            AppointmentStatus? status = null,
            DateTime? date = null,
            CancellationToken ct = default)
        {
            var doctor = await _doctorRepository.GetByUserIdAsync(userId, ct);

            if (doctor == null)
            {
                throw new NotFoundException(DoctorNotFoundMessage);
            }

            var pagedAppointments = await _doctorRepository.GetAppointmentsAsync(
                doctor.DoctorId,
                request,
                search,
                status,
                date,
                ct);

            return new PagedResult<AppointmentDto>
            {
                Items = _mapper.Map<List<AppointmentDto>>(pagedAppointments.Items),
                TotalCount = pagedAppointments.TotalCount,
                PageNumber = pagedAppointments.PageNumber,
                PageSize = pagedAppointments.PageSize
            };
        }

        public async Task<PagedResult<AppointmentDto>> GetTodaysAppointmentsAsync(
           string userId,
           PaginationRequest request,
           string? search = null,
           AppointmentStatus? status = null,
           CancellationToken ct = default)
        {
            var doctor = await _doctorRepository.GetByUserIdAsync(userId, ct);

            if (doctor == null)
            {
                throw new NotFoundException(DoctorNotFoundMessage);
            }

            var pagedAppointments = await _doctorRepository.GetTodaysAppointmentsAsync(
                doctor.DoctorId,
                DateTime.Today,
                request,
                search,
                status,
                ct);

            return new PagedResult<AppointmentDto>
            {
                Items = _mapper.Map<List<AppointmentDto>>(pagedAppointments.Items),
                TotalCount = pagedAppointments.TotalCount,
                PageNumber = pagedAppointments.PageNumber,
                PageSize = pagedAppointments.PageSize
            };
        }
        public async Task<PagedResult<AppointmentDto>> GetWeeklyAppointmentsAsync(
             string userId,
             PaginationRequest request,
             string? search = null,
             AppointmentStatus? status = null,
             DateTime? date = null,
             CancellationToken ct = default)
        {
            var doctor = await _doctorRepository.GetByUserIdAsync(userId, ct);

            if (doctor == null)
            {
                throw new NotFoundException(DoctorNotFoundMessage);
            }

            DateTime today = DateTime.Today;

            int diff = today.DayOfWeek == DayOfWeek.Sunday
                ? 6
                : (int)today.DayOfWeek - 1;

            DateTime startOfWeek = today.AddDays(-diff);
            DateTime endOfWeek = startOfWeek.AddDays(6);

            var pagedAppointments = await _doctorRepository.GetWeeklyAppointmentsAsync(
                doctor.DoctorId,
                startOfWeek,
                endOfWeek,
                request,
                search,
                status,
                date,
                ct);

            return new PagedResult<AppointmentDto>
            {
                Items = _mapper.Map<List<AppointmentDto>>(pagedAppointments.Items),
                TotalCount = pagedAppointments.TotalCount,
                PageNumber = pagedAppointments.PageNumber,
                PageSize = pagedAppointments.PageSize
            };
        }

        public async Task<DoctorDto> GetDoctorByUserIdAsync(
            string userId,
            CancellationToken ct = default)
        {
            var doctor = await _doctorRepository.GetByUserIdAsync(
                userId,
                ct);

            if (doctor is null)
            {
                throw new NotFoundException(
                    DoctorProfileNotFoundMessage);
            }

            return _mapper.Map<DoctorDto>(doctor);
        }

        public async Task<DoctorDto> UpdateDoctorByUserIdAsync(
            string userId,
            UpdateDoctorDto dto,
            CancellationToken ct = default)
        {
            var doctor = await _doctorRepository.GetByUserIdAsync(
                userId,
                ct);

            if (doctor is null)
            {
                throw new NotFoundException(
                    DoctorProfileNotFoundMessage);
            }

            _mapper.Map(dto, doctor);

            await _doctorRepository.UpdateAsync(
                doctor.DoctorId,
                doctor,
                ct);

            return _mapper.Map<DoctorDto>(doctor);
        }

        public async Task<DoctorDashboardDto> GetDashboardAsync(
            int doctorId,
            CancellationToken ct = default)
        {
            var dashboard = await _doctorRepository.GetDashboardAsync(
                doctorId,
                ct);

            if (dashboard is null)
                throw new NotFoundException(DoctorNotFoundMessage);

            return dashboard;
        }


        public async Task<IEnumerable<HealthRecordDto>> GetPatientHealthHistoryAsync(
            int patientId,
            int appointmentId,
            CancellationToken ct = default)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(
                appointmentId,
                ct);

            if (appointment == null)
            {
                throw new NotFoundException(
                    $"Appointment with ID {appointmentId} was not found.");
            }

            if (appointment.PatientId != patientId)
            {
                throw new BusinessRuleException(
                    "The appointment does not belong to the specified patient.");
            }

            var records = await _healthRecordRepository
                .GetPatientHealthRecordsAsync(
                    patientId,
                    ct);

            return _mapper.Map<IEnumerable<HealthRecordDto>>(records);
        }
    }
}