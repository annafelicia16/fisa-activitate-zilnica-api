namespace FisaActivitateZilnicaApi.ScheduleCatalog.DTOs.Responses;

// Cascade dropdown options, all scoped to one teacher's schedule and read 100%
// from the local DB (names cached at import). Id is the AGSIS external id used as
// the next level's filter; GroupKey is the resolved group name (or "-" for a
// whole-class slot); rooms have no external id so they're name-only.
public sealed record CatalogItemResponse(int Id, string Name);

public sealed record CatalogYearResponse(int Year);

public sealed record CatalogGroupResponse(string GroupKey, string Name);

public sealed record CatalogRoomResponse(string Name);

public sealed record CatalogSubjectResponse(string Name);
