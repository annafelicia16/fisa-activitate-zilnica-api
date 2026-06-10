using FisaActivitateZilnicaApi.ExternalReferences.Models;

namespace FisaActivitateZilnicaApi.Schedules.Services;

// Single source of truth for resolving a row's "effective" specialization id and
// the small string helpers around it. Used both at READ time (SchedulesQueryService
// slot resolution) and at WRITE time (FetImportService / backfill caching the
// AGSIS names) so the cached EffectiveSpecializationExternalId always matches what
// the read path would have computed.
public static class SpecializationResolver
{
    // Priority: the row's own SpecializationExternalId > the group's IdSpecializare
    // (resolved from the first id in GroupExternalId) > a short-name + faculty hint
    // derived from the friendly group code (e.g. "IE3" → "IE"). Returns 0 when none
    // resolve.
    public static int ResolveEffectiveSpecializationId(
        int? ownSpecializationExternalId,
        string? groupExternalIdCsv,
        int? facultyExternalId,
        string? groupNames,
        IReadOnlyDictionary<int, ExternalGroup> groups,
        IReadOnlyDictionary<(string ShortName, int FacultyId), int>? shortNameToSpecId = null
    )
    {
        if (ownSpecializationExternalId is int own && own > 0)
            return own;

        int groupId = TryParseFirstInt(groupExternalIdCsv);
        if (
            groupId > 0
            && groups.TryGetValue(groupId, out var group)
            && group.IdSpecializare is long specId
            && specId > 0
        )
        {
            return (int)specId;
        }

        if (shortNameToSpecId is not null && facultyExternalId is int facId && facId > 0)
        {
            string? prefix = ExtractAlphaPrefix(groupNames);
            if (
                !string.IsNullOrEmpty(prefix)
                && shortNameToSpecId.TryGetValue((prefix, facId), out int hintedSpec)
                && hintedSpec > 0
            )
            {
                return hintedSpec;
            }
        }

        return 0;
    }

    // Group external id arrives as "30638" or — after a bracket strip — sometimes
    // "30638,30639". Take the first numeric token.
    public static int TryParseFirstInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;
        var first = value.Split(',', ' ', ';', '/')[0].Trim();
        return int.TryParse(first, out var parsed) ? parsed : 0;
    }

    // Pulls the leading alpha prefix from a friendly group code like "IE3" → "IE",
    // "MNG4" → "MNG". Returns null when the value starts with digits (a real group
    // code such as "13LF731", which we never need to fall back through).
    public static string? ExtractAlphaPrefix(string? grupa)
    {
        if (string.IsNullOrWhiteSpace(grupa))
            return null;
        var first = grupa.Split(',', ' ', ';', '/')[0].Trim();
        int i = 0;
        while (i < first.Length && char.IsLetter(first[i]))
            i++;
        if (i == 0)
            return null;
        return first[..i].ToUpperInvariant();
    }
}
