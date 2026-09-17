# TechNotes API Documentation — For Frontend Consumption

**Base URL:** `https://your-backend-domain.com/api/v1`
**All endpoints prefix with:** `/api/v1/`

---

## Authentication APIs

### 1. POST /api/v1/auth/register
**Description:** Register a new user account
**Request Body (CreateRequest):**
```json
{
  "email": "string",
  "userName": "string",
  "password": "string"
}
```
**Success Response:** 200 OK — Returns user object with JWT access token
**Auth:** Public (no auth required)
**Notes:** On success, sets HTTP-only refresh token cookie (`technotes_refresh_token`)

---

### 2. POST /api/v1/auth/login
**Description:** Login user and obtain access token
**Request Body:**
```json
{
  "userName": "string",
  "password": "string"
}
```
**Success Response:** 200 OK — Returns user object with JWT
**Auth:** Public
**Notes:** On success, sets HTTP-only refresh token cookie (`technotes_refresh_token`, `SameSite=Strict`, `Secure`, `Path=/api/user/auth`)

---

### 3. POST /api/v1/auth/refresh
**Description:** Refresh access token using refresh token cookie
**Response:** Sets new refresh token cookie on success
**Auth:** Uses refresh token cookie (`technotes_refresh_token`)
**Important:** `withCredentials: true` required on Angular HttpClient

---

### 4. POST /api/v1/auth/logout
**Description:** Logout user and invalidate refresh token
**Response:** Clears refresh token cookie
**Auth:** Uses refresh token cookie (`technotes_refresh_token`)
**Important:** `withCredentials: true` required on Angular HttpClient

---

### 5. GET /api/v1/auth/me
**Description:** Get current authenticated user profile
**Success Response:** 200 OK — User details (Id, UserName, Email, etc.)
**Auth:** Bearer JWT access token required in Authorization header

---

## Notes APIs

### 6. GET /api/v1/notes
**Description:** Get all public notes
**Success Response:** 200 OK — Array of NoteResponse (minimal data, only root sections)
**Auth:** Public (anonymous access allowed)
**Example:**
```json
[
  {
    "id": "guid",
    "title": "Sample Note",
    "visibility": "Public",
    "sections": [...]
  }
]
```

---

### 7. GET /api/v1/notes/{id}
**Description:** Get a single note by ID
**URL Parameters:** `id` (Guid)
**Success Response:** 200 OK — Full NoteResponse with hierarchical sections
**Auth:** Public or authenticated — owner, public, or shared users can access
**Error:** 404 if note not found or user lacks permission
**Important:** Read `userId` from query or context — anonymous requests set `userId: null`

**Request with auth:**
```
Authorization: Bearer <jwt-token>
```

**Request without auth (anonymous):**
- Works if note visibility is Public

---

### 8. GET /api/v1/notes/my
**Description:** Get current user's notes
**Success Response:** 200 OK — Array of NoteResponse
**Auth:** Bearer JWT access token required
**Important:** `withCredentials: true` for cookie-based auth

---

### 9. POST /api/v1/notes
**Description:** Create a new note
**Request Body (CreateNoteRequest):**
```json
{
  "title": "string",
  "visibility": "Public | Shared | Private",
  "folderId": "guid (nullable)",
  "categoryId": "guid (nullable)",
  "sections": [
    {
      "title": "string",
      "sortOrder": 0,
      "children": [...],
      "contents": [
        {
          "type": "NoteContentType enum",
          "sortOrder": 0,
          "content": "string (optional)",
          "data": "string (JSON, optional)",
          "metadata": "string (optional)"
        }
      ]
    }
  ]
}
```
**Success Response:** 201 Created — Full NoteResponse
**Auth:** Bearer JWT access token required
**Important:** 
- `withCredentials: true` required (refresh cookie sent automatically)
- Sections use recursive structure with ParentSection/Children
- SortOrder must be provided for each section/content

---

### 10. PUT /api/v1/notes/{id}
**Description:** Update an existing note
**URL Parameters:** `id` (Guid)
**Request Body (UpdateNoteRequest):** Same structure as CreateNoteRequest (title, visibility, folderId, categoryId, sections)
**Success Response:** 200 OK — Updated NoteResponse
**Auth:** Bearer JWT access token required
**Important:** 
- User must be owner or have Edit share permission
- Old section tree is cleared before adding updated structure
- `withCredentials: true` required

---

### 11. DELETE /api/v1/notes/{id}
**Description:** Delete a note
**URL Parameters:** `id` (Guid)
**Success Response:** 204 No Content
**Auth:** Bearer JWT access token required
**Error:** 404 if note not found or user is not owner
**Important:** `withCredentials: true` required

---

### 12. GET /api/v1/notes/search
**Description:** Search notes by title
**URL Parameters:** `searchTerm` (string, required)
**Success Response:** 200 OK — Array of Note (filtered by title)
**Auth:** Bearer JWT access token required (authenticated users only)
**Example:** `/api/v1/notes/search?searchTerm=csharp`

---

## Response DTO Structures

