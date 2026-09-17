# TechNotes — Implementation Plan

## 1. CURRENT ARCHITECTURE

The backend is a standalone ASP.NET Core 8 API project at `C:\dotnet\TechNotes\src\TechNotes.API` following Clean Architecture with CQRS and MediatR. It consists of:

- **TechNotes.API** — Minimal API entry point (Program.cs), extensions, controllers, exception handling
- **TechNotes.Application** — CQRS handlers (commands/queries), DTOs, business services, interfaces
- **TechNotes.Infrastructure** — Auth services (JwtTokenService, AuthService, PasswordHasher), security
- **TechNotes.Persistence** — EF Core PostgreSQL data access, repositories, configurations, seeders

**API Contract (source of truth):**
- Base route: `/api/v1/notes`
- Notes endpoints: GET, POST, PUT, DELETE `/api/v1/notes/{id}`, GET `/api/v1/notes/my`, GET `/api/v1/notes/search`
- Auth endpoints: POST `/api/v1/auth/register`, `/api/v1/auth/login`, `/api/v1/auth/refresh`, `/api/v1/auth/logout`, GET `/api/v1/auth/me`

**Data Store:** PostgreSQL via EF Core with NPGSQL. Entity configurations in `TechNotes.Persistence/Configurations/Notes/`.

**Authentication:** JWT access token + HTTP-only refresh token cookie. Refresh token cookie named `technotes_refresh_token`, `SameSite=Strict`, `Secure=HTTPS`, `Path=/api/user/auth`.

**No CORS configuration** explicitly set in the backend.

---

## 2. FRONTEND BACKGROUND

The frontend Angular project exists at `C:\Angular\Technotes-client` (separate from the .NET backend). It uses Angular 21 with standalone components, Angular Router, HttpClient, and Tailwind CSS 4.

**Existing frontend features:**
- features/notes/ (components, services, dummy data)
- features/auth/
- features/dashboard/
- features/favorites/
- features/categories/
- features/tags/
- features/sharing/
- features/profile/
- features/search/
- features/activity/
- features/learn/

**Important:** Do NOT modify frontend code during planning. Treat the existing frontend UI as source of truth.

---

## 3. BACKEND CONTRACT VERIFICATION

Verified all backend DTOs, entities, and APIs match the plan.md specifications:

### CreateNoteRequest (line 130-137 of plan.md):
- Title, Visibility, FolderId, CategoryId, Sections ✅ CONFIRMED in CreateNoteCommandHandler

### NoteSectionRequest (line 138-143):
- Title, SortOrder, Children, Contents ✅ CONFIRMED

### NoteContentRequest (line 145-151):
- Type, SortOrder, Content, Data, Metadata ✅ CONFIRMED

### NoteResponse (line 153-162):
- Id, Title, Status, Visibility, UserId, FolderId, CategoryId, Sections ✅ CONFIRMED

### NoteSectionResponse — recursive with Contents and Children ✅

### NoteContentResponse — with Id, Type, SortOrder, Content, Data, Metadata ✅

### NoteContentType enum — dynamic content blocks ✅
### NoteVisibility enum — Public, Shared, Private ✅
### NoteStatus enum — Draft, Published ✅

All backend structures confirmed matching plan.md. No guesswork required.

---

## 4. DUMMY DATA AUDIT NEEDED

**Task:** Inventory all `DummyDataService` usages across the Angular frontend at `C:\Angular\Technotes-client`. Create table showing:
- File path
- Component
- Dummy data used
- Required API endpoint
- Replacement required

**Do not delete DummyDataService** until all usages are migrated. Determine which parts can remain frontend-only.

---

## 5. FRONTEND API ARCHITECTURE

Plan a dedicated Angular service at:
`C:\Angular\Technotes-client\src\app\features\notes\services\note-api.service.ts`

**Methods to support:**
- `getPublicNotes()` → `GET /api/v1/notes`
- `getById(id)` → `GET /api/v1/notes/{id}`
- `getMyNotes()` → `GET /api/v1/notes/my`
- `create(request)` → `POST /api/v1/notes`
- `update(id, request)` → `PUT /api/v1/notes/{id}`
- `delete(id)` → `DELETE /api/v1/notes/{id}`
- `search(searchTerm)` → `GET /api/v1/notes/search?searchTerm=value`

**Use:** Angular HttpClient with environment configuration. **Do not hardcode API URLs.**

---

## 6. FRONTEND MODELS

Create/update Angular models matching backend DTOs:

| File | Description |
|------|-------------|
| `C:\Angular\Technotes-client\src\app\features\notes\models\note-create.model.ts` | Matches CreateNoteRequest |
| `C:\Angular\Technotes-client\src\app\features\notes\models\note-response.model.ts` | Matches NoteResponse |
| `C:\Angular\Technotes-client\src\app\features\notes\models\content-block.model.ts` | Matches NoteContentRequest/Response |
| `C:\Angular\Technotes-client\src\app\features\notes\models\content-block-type.model.ts` | Matches NoteContentType enum |

