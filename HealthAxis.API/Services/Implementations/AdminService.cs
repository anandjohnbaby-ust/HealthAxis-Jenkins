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
using HealthAxis.Shared.DTOs.PatientDtos;
using HealthAxis.Shared.Enums;
using Microsoft.AspNetCore.Identity;

namespace HealthAxis.API.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IDoctorService _doctorService;
        private readonly IPatientService _patientService;
        private readonly IAppointmentService _appointmentService;
        private readonly IAdminRepository _adminRepository;

        

        public AdminService(
            IDoctorService doctorService,
            IPatientService patientService,
            IAppointmentService appointmentService,
            IAdminRepository adminRepository
            )
        {
            _doctorService = doctorService;
            _patientService = patientService;
            _appointmentService = appointmentService;
            _adminRepository = adminRepository;
        }

        #region Doctor

        // Get Doctor By ID
        public async Task<DoctorDto> GetDoctorById(int id)
        {
            return await _doctorService.GetDoctorById(id);
        }

        // Get All Doctors (Search, filter)
        public async Task<PagedResult<DoctorDto>> GetDoctorsAsync(
            PaginationRequest request,
            Specialisation? specialisation,
            string? search,
            CancellationToken ct = default)
        {
            return await _doctorService.GetDoctorsAsync(
                request,
                specialisation,
                search,
                ct);
        }

        // Create Doctor
        public async Task<DoctorDto> CreateDoctor(CreateDoctorDto dto)
        {
            return await _doctorService.CreateDoctor(dto);
        }

        // Update Doctor
        public async Task<DoctorDto> UpdateDoctor(
            int doctorId,
            UpdateDoctorDto dto)
        {
            return await _doctorService.UpdateDoctor(doctorId, dto);
        }

        #endregion

        #region Patient

        // Get All Patient (Search)
        public async Task<PagedResult<PatientDto>> GetPatientsAsync(
            PaginationRequest request,
            string? search,
            CancellationToken ct = default)
        {
            return await _patientService.GetPatientsAsync(
                request,
                search,
                ct);
        }

        // Update Patient
        public async Task<PatientDto> UpdatePatientAsync(
            int id,
            UpdatePatientDto dto,
            CancellationToken ct = default)
        {
            return await _patientService.UpdateAsync(
                id,
                dto,
                ct);
        }

        #endregion

        #region Appointment

        public async Task<PagedResult<AppointmentReportDto>> GetAppointmentReport(
            PaginationRequest request)
        {
            return await _appointmentService.GetAppointmentReportAsync(request);
        }

        #endregion

        #region DashBoard

        public async Task<DashboardDto> GetDashboardAsync()
        {
            return await _adminRepository.GetDashboardAsync();
        }

        #endregion
    }
}
