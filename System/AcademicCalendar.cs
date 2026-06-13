namespace FisaActivitateZilnicaApi.System;

public static class AcademicCalendar
{
    // Romanian academic year is calendar-driven: the cycle "Y-(Y+1)" starts in
    // August and runs through July of the next year. Past Aug 1 →
    // "thisYear-nextYear", otherwise (Jan–Jul) → "prevYear-thisYear". The label
    // is matched against AGSIS AnUniversitar.Denumire ("An universitar
    // 2025-2026") via Contains.
    public static string CurrentYearLabel(DateTime today)
    {
        int year = today.Year;
        return today.Month >= 8 ? $"{year}-{year + 1}" : $"{year - 1}-{year}";
    }
}