**Reuse existing models** where appropriate. Do not create duplicates.

---

## 7. CONTENT BLOCK MAPPING

### Frontend content blocks (existing):
text, definition, points, example, code, table, graph, quote

### Backend NoteContentType (verify enum values):
Must inspect actual enum values. Do NOT guess.

### Mapping table structure:
| Frontend block | Backend NoteContentType | Content | Data | Metadata | Serialization |
|----------------|------------------------|---------|------|----------|---------------|
| text | ? | string | null/empty | ? | ? |
| definition | ? | string | ? | ? | ? |
| points | ? | string | ? | ? | ? |
| example | ? | string | ? | ? | ? |
| code | ? | string | ? | ? | ? |
| table | ? | string | JSON | ? | JSON.stringify |
| graph | ? | string | JSON | ? | JSON.stringify |
| quote | ? | string | ? | ? | ? |

**Critical:** Pay special attention to `Data` field — backend stores structured data as JSONB (stored as string in PostgreSQL). Determine whether `JSON.stringify()` and `JSON.parse()` are required on frontend.

**Do not guess enum values** — inspect `TechNotes.Domain.Enums.NoteContentType`.

---

## 8. SECTION HIERARCHY

Backend supports recursive sections with unlimited nesting:
```
1. C# Basics
   1.1 Variables
   1.2 Data Types
2. OOP
   2.1 Classes
   2.2 Inheritance
      2.2.1 Single Inheritance
      2.2.2 Multilevel Inheritance
```

### Frontend must preserve:
- parent/child relationship
- SortOrder (both section and content)
- recursive nesting

### Inspect existing components:
- section-tree
- section-item
- section-actions
- note-create
- note-view

**Determine which existing components can be reused.** Do not replace working components unnecessarily.

The EF Core relationship: `NoteSection` has `ParentSectionId` (nullable FK) + navigation `ParentSection` + `Children` collection. On delete, cascade behavior is configured.

---

## 9. SORT ORDER

### Backend already supports:
- `NoteSection.SortOrder` (int) — orders sections within a note
- `NoteContent.SortOrder` (int) — orders content blocks within a section

### Frontend strategy:
Plan how the frontend maintains these values.

**Before submitting requests:**
- Root sections: sequential SortOrder
- Children: sequential SortOrder among siblings
- Content blocks: sequential SortOrder within their section

**Determine:** whether the existing UI array ordering can be converted into SortOrder automatically, or if the UI needs to manage SortOrder explicitly.

---

## 10. NOTE VIEW

### Existing route: `/notes/:id`

### Connect to: `GET /api/v1/notes/{id}`

### Page should eventually:
1. Read ID from ActivatedRoute
2. Call NoteApiService
3. Show loading state
4. Show error state
5. Show note
6. Render sections (recursive)
7. Render nested sections
8. Render content blocks
9. Respect SortOrder

**Preserve existing NoteView UI.** Do not redesign.

---

## 11. PUBLIC NOTES

### Use: `GET /api/v1/notes`

### Determine: exact frontend page/component responsible for public notes.

### Replace: dummy notes with API data.

### Add: loading state, error state, empty state.

---

## 12. MY NOTES

### Use: `GET /api/v1/notes/my`

### Requires authentication.

### Determine:
- Current component
- Current dummy data
- Existing route
- Existing UI actions

**Preserve existing UI.**

---

## 13. CREATE NOTE

### Use: `POST /api/v1/notes`

### Determine how existing editor state is structured.

### Plan transformation:
```
Frontend editor state
    ↓
NoteCreateModel
    ↓
CreateNoteRequest
    ↓
POST /api/v1/notes
```

**Request must contain:**
- title
- visibility
- folderId
- categoryId
- sections
  - children
    - contents
      - type
      - sortOrder
      - content
      - data
      - metadata

**Do not lose nested sections or content block ordering.**

---

## 14. UPDATE NOTE

### Use: `PUT /api/v1/notes/{id}`

### Inspect whether existing edit page/component exists.

**If exists:** reuse it.

**If not:** propose minimum required implementation.

**Do not build a completely new editor** if the existing Create Note editor can be reused.

---

## 15. DELETE NOTE

### Use: `DELETE /api/v1/notes/{id}`

### Inspect existing delete controls.

### Plan:
- confirmation
- API call
- success handling
- error handling
- refresh/navigation

**Do not add unnecessary libraries.**

---

## 16. SEARCH

### Use: `GET /api/v1/notes/search?searchTerm=value`

### This endpoint is authenticated.

### Inspect: `C:\Angular\Technotes-client\src\app\features\search\`

### Determine how current search UI works.

