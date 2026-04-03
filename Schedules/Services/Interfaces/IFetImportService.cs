using FisaActivitateZilnicaApi.Schedules.DTOs.Payloads;

namespace FisaActivitateZilnicaApi.Schedules.Services.Interfaces;

public interface IFetImportService
{
    Task<FetImportResult> ImportAsync(
        Stream fetStream,
        string name,
        int year,
        int semester,
        bool oddWeek,
        CancellationToken ct = default
    );
}
