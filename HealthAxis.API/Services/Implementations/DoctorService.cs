using AutoMapper;
using HealthAxis.API.Data;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Implementations;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Interfaces;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.AdminDtos;
using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.Shared.DTOs.DoctorDtos;
using HealthAxis.Shared.DTOs.HealthRecordDtos;
using HealthAxis.Shared.DTOs.PatientDtos;
using HealthAxis.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HealthAxis.API.Services.Implementations
{
    public class DoctorService : IDoctorService 
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHealthRecordRepository _healthRecordRepository;
        private readonly IAppointmentService _appointmentService;
        private readonly IHealthRecordService _healthRecordService;

        public DoctorService(
            IDoctorRepository repository,
            IHealthRecordRepository healthRecordRepository,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IMapper mapper,
            IAppointmentService appointmentService,
            IHealthRecordService healthRecordService)
        {
            _doctorRepository = repository;
            _healthRecordRepository = healthRecordRepository;
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
            _appointmentService = appointmentService;
            _healthRecordService = healthRecordService;
        }

        // Get the Doctor By ID
        public async Task<DoctorDto> GetDoctorById(int id)
        {
            var doctor = await _doctorRepository.GetByIdAsync(id);

            if (doctor == null)
                throw new NotFoundException("Doctor not found.");

            return _mapper.Map<DoctorDto>(doctor);
        }

        // Get All Doctors(search by Id,Name,Email, Filter By Specialization)
        public async Task<PagedResult<DoctorDto>> GetDoctorsAsync(
            PaginationRequest request,
            Specialisation? specialisation,
            string? search,
            CancellationToken ct = default)
        {
            var pagedDoctors = await _doctorRepository.GetDoctorsAsync(
                request,
                specialisation,
                search,
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
            var existingUser =
                await _userManager.FindByEmailAsync(dto.Email);

            if (existingUser != null)
            {
                throw new BusinessRuleException(
                    "Email already exists.");
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email
                };

                var result =
                    await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                {
                    throw new BusinessRuleException(
                        string.Join(", ",
                            result.Errors.Select(e => e.Description)));
                }

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

                _context.Doctors.Add(doctor);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return _mapper.Map<DoctorDto>(doctor);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
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


        public async Task<IEnumerable<DoctorDto>> GetAvailableDoctorsAsync(
            Specialisation? specialisation,
            string? search,
            CancellationToken ct = default)
        {
            var doctors = await _doctorRepository.GetAvailableDoctorsAsync(
                specialisation,
                search,
                ct);

            return _mapper.Map<IEnumerable<DoctorDto>>(doctors);
        }

        // Doctor portal
        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsAsync(
            string userId,
            CancellationToken ct)
        {
            var doctor = await _doctorRepository.GetByUserIdAsync(userId, ct);

            if (doctor == null)
                throw new NotFoundException("Doctor not found.");

            var appointments = await _doctorRepository
                .GetAppointmentsAsync(doctor.DoctorId, ct);

            return _mapper.Map<IEnumerable<AppointmentDto>>(appointments);
        }

        public async Task<IEnumerable<AppointmentDto>> GetTodaysAppointmentsAsync(
            string userId,
            CancellationToken ct = default)
        {
            var doctor = await _doctorRepository.GetByUserIdAsync(userId, ct);

            if (doctor == null)
                throw new NotFoundException("Doctor not found.");

            var appointments = await _doctorRepository
                .GetTodaysAppointmentsAsync(
                    doctor.DoctorId,
                    DateTime.Today,
                    ct);

            return _mapper.Map<IEnumerable<AppointmentDto>>(appointments);
        }

        public async Task<IEnumerable<AppointmentDto>> GetWeeklyAppointmentsAsync(
            string userId,
            CancellationToken ct = default)
        {
            var doctor = await _doctorRepository.GetByUserIdAsync(userId, ct);

            if (doctor == null)
                throw new NotFoundException("Doctor not found.");

            DateTime today = DateTime.Today;

            int diff = today.DayOfWeek == DayOfWeek.Sunday
                ? 6
                : (int)today.DayOfWeek - 1;

            DateTime startOfWeek = today.AddDays(-diff);
            DateTime endOfWeek = startOfWeek.AddDays(6);

            var appointments = await _doctorRepository
                .GetWeeklyAppointmentsAsync(
                    doctor.DoctorId,
                    startOfWeek,
                    endOfWeek,
                    ct);

            return _mapper.Map<IEnumerable<AppointmentDto>>(appointments);
        }

        public async Task<AppointmentDto> UpdateAppointmentStatusAsync(
            int appointmentId,
            UpdateAppointmentStatusDto dto,
            CancellationToken ct = default)
        {
            return await _appointmentService.UpdateStatusAsync(
                appointmentId,
                dto,
                ct);
        }


        public async Task<HealthRecordDto> AddHealthRecordAsync(
            CreateHealthRecordDto dto,
            CancellationToken ct = default)
        {
            return await _healthRecordService.AddAsync(
                dto,
                ct);
        }

        public async Task<HealthRecordDto> GetHealthRecordByIdAsync(
            int id,
            CancellationToken ct = default)
        {
            return await _healthRecordService.GetByRecordIdAsync(
                id,
                ct);
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
                    "Doctor profile not found.");
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
                    "Doctor profile not found.");
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
                throw new NotFoundException("Doctor not found.");

            return dashboard;
        }
    }
}