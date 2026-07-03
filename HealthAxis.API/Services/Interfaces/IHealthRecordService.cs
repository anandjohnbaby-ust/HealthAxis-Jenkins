using HealthAxis.API.Models;
using HealthAxis.Shared.DTOs.HealthRecordDtos;

namespace HealthAxis.API.Services.Interfaces
{
    public interface IHealthRecordService
    {

        Task<HealthRecordDto>
            GetByRecordIdAsync(
                int id,
                CancellationToken ct = default);

        Task<HealthRecordDto>
            AddAsync(
                CreateHealthRecordDto dto,
                CancellationToken ct = default);

    }
}