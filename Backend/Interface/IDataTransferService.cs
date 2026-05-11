using AI_stats_measurement.Backend.Dto;

namespace AI_stats_measurement.Backend.Services;

public interface IDataTransferService
{
    Task<DataExportBundleDto> ExportAsync();
    Task<DataExportAllBundleDto> ExportAllAsync();
    Task ImportAsync(DataExportBundleDto bundle);
}