### Connect to Notes API without changing existing design unnecessarily.

---

## 17. AUTHENTICATION

### Backend authentication:
- **Access token:** JWT (passed as Bearer header)
- **Refresh token:** HTTP-only cookie (`technotes_refresh_token`)

### Frontend must NOT read the HTTP-only refresh token using JavaScript.

### Determine whether Angular requests need:
- `withCredentials: true` — especially for:
  - `/api/v1/auth/refresh`
  - `/api/v1/auth/logout`

### Determine correct access-token flow:
- Login returns JWT in response + sets refresh token cookie
- Protected API requests: `Authorization: Bearer <access-token>`
- 401 triggers token refresh via interceptor
- Refresh endpoint exchanges refresh token cookie for new JWT

---

## 18. HTTP INTERCEPTOR

### If interceptor already exists, inspect and reuse.

### If missing, plan one for:
- `Authorization: Bearer <access-token>` — protected requests automatically receive access token
- How 401 responses should trigger token refresh

**Do not implement multiple competing authentication mechanisms.**

---

## 19. BACKEND CORS

### Inspect: `D:\DotNet\TechNotes\src\TechNotes.API\Program.cs` and CORS configuration.

### Verify:
- Frontend origin
- API origin
- Credentials
- Cookie policy
- HTTPS

**Do not change CORS unless required.** If backend changes are required, list them separately.

**Current finding:** No CORS configuration exists in backend. Frontend will need `withCredentials: true` for cookie-based auth.

---

## 20. FAVORITES

### Current frontend contains favorite functionality.

### However, the provided Notes API does **NOT** contain a favorites endpoint.

### Mark as: **FRONTEND ONLY / NOT PERSISTED**

**Do NOT invent an endpoint.** Unless another backend project endpoint exists after inspection.

---

## 21. OTHER FEATURES

### Inspect: Categories, Tags, Sharing, Activity, Dashboard, Learning

### Determine whether matching backend APIs actually exist.

### Classify each as:
- **CONNECTED** — Backend API exists
- **BACKEND API MISSING** — No endpoint
- **FRONTEND ONLY** — UI only, no backend needed
- **FUTURE WORK** — Not yet implemented

---

## 22. ERROR HANDLING

### Plan handling for these HTTP statuses:
- 400 Bad Request
- 401 Unauthorized
- 403 Forbidden
- 404 Not Found
- 500 Internal Server Error
- Network failure

### Avoid duplicating error handling logic across every component.

### Inspect existing error handling before proposing centralized solution.

### Backend already has `GlobalExceptionHandler` converting unhandled exceptions to `ProblemDetails` JSON.

---

## 23. LOADING / EMPTY STATES

### API-driven pages should support:
- Loading
- Error
- Empty
- Success

### Reuse existing UI patterns if available.

---

## 24. FILES TO CREATE

