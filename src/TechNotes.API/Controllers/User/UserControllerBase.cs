using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechNotes.API.Controllers.User;

// Base class for future normal-user endpoints, such as api/user/notes.
[ApiController]
[Authorize]
[Route("api/user/[controller]")]
public abstract class UserControllerBase : ControllerBase
{
}
