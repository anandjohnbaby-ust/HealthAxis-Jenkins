using AutoMapper;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Interfaces;
using HealthAxis.Shared.DTOs.HealthRecordDtos;
using HealthAxis.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace HealthAxis.API.Services.Implementations
{
    public class HealthRecordService :
        IHealthRecordService
    {
        private readonly IHealthRecordRepository
            _healthRecordRepository;

        private readonly IMapper _mapper;

        public HealthRecordService(
            IHealthRecordRepository healthRecordRepository,
            IMapper mapper)
        {
            _healthRecordRepository =
                healthRecordRepository;

            _mapper = mapper;
        }

        public async Task<HealthRecordDto>
            GetByRecordIdAsync(
                int id,
                CancellationToken ct = default)
        {
            var healthRecord =
                await _healthRecordRepository
                    .GetByIdAsync(id, ct);

            if (healthRecord is null)
            {
                throw new NotFoundException(
                    "Health record not found.");
            }

            return _mapper.Map<HealthRecordDto>(
                healthRecord);
        }

    }
}