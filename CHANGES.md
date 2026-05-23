# Backend ↔ Frontend Wire-Up Changes

This file summarises the changes made to wire the **fisa-activitate-zilnica-client**
React app to the **fisa-activitate-zilnica-api** .NET API, including all schema,
endpoint, and client-side migrations.

Both projects build cleanly:

- API: `dotnet build` → 0 warnings, 0 errors
- Client: `npm run build` (`tsc -b && vite build`) → clean

---

## 1. CORS

`Program.cs`

- Replaced the previous `AllowAll` policy with a `Client` policy that uses
  `WithOrigins(clientUrl).AllowAnyMethod().AllowAnyHeader().AllowCredentials()`.
- The CORS configuration is **always** registered (no null check); the new
  `CLIENT_URL` env var is required.
- `app.UseCors("Client")` is invoked before `app.MapControllers()` (and would sit
  before authentication if/when it is added).

`requiredEnvVars` in `Program.cs` now includes `CLIENT_URL`. `.env.example` adds
`CLIENT_URL=http://localhost:5173`.

## 2. Daily Activity Records — schema extension

`DailyActivityRecord` model and DTOs gained four new fields (all required):

| Column              | Type     | Notes                                                  |
|---------------------|----------|--------------------------------------------------------|
| `FacultyName`       | `string` | Maps from the client's `faculty` field.                |
| `StudyProgram`      | `string` | Maps from the client's `studyProgram` field.           |
| `CourseType`        | `string` | "Course" / "Seminar" / "Other" (the client UI value).  |
| `ConventionalHours` | `double` | Computed client-side from actual hours × type weight.  |

The existing `ActivityType` enum (`RegularHours`/`OvertimeHours`) is now optional
on `CreateDailyActivityRecordRequest` and defaults to `RegularHours`. The client
does not surface this concept yet.

Migration: `Data/Master/Migrations/AddFacultyAndCourseFieldsToDailyActivityRecords.cs`
(`20260509001`) adds the columns with safe defaults so existing rows migrate
without manual back-fill.

`DailyActivityRecordsCommandService` was updated to validate the new required
fields and the non-negative `ConventionalHours`.

The `query` endpoint now returns `200` with an empty array when there are no
records (previously it returned `404`, which broke empty-state UX in the client).

## 3. Daily Activity Records — monthly summary

New endpoint:

```
GET /api/DailyActivityRecords/monthly-summary?teacherId={int}
→ 200 [{ year, month, recordCount, totalConventionalHours, status }]
```

- New DTO: `MonthlyActivitySummaryResponse`.
- New enum: `MonthlySheetStatus` (`Draft = 0`, `Submitted = 1`, `Approved = 2`).
  Today every row is returned as `Draft` — the approval workflow is left as
  future work (it would require a separate `MonthlySheets` table).
- Repository aggregates with `GroupBy` on `(StartDate.Year, StartDate.Month)`.
- Service adds `GetMonthlySummariesAsync(teacherId)`.

## 4. New module: Supplementary Activities

A new resource was added end-to-end so the **Supplementary Activities Annex**
page can read/write through the API.

**Files added under `SupplementaryActivities/`:**

- `Models/SupplementaryActivity.cs` (Id, ExternalTeacherId, Date, ActivityType, Observations, TotalHours)
- `DTOs/Requests/CreateSupplementaryActivityRequest.cs`
- `DTOs/Requests/UpdateSupplementaryActivityRequest.cs`
- `DTOs/Responses/GetSupplementaryActivityResponse.cs`
- `Repositories/Interfaces/ISupplementaryActivitiesRepository.cs`
- `Repositories/SupplementaryActivitiesRepository.cs`
- `Services/Interfaces/ISupplementaryActivitiesQueryService.cs`
- `Services/Interfaces/ISupplementaryActivitiesCommandService.cs`
- `Services/SupplementaryActivitiesQueryService.cs`
- `Services/SupplementaryActivitiesCommandService.cs`
- `Controllers/Interfaces/SupplementaryActivitiesApiController.cs`
- `Controllers/SupplementaryActivitiesController.cs`

**Endpoints (all under `/api/SupplementaryActivities`):**

- `GET /get-by-id/{id}`
- `GET /query?teacherId={int}&startDate=&endDate=`
- `POST /create`
- `PUT /update`
- `DELETE /delete/{id}`

Migration: `Data/Master/Migrations/CreateSupplementaryActivitiesTable.cs`
(`20260509002`) creates the table.

`MasterDbContext` got a `SupplementaryActivities` `DbSet`, `MappingProfile` got
the three Auto Mapper maps, and `Program.cs` registers the new repo + services
in DI.

