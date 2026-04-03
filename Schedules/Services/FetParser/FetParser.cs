using System.Xml.Linq;
using System.Text.Json;

namespace FisaActivitateZilnicaApi.Schedules.Services.FetParser;

/// <summary>
/// Parses FET timetable (.fet) XML files into structured data for database import.
/// </summary>
public static class FetXmlParser
{
    public static FetParsedData Parse(Stream stream)
    {
        var doc = XDocument.Load(stream);
        var root = doc.Root ?? throw new InvalidOperationException("FET file has no root element.");

        var days = ParseDays(root);
        var hours = ParseHours(root);
        var subjects = ParseSubjects(root);
        var activityTags = ParseActivityTags(root);
        var teachers = ParseTeachers(root);
        var years = ParseYears(root);
        var buildings = ParseBuildings(root);
        var rooms = ParseRooms(root);
        var activities = ParseActivities(root);
        var activitySlots = ParseActivitySlots(root);

        // Ensure all subjects, teachers, and activity tags referenced by activities exist
        var subjectNames = subjects.Select(s => s.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var teacherNames = teachers.Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var tagNames = activityTags.Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var extraSubjects = new List<FetSubject>();
        var extraTeachers = new List<FetTeacher>();
        var extraTags = new List<FetActivityTag>();

        foreach (var a in activities)
        {
            if (!string.IsNullOrEmpty(a.SubjectName) && !subjectNames.Contains(a.SubjectName))
            {
                subjectNames.Add(a.SubjectName);
                extraSubjects.Add(new FetSubject(a.SubjectName, null));
            }
            if (!string.IsNullOrEmpty(a.TeacherName) && !teacherNames.Contains(a.TeacherName))
            {
                teacherNames.Add(a.TeacherName);
                extraTeachers.Add(new FetTeacher(a.TeacherName, 0, null));
            }
            if (!string.IsNullOrEmpty(a.ActivityTagName) && !tagNames.Contains(a.ActivityTagName))
            {
                tagNames.Add(a.ActivityTagName);
                extraTags.Add(new FetActivityTag(a.ActivityTagName, true, null));
            }
        }

        subjects = [.. subjects, .. extraSubjects];
        teachers = [.. teachers, .. extraTeachers];
        activityTags = [.. activityTags, .. extraTags];

        return new FetParsedData
        {
            Days = days,
            Hours = hours,
            Subjects = subjects,
            ActivityTags = activityTags,
            Teachers = teachers,
            Years = years,
            Buildings = buildings,
            Rooms = rooms,
            Activities = activities,
            ActivitySlots = activitySlots,
        };
    }

    private static IReadOnlyList<FetActivitySlot> ParseActivitySlots(XElement root)
    {
        var list = root.Element("Time_Constraints_List");
        if (list == null)
            return [];

        return list.Elements("ConstraintActivityPreferredStartingTime")
            .Select(c =>
            {
                var id = ParseInt(GetElementValue(c, "Activity_Id"), -1);
                var day = GetElementValue(c, "Preferred_Day") ?? "";
                var hour = GetElementValue(c, "Preferred_Hour") ?? "";
                return id >= 0 && !string.IsNullOrWhiteSpace(day) && !string.IsNullOrWhiteSpace(hour)
                    ? new FetActivitySlot(id, day, hour)
                    : null;
            })
            .Where(s => s != null)
            .Cast<FetActivitySlot>()
            .ToList();
    }

    private static IReadOnlyList<FetDay> ParseDays(XElement root)
    {
        var list = root.Element("Days_List");
        if (list == null)
            return [];

        return list.Elements("Day")
            .Select(d => new FetDay(GetElementValue(d, "Name") ?? ""))
            .Where(d => !string.IsNullOrWhiteSpace(d.Name))
            .ToList();
    }

    private static IReadOnlyList<FetHour> ParseHours(XElement root)
    {
        var list = root.Element("Hours_List");
        if (list == null)
            return [];

        return list.Elements("Hour")
            .Select(h => new FetHour(GetElementValue(h, "Name") ?? ""))
            .Where(h => !string.IsNullOrWhiteSpace(h.Name))
            .ToList();
    }

    private static IReadOnlyList<FetSubject> ParseSubjects(XElement root)
    {
        var list = root.Element("Subjects_List");
        if (list == null)
            return [];

        return list.Elements("Subject")
            .Select(s => new FetSubject(
                GetElementValue(s, "Name") ?? "",
                GetElementValue(s, "Comments")
            ))
            .Where(s => !string.IsNullOrWhiteSpace(s.Name))
            .ToList();
    }

    private static IReadOnlyList<FetActivityTag> ParseActivityTags(XElement root)
    {
        var list = root.Element("Activity_Tags_List");
        if (list == null)
            return [];

        return list.Elements("Activity_Tag")
            .Select(t => new FetActivityTag(
                GetElementValue(t, "Name") ?? "",
                ParseBool(GetElementValue(t, "Printable"), true),
                GetElementValue(t, "Comments")
            ))
            .Where(t => !string.IsNullOrWhiteSpace(t.Name))
            .ToList();
    }

    private static IReadOnlyList<FetTeacher> ParseTeachers(XElement root)
    {
        var list = root.Element("Teachers_List");
        if (list == null)
            return [];

        return list.Elements("Teacher")
            .Select(t => new FetTeacher(
                GetElementValue(t, "Name") ?? "",
                ParseInt(GetElementValue(t, "Target_Number_of_Hours"), 0),
                GetElementValue(t, "Comments")
            ))
            .Where(t => !string.IsNullOrWhiteSpace(t.Name))
            .ToList();
    }

    private static IReadOnlyList<FetYear> ParseYears(XElement root)
    {
        var list = root.Element("Students_List");
        if (list == null)
            return [];

        return list.Elements("Year")
            .Select(y => new FetYear(
                GetElementValue(y, "Name") ?? "",
                ParseInt(GetElementValue(y, "Number_of_Students"), 0),
                GetElementValue(y, "Comments"),
                ParseGroups(y)
            ))
            .Where(y => !string.IsNullOrWhiteSpace(y.Name))
            .ToList();
    }

    private static IReadOnlyList<FetGroup> ParseGroups(XElement year)
    {
        return year.Elements("Group")
            .Select(g => new FetGroup(
                GetElementValue(g, "Name") ?? "",
                ParseInt(GetElementValue(g, "Number_of_Students"), 0),
                GetElementValue(g, "Comments"),
                ParseSubgroups(g)
            ))
            .Where(g => !string.IsNullOrWhiteSpace(g.Name))
            .ToList();
    }

    private static IReadOnlyList<FetSubgroup> ParseSubgroups(XElement group)
    {
        return group
            .Elements("Subgroup")
            .Select(s => new FetSubgroup(
                GetElementValue(s, "Name") ?? "",
                ParseInt(GetElementValue(s, "Number_of_Students"), 0),
                GetElementValue(s, "Comments")
            ))
            .Where(s => !string.IsNullOrWhiteSpace(s.Name))
            .ToList();
    }

    private static IReadOnlyList<FetBuilding> ParseBuildings(XElement root)
    {
        var list = root.Element("Buildings_List");
        if (list == null)
            return [];

        return list.Elements("Building")
            .Select(b => new FetBuilding(
                GetElementValue(b, "Name") ?? "",
                GetElementValue(b, "Comments")
            ))
            .ToList();
    }

    private static IReadOnlyList<FetRoom> ParseRooms(XElement root)
    {
        var list = root.Element("Rooms_List");
        if (list == null)
            return [];

        return list.Elements("Room")
            .Select(r => new FetRoom(
                GetElementValue(r, "Name") ?? "",
                GetElementValue(r, "Building"),
                ParseInt(GetElementValue(r, "Capacity"), null),
                ParseBool(GetElementValue(r, "Virtual"), false),
                GetElementValue(r, "Comments")
            ))
            .Where(r => !string.IsNullOrWhiteSpace(r.Name))
            .ToList();
    }

    private static IReadOnlyList<FetActivity> ParseActivities(XElement root)
    {
        var list = root.Element("Activities_List");
        if (list == null)
            return [];

        return list.Elements("Activity")
            .Select(a =>
            {
                var comments = GetElementValue(a, "Comments");
                return new FetActivity(
                    ParseInt(GetElementValue(a, "Id"), -1),
                    GetElementValue(a, "Teacher") ?? "",
                    GetElementValue(a, "Subject") ?? "",
                    GetElementValue(a, "Activity_Tag"),
                    GetElementValue(a, "Students") ?? "",
                    ParseInt(GetElementValue(a, "Duration"), 1),
                    ParseInt(GetElementValue(a, "Total_Duration"), 1),
                    ParseInt(GetElementValue(a, "Activity_Group_Id"), 0),
                    ParseBool(GetElementValue(a, "Active"), true),
                    comments,
                    ParseCommentRefs(comments)
                );
            })
            .ToList();
    }

    private static IReadOnlyList<FetActivityCommentRef> ParseCommentRefs(string? comments)
    {
        if (string.IsNullOrWhiteSpace(comments))
            return [];

        try
        {
            using var doc = JsonDocument.Parse(comments);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                return root
                    .EnumerateArray()
                    .Select(ParseCommentRef)
                    .Where(r => r != null)
                    .Cast<FetActivityCommentRef>()
                    .ToList();
            }

            if (root.ValueKind == JsonValueKind.Object)
            {
                var single = ParseCommentRef(root);
                return single == null ? [] : [single];
            }
        }
        catch (JsonException)
        {
            // Keep import resilient when comments are free-form text and not JSON.
        }

        return [];
    }

