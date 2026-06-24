using AutoMapper;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Interfaces;
using HealthAxis.Shared.Common;
using HealthAxis.Shared.DTOs.DoctorDtos;
using HealthAxis.Shared.Enums;

namespace HealthAxis.API.Services.Implementations
{
    public class DoctorService : IDoctorService
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IMapper _mapper;

        public DoctorService(
            IDoctorRepository repository,
            IMapper mapper)
        {
            _doctorRepository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DoctorDto>> GetAllAsync(
            CancellationToken ct = default)
        {
            var doctors =
                await _doctorRepository.GetAllAsync(ct);

            return _mapper.Map<IEnumerable<DoctorDto>>(doctors);
        }

        public async Task<DoctorDto?> GetByIdAsync(
            int id,
            CancellationToken ct = default)
        {
            var doctor =
                await _doctorRepository.GetByIdAsync(id, ct);

            if (doctor is null)
            {
                throw new NotFoundException("Doctor not found");
            }

            return _mapper.Map<DoctorDto>(doctor);
        }

        public async Task<DoctorDto>
            GetAvailableDoctorByIdAsync(
                int doctorId,
                CancellationToken ct = default)
        {
            var doctor =
                await _doctorRepository
                    .GetAvailableDoctorByIdAsync(
                        doctorId,
                        ct);

            if (doctor is null)
            {
                throw new NotFoundException(
                    "Doctor not found or is not available.");
            }

            return _mapper.Map<DoctorDto>(
                doctor);
        }

        public async Task<IEnumerable<DoctorDto>> FilterBySpecialisationAsync(
                Specialisation? specialisation,
                CancellationToken ct = default)
        {
            var doctors =
                await _doctorRepository.FilterBySpecialisationAsync(
                    specialisation,
                    ct);

            return _mapper.Map<IEnumerable<DoctorDto>>(doctors);
        }

        public async Task<IEnumerable<DoctorDto>> SearchAsync(
            string searchTerm,
            CancellationToken ct = default)
        {
            var doctors =
                await _doctorRepository.SearchAsync(
                    searchTerm,
                    ct);

            return _mapper.Map<IEnumerable<DoctorDto>>(doctors);
        }

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
    }
}