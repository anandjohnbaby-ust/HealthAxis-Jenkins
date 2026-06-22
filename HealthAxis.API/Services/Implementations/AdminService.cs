using AutoMapper;
using HealthAxis.API.Data;
using HealthAxis.Shared.DTOs.AppointmentDtos;
using HealthAxis.Shared.DTOs.DoctorDtos;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using HealthAxis.Shared.DTOs.AdminDtos;

namespace HealthAxis.API.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IMapper _mapper;

        public AdminService(
            IDoctorRepository doctorRepository,
            IAppointmentRepository appointmentRepository,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _doctorRepository = doctorRepository;
            _appointmentRepository = appointmentRepository;
            _mapper = mapper;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IEnumerable<DoctorDto>> GetDoctors()
        {
            var doctors =
                await _doctorRepository.GetAllAsync();

            return _mapper.Map<IEnumerable<DoctorDto>>(doctors);
        }

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
