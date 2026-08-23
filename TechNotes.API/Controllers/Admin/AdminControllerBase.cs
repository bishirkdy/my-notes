using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechNotes.API.Authorization;

namespace TechNotes.API.Controllers.Admin;

// Base class for future administrator-only endpoints, such as api/admin/users.
[ApiController]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin/[controller]")]
public abstract class AdminControllerBase : ControllerBase
{
}
