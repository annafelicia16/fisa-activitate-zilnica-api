namespace FisaActivitateZilnicaApi.ExternalTeachers.DTOs.Responses;

public sealed record TeacherFacultyResponse(
    int IdFacultate,
    string Name,
    string? ShortName
);

public sealed record TeacherDepartmentResponse(
    long IdDepartament,
    string Name,
    string ShortName
);

public sealed record TeacherProfileResponse(
    long IdProfesor,
    string Nume,
    string Prenume,
    string? Email,
    string? Cnp,
    TeacherDepartmentResponse? Department,
    IReadOnlyList<TeacherFacultyResponse> Faculties,
    TeacherFacultyResponse? PrimaryFaculty,
    string? CurrentAcademicYear,
    int? CurrentSemester,
    DateTime? CurrentSemesterStartDate,
    DateTime? CurrentSemesterEndDate
);
