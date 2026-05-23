namespace FisaActivitateZilnicaApi.Schedules.Services.FetParser;

/// <summary>
/// Parsed data from a .fet XML file, ready for import into the database.
/// </summary>
public sealed class FetParsedData
{
    public required IReadOnlyList<FetDay> Days { get; init; }
    public required IReadOnlyList<FetHour> Hours { get; init; }
    public required IReadOnlyList<FetSubject> Subjects { get; init; }
    public required IReadOnlyList<FetActivityTag> ActivityTags { get; init; }
    public required IReadOnlyList<FetTeacher> Teachers { get; init; }
    public required IReadOnlyList<FetYear> Years { get; init; }
    public required IReadOnlyList<FetBuilding> Buildings { get; init; }
    public required IReadOnlyList<FetRoom> Rooms { get; init; }
    public required IReadOnlyList<FetActivity> Activities { get; init; }
    /// <summary>Timetable from ConstraintActivityPreferredStartingTime: FET Activity_Id -> (Day, Hour).</summary>
    public required IReadOnlyList<FetActivitySlot> ActivitySlots { get; init; }
    /// <summary>Room binding from ConstraintActivityPreferredRoom: FET Activity_Id -> Room name.</summary>
    public required IReadOnlyList<FetActivityRoom> ActivityRooms { get; init; }
}

public sealed record FetActivitySlot(int FetActivityId, string DayName, string HourName);

public sealed record FetActivityRoom(int FetActivityId, string RoomName);

public sealed record FetDay(string Name);

public sealed record FetHour(string Name);

public sealed record FetSubject(string Name, string? Comments);

public sealed record FetActivityTag(string Name, bool Printable, string? Comments);

public sealed record FetTeacher(string Name, int TargetNumberOfHours, string? Comments);

public sealed record FetYear(
    string Name,
    int NumberOfStudents,
    string? Comments,
    IReadOnlyList<FetGroup> Groups
);

public sealed record FetGroup(
    string Name,
    int NumberOfStudents,
    string? Comments,
    IReadOnlyList<FetSubgroup> Subgroups
);

public sealed record FetSubgroup(string Name, int NumberOfStudents, string? Comments);

public sealed record FetBuilding(string Name, string? Comments);

public sealed record FetRoom(
    string Name,
    string? BuildingName,
    int? Capacity,
    bool Virtual,
    string? Comments
);

public sealed record FetActivity(
    int FetId,
    string TeacherName,
    string SubjectName,
    string? ActivityTagName,
    string Students,
    int Duration,
    int TotalDuration,
    int ActivityGroupId,
    bool Active,
    string? Comments,
    IReadOnlyList<FetActivityCommentRef> CommentRefs
);

public sealed record FetActivityCommentRef(
    int? TeacherExternalId,
    int? PlanMatterProviderExternalId,
    int? FacultyExternalId,
    int? MetaSpecializationExternalId,
    int? StudyYearNumber,
    int? Semester,
    int? PlataNB,
    string? OldActivityTag,
    string? ActivityTag,
    string? GroupExternalId,
    string? GroupNames,
    int? SpecializationExternalId,
    int? SubjectExternalId
);