## 5. Client — dependency setup

`package.json` now depends on:

- `axios@^1.16.0`
- `@tanstack/react-query@^5.100.9`
- `zustand@^5.0.13`

Configuration files:

- `.env.example`, `.env.local`: `VITE_API_BASE_URL=http://localhost:8000`.
- `src/vite-env.d.ts`: typed `ImportMetaEnv`.
- `src/lib/query-client.ts`: shared `QueryClient` (`staleTime: 30s`, `retry: 1`,
  `refetchOnWindowFocus: false`).
- `src/api/client.ts`: shared axios instance keyed off the env var.
- `src/App.tsx`: wraps the router in `<QueryClientProvider />`.

## 6. Client — auth-lite & store

A simple email-based "login" is now the entry point. There is no password yet —
the API's existing `/api/v1/ExternalTeachers/by-email/{email}` endpoint is used
to resolve the teacher by email and seed the local store.

- `src/pages/Login.tsx`: form with the email field pre-filled with
  `maican@unitbv.ro`. Calls the API, persists the resolved teacher, navigates to
  the dashboard. Surfaces a friendly error on `404` or network failure.
- `src/components/auth/RequireTeacher.tsx`: redirects to `/login` when no
  `externalTeacherId` is in the store.
- `src/store/teacher.ts`: `useTeacherStore` (zustand + `persist` to
  `daily-activity-header`) holds:
  - `externalTeacherId`, `email`, `fullName` (resolved from the API)
  - editable header fields the existing UI exposed (`teacherName`, `department`,
    `academicYear`)
  - `setFromExternalTeacher`, `setHeader`, `reset`
- `src/components/layout/Header.tsx`: the existing **Logout** button now resets
  the store and navigates to `/login`.

## 7. Client — API hooks

- `src/api/daily-activity-records.ts`: typed CRUD + query hooks, now with the
  four new fields and a new `useMonthlyActivitySummary(teacherId)` hook.
- `src/api/supplementary-activities.ts`: full CRUD + query hooks for the new
  resource.
- `src/api/external-teachers.ts`: existing `useExternalTeacherByEmail`.
- `src/api/schedules.ts`: existing schedule hooks (untouched in this turn but
  available).

All hooks use TanStack Query keys, `enabled: Boolean(...)` guards for missing
ids, and invalidate caches on mutations.

## 8. Client — page migrations

### Dashboard

- `PersonalInfoCard.tsx`: now reads from `useTeacherStore` and the latest
  `DailyActivityRecord` for `Department` / `Faculty`. Shows
  `Name`, `Email`, `Department`, `Faculty`.
- `PreviousSheetsCard.tsx`: uses `useMonthlyActivitySummary`. Shows month-year,
  record count, conventional-hours total, and status badge (Draft / Submitted /
  Approved). Displays loading / empty / error states.
- `SupplementaryCard.tsx`: unchanged (just navigates).
- `ActivitySheetCard.tsx`: unchanged.

### `DailyActivitySheet.tsx` (full rewrite)

- All `localStorage` reads/writes were removed.
- Uses `useDailyActivityRecords({ teacherId })`,
  `useCreateDailyActivityRecord`, `useUpdateDailyActivityRecord`.
- Pulls supplementary records via `useSupplementaryActivities` for the PDF
  annex page.
- Day status colouring (green / yellow) is derived from the records returned
  by the API (a record with non-empty `observations` counts as completed,
  otherwise partial).
- Form ↔ payload mapping:
  - `faculty` → `facultyName`
  - `studyProgram` → `studyProgram`
  - `discipline` → `subjectName`
  - `activityType` → `courseType`
  - `group` → `groupName`
  - `room` → `roomName`
  - `year` → `parseInt(year)`
  - `actualHours` → encoded as `endDate - startDate` (so we don't need a
    separate column)
  - `conventionalHours` → `conventionalHours`
  - `status` `NB` / `PO` → `revenueType` `BaseSalary` / `HourlyPay`
  - `observations` passes through; null when blank
  - `startDate` is `${date}T${time}:00` as ISO; `endDate` is start + actual hrs
- Existing UX preserved: calendar with status colours, per-day entries with
  edit pencil, monthly summary table, and PDF export. The PDF now reads from
  the API instead of `localStorage` (both the main sheet and the annex page).
- "Duplicate Row" creates an additional record via the API; "Submit for
  Approval" still shows an alert and navigates home (no real workflow yet).

### `SupplementaryActivitiesAnnex.tsx` (full rewrite)