### NoteResponse
```json
{
  "id": "guid",
  "userId": "guid",
  "title": "string",
  "status": "NoteStatus enum (Draft | Published)",
  "visibility": "NoteVisibility enum (Public | Shared | Private)",
  "folderId": "guid (nullable)",
  "categoryId": "guid (nullable)",
  "sections": ["NoteSectionResponse array"]
}
```

### NoteSectionResponse (recursive)
```json
{
  "id": "guid",
  "title": "string",
  "sortOrder": 0,
  "contents": ["NoteContentResponse array"],
  "children": ["NoteSectionResponse array"]  // nested subsections
}
```

### NoteContentResponse
```json
{
  "id": "guid",
  "type": "NoteContentType enum",
  "sortOrder": 0,
  "content": "string (text content, optional)",
  "data": "string (JSON string for tables/graphs, optional)",
  "metadata": "string (rendering info, optional)"
}
```

### NoteContentType enum values (frontend must map these):
- text, definition, points, example, code, table, graph, quote
- *Plus any additional enum values defined in backend*

---

## HTTP Client Requirements (Angular)

### 1. Authorization Header
```typescript
// For protected endpoints (all except auth register/login/public notes)
headers: {
  'Authorization': `Bearer ${accessToken}`
}
```

### 2. withCredentials for Cookie-Based Auth
```typescript
// Required for refresh token cookie to be sent
{ withCredentials: true }
```
**Endpoints requiring withCredentials:**
- `/api/v1/auth/refresh`
- `/api/v1/auth/logout`
- `/api/v1/notes/my`
- `/api/v1/notes/` (POST, PUT, DELETE)
- Any endpoint where user is authenticated

### 3. Token Refresh Flow
1. Login → receives JWT + refresh token cookie set
2. Protected requests → include `Authorization: Bearer <jwt>`
3. On 401 response → call `/api/v1/auth/refresh` with `withCredentials: true`
4. After refresh, retry original request with new token
5. Logout → call `/api/v1/auth/logout` with `withCredentials: true`

---

## Error Response Formats

### 400 Bad Request
```json
{
  "type": "/errors/bad-request",
  "title": "Bad Request",
  "detail": "Error description",
  "status": 400
}
```

### 401 Unauthorized
```json
{
  "type": "/errors/unauthorized",
  "title": "Unauthorized",
  "detail": "Invalid or missing token",
  "status": 401
}
```

### 403 Forbidden
```json
{
  "type": "/errors/forbidden",
  "title": "Forbidden",
  "detail": "Insufficient permissions",
  "status": 403
}
```

### 404 Not Found
```json
{
  "type": "/errors/not-found",
  "title": "Not Found",
  "detail": "Resource does not exist or access denied",
  "status": 404
}
```

### 500 Internal Server Error
```json
{
  "type": "/errors/internal-server-error",
  "title": "Internal Server Error",
  "detail": "Unexpected error occurred",
  "status": 500
}
```

---

## Frontend Model Mapping Summary

### CreateNoteRequest → Frontend NoteCreateModel
| Backend Field | Frontend Model | Type |
|--------------|---------------|------|
| title | title | string |
| visibility | visibility | 'Public' | 'Shared' | 'Private' |
| folderId | folderId | Guid (nullable) |
| categoryId | categoryId | Guid (nullable) |
| sections | sections | SectionModel[] |
| → children | children | SectionModel[] |
| → contents | contents | ContentModel[] |
| →→ type | type | NoteContentType |
| →→ sortOrder | sortOrder | number |
| →→ content | content | string (optional) |
| →→ data | data | string (JSON, optional) |
| →→ metadata | metadata | string (optional) |

### NoteResponse → Frontend NoteResponseModel
| Backend Field | Frontend Model | Type |
|--------------|---------------|------|
| id | id | Guid |
| title | title | string |
| status | status | NoteStatus |
| visibility | visibility | NoteVisibility |
| userId | userId | Guid |
| folderId | folderId | Guid (nullable) |
| categoryId | categoryId | Guid (nullable) |
| sections | sections | NoteSectionResponse[] |

---

## Key Frontend Implementation Notes

1. **Content Block Data Serialization:** Backend stores `Data` as JSON string. Frontend must `JSON.stringify()` when sending to API and `JSON.parse()` when receiving.

2. **Section Hierarchy:** Recursive structure. Each `NoteSectionResponse` has `children` array for subsections. Map recursively when rendering.

3. **SortOrder:** Both sections and content blocks have `sortOrder` (number). Frontend must maintain sequential ordering when creating/updating.

4. **Authentication State:** 
   - Login sets JWT and refresh cookie
   - Access token in `Authorization: Bearer` header
   - Refresh cookie automatic with `withCredentials: true`
   - 401 triggers refresh flow

5. **Empty States:** API returns `[]` (empty arrays) for notes/sections/contents — frontend handles UI empty state, not API.

6. **CORS:** Backend has no CORS config. Angular must use `withCredentials: true` on all authenticated requests.

7. **Error Handling:** 
   - 401 → trigger token refresh
   - 404 → show "not found" message
   - Network errors → show generic error
   - Use centralized error handler in Angular