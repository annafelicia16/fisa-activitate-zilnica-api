using FisaActivitateZilnicaApi.Schedules.DTOs.Payloads;

namespace FisaActivitateZilnicaApi.Schedules.Services.Interfaces;

public interface IFetImportService
{
    Task<FetImportResult> ImportAsync(
        Stream oddWeekFetStream,
        Stream evenWeekFetStream,
        string name,
        int year,
        int semester,
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct = default
    );
}
