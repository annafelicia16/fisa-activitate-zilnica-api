namespace FisaActivitateZilnicaApi.ExternalReferences.DTOs.Responses;

// Year is the numeric study year (1..6), Name the Roman-numeral label ("I").
public sealed record StudyYearResponse(int Year, string Name);