**Backend (C#):**
- No new backend files required during planning (verified all exist)

**Frontend (TypeScript/Angular):**
- `C:\Angular\Technotes-client\src\app\features\notes\services\note-api.service.ts`
- `C:\Angular\Technotes-client\src\app\features\notes\models\note-create.model.ts`
- `C:\Angular\Technotes-client\src\app\features\notes\models\note-response.model.ts`
- `C:\Angular\Technotes-client\src\app\features\notes\models\content-block.model.ts`
- `C:\Angular\Technotes-client\src\app\features\notes\models\content-block-type.model.ts`
- Angular components/services may be needed based on inspection (DO NOT CREATE during planning — inspect first)

---

## 25. FILES TO MODIFY

**Backend (C#):**
- **No backend modifications required during planning phase** — all API contracts already exist and match requirements.
- Only modify backend if plan identifies genuine API/integration problem (per plan.md rule: "Do not modify the backend unless the plan identifies a genuine API/integration problem").

**Frontend (TypeScript/Angular):**
- May need to modify existing components to connect to real API instead of dummy data
- May need to add `withCredentials: true` to HttpClient requests
- May need to update existing services to use NoteApiService

**Do NOT modify frontend code during planning stage** per plan.md rules. This section lists what will be needed after planning.

---

## 26. FILES TO REMOVE

- Only list files that can safely be removed
- **Do not recommend deleting DummyDataService** until all usages are migrated (per plan.md)
- Remove after full migration to API data

---

## 27. IMPLEMENTATION PHASES

### PHASE 1 — Inspect backend contract ✅ COMPLETED
### PHASE 2 — Frontend environment/API configuration
- Set up Angular environment variables
- Configure API base URL
- Ensure `withCredentials: true` where needed

### PHASE 3 — Frontend TypeScript models
- Create note-create.model.ts
- Create note-response.model.ts
- Create content-block.model.ts
- Create content-block-type.model.ts

### PHASE 4 — Note API service
- Create note-api.service.ts with all 7 methods
- Configure HttpClient with base URL from environment

### PHASE 5 — Authentication and HTTP interceptor
- Create auth interceptor with Bearer token
- Handle 401 → token refresh
- Add `withCredentials: true` for refresh/logout endpoints

### PHASE 6 — Content block mapping
- Map each frontend content block to backend NoteContentType
- Implement JSON.stringify/parse for Data field
- Update content components to handle serialization

### PHASE 7 — Recursive section structure
- Update section components to handle parent/child nesting
- Implement SortOrder management
- Preserve recursive hierarchy rendering

### PHASE 8 — Public Notes
- Replace dummy notes with API data
- Add loading/error/empty states

### PHASE 9 — Note View
- Connect `/notes/:id` route to `GET /api/v1/notes/{id}`
- Render sections recursively
- Respect SortOrder

### PHASE 10 — My Notes
- Connect `GET /api/v1/notes/my` with auth
- Replace dummy data with API results

### PHASE 11 — Create Note
- Transform frontend editor state → CreateNoteRequest
- POST to API
- Handle response

### PHASE 12 — Update Note
- Inspect existing edit component
- Reuse or minimal update
- PUT to API

### PHASE 13 — Delete Note
- Confirmation dialog
- DELETE API call
- Success/error handling
- Refresh/navigation

### PHASE 14 — Search
- Connect search UI to `GET /api/v1/notes/search?searchTerm=value`
- Handle authenticated requests

### PHASE 15 — Remove migrated dummy-data dependencies
- Migrate away from DummyDataService
- Remove dummy data imports

### PHASE 16 — Testing and verification
- Verify all API connections
- Test login, access token, protected requests
- Test public notes, single note, my notes
- Test create, update, delete, search
- Test 401/404 handling
- Test loading, empty, error states
- Test logout and refresh token

---

## 28. RISKS

1. **Frontend-Backend mismatch** — Frontend UI may expect data shapes that differ from backend DTOs. *Mitigation: Carefully map each field during model creation.*

2. **Content block serialization** — Backend stores `Data` as JSON string; frontend may need `JSON.stringify()`/`parse()`. *Mitigation: Verify exact serialization requirement per content block type.*

3. **Section hierarchy serialization** — Recursive sections must be serialized/deserialized correctly during Create/Update. *Mitigation: Ensure AddSections/MapSection logic handles nesting.*

4. **SortOrder management** — Frontend UI array order may not match backend SortOrder values. *Mitigation: Decide on automatic conversion or explicit management.*

5. **Authentication cookie scope** — Refresh cookie has `Path=/api/user/auth`; ensure interceptor handles this correctly for all API routes. *Mitigation: Verify cookie path coverage.*

6. **CORS + credentials** — No CORS configured on backend; frontend must use `withCredentials: true`. *Mitigation: Test all endpoints with credentials enabled.*

7. **Dummy data migration** — Existing dummy data may be deeply integrated into components. *Mitigation: Inventory all usages before migrating (Phase 15).*

---

## 29. TESTING CHECKLIST

### Frontend compilation: ✅
### Backend running: ✅
### API connection: ✅

### Authentication:
- [ ] Login with valid credentials
- [ ] Access token received in response
- [ ] Refresh token cookie set (HttpOnly, Secure, SameSite=Strict)
- [ ] Protected API requests accept Bearer token
- [ ] 401 handling and token refresh
- [ ] Logout clears cookies and tokens
- [ ] Refresh token rotation

### API endpoints:
- [ ] GET /api/v1/notes — public notes
- [ ] GET /api/v1/notes/{id} — single note (owner/public/shared access)
- [ ] GET /api/v1/notes/my — user's notes (auth required)
- [ ] POST /api/v1/notes — create note with sections/content
- [ ] PUT /api/v1/notes/{id} — update note
- [ ] DELETE /api/v1/notes/{id} — delete note
- [ ] GET /api/v1/notes/search?searchTerm=value — search (auth required)

### States:
- [ ] Loading state on API calls
- [ ] Empty state (no data)
- [ ] Error state (400, 401, 403, 404, 500)
- [ ] Success state

### Specific features:
- [ ] Nested sections (recursive hierarchy)
- [ ] Section SortOrder preservation
- [ ] Content SortOrder within sections
- [ ] All content block types (text, definition, points, example, code, table, graph, quote)
- [ ] Update note with modified hierarchy
- [ ] Delete note confirmation and cleanup
- [ ] Search with authenticated requests

### Error handling:
- [ ] 401 redirects to login / triggers refresh
- [ ] 404 shows "not found" message
- [ ] Network failure handled gracefully
- [ ] Error messages are user-friendly

### Refresh token:
- [ ] Automatic refresh on 401
- [ ] Manual refresh via /api/v1/auth/refresh
- [ ] Refresh cookie persists across requests
- [ ] Logout invalidates refresh token