    private static FetActivityCommentRef? ParseCommentRef(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return null;

        return new FetActivityCommentRef(
            TryGetInt(element, "id_profesor"),
            TryGetInt(element, "id_planmaterie_prestator"),
            TryGetInt(element, "id_facultate"),
            TryGetInt(element, "id_metaspecializare"),
            TryGetInt(element, "nranstudii"),
            TryGetString(element, "activity_tag"),
            TryGetString(element, "id_grupa"),
            TryGetInt(element, "id_specializare"),
            TryGetInt(element, "id_materie")
        );
    }

    private static int? TryGetInt(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
            return null;

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var numericValue))
            return numericValue;

        if (
            property.ValueKind == JsonValueKind.String
            && int.TryParse(property.GetString(), out var parsedValue)
        )
            return parsedValue;

        return null;
    }

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
            return null;

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString(),
            JsonValueKind.Number => property.GetRawText(),
            _ => null,
        };
    }

    private static string? GetElementValue(XElement parent, string localName)
    {
        var el = parent.Element(localName);
        return el?.Value?.Trim();
    }

    private static bool ParseBool(string? value, bool defaultValue)
    {
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;
        return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }

    private static int ParseInt(string? value, int defaultValue)
    {
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;
        return int.TryParse(value, out var n) ? n : defaultValue;
    }

    private static int? ParseInt(string? value, int? defaultValue)
    {
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;
        return int.TryParse(value, out var n) ? n : defaultValue;
    }
}