- Uses `useSupplementaryActivities`, `useCreateSupplementaryActivity`,
  `useUpdateSupplementaryActivity`.
- Header data comes from `useTeacherStore`.
- Same "Duplicate / Save / Submit / Export" interaction surface as before,
  with API-backed persistence.
- PDF export reads from records returned by the API.

## 9. Things deliberately left as future work

- **Approval workflow.** Today every monthly summary row is `Draft`. To support
  Submitted / Approved you would add a `MonthlySheets` table (or columns on the
  daily record), an endpoint to flip the status, and a UI affordance.
- **Real authentication.** The login is a soft email-based lookup with no
  password and no token. Add real auth (JWT, ASP.NET Identity, or an SSO) when
  needed.
- **Faculty / Study Program / Discipline dropdowns.** These remain hardcoded in
  the client. They could be served from the schedule (`Activity` / `Subject`)
  data once a teacher is associated with a schedule.
- **Server-side duplicate detection.** Removed from the client. If you want
  it, add a uniqueness constraint or an explicit pre-flight check on the API
  side.

---

## File map (new + modified)

### API

```
Program.cs                                                              (modified)
.env.example                                                            (modified)
DailyActivities/
  Models/DailyActivityRecord.cs                                         (modified)
  Models/MonthlySheetStatus.cs                                          (new)
  DTOs/Requests/CreateDailyActivityRecordRequest.cs                     (modified)
  DTOs/Requests/UpdateDailyActivityRecordRequest.cs                     (modified)
  DTOs/Responses/GetDailyActivityRecordResponse.cs                      (modified)
  DTOs/Responses/MonthlyActivitySummaryResponse.cs                      (new)
  Repositories/DailyActivityRecordsRepository.cs                        (modified)
  Repositories/Interfaces/IDailyActivityRecordsRepository.cs            (modified)
  Services/DailyActivityRecordsCommandService.cs                        (modified)
  Services/DailyActivityRecordsQueryService.cs                          (modified)
  Services/Interfaces/IDailyActivityRecordsQueryService.cs              (modified)
  Controllers/DailyActivityRecordsController.cs                         (modified)
  Controllers/Interfaces/DailyActivityRecordsApiController.cs           (modified)
SupplementaryActivities/                                                (new module)
  Models/SupplementaryActivity.cs
  DTOs/Requests/CreateSupplementaryActivityRequest.cs
  DTOs/Requests/UpdateSupplementaryActivityRequest.cs
  DTOs/Responses/GetSupplementaryActivityResponse.cs
  Repositories/Interfaces/ISupplementaryActivitiesRepository.cs
  Repositories/SupplementaryActivitiesRepository.cs
  Services/Interfaces/ISupplementaryActivitiesQueryService.cs
  Services/Interfaces/ISupplementaryActivitiesCommandService.cs
  Services/SupplementaryActivitiesQueryService.cs
  Services/SupplementaryActivitiesCommandService.cs
  Controllers/Interfaces/SupplementaryActivitiesApiController.cs
  Controllers/SupplementaryActivitiesController.cs
Data/Master/MasterDbContext.cs                                          (modified)
Data/Master/Migrations/AddFacultyAndCourseFieldsToDailyActivityRecords.cs (new)
Data/Master/Migrations/CreateSupplementaryActivitiesTable.cs            (new)
System/MappingProfile.cs                                                (modified)
```

### Client

```
package.json                                                            (modified — new deps)
.env.example                                                            (new)
.env.local                                                               (new)
src/vite-env.d.ts                                                       (new)
src/App.tsx                                                             (modified — provider + routes)
src/api/client.ts                                                       (new)
src/api/daily-activity-records.ts                                       (modified)
src/api/schedules.ts                                                    (new earlier; unchanged)
src/api/external-teachers.ts                                            (new earlier; unchanged)
src/api/supplementary-activities.ts                                     (new)
src/lib/query-client.ts                                                 (new)
src/store/teacher.ts                                                    (modified — full ext-teacher state)
src/components/auth/RequireTeacher.tsx                                  (new)
src/components/layout/Header.tsx                                        (modified — wired logout)
src/components/dashboard/PersonalInfoCard.tsx                           (modified — wired)
src/components/dashboard/PreviousSheetsCard.tsx                         (modified — wired)
src/pages/Login.tsx                                                     (new)
src/pages/Dashboard.tsx                                                 (unchanged — children carry the wiring)
src/pages/DailyActivitySheet.tsx                                        (full rewrite)
src/pages/SupplementaryActivitiesAnnex.tsx                              (full rewrite)
```
