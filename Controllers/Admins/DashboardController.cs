using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/admin/[controller]")]
[ApiController]
[Authorize]
public class DashboardController : ControllerBase
{
    [HttpGet]
    public IActionResult GetDashboard()
    {
        return Ok(new { message = "Welcome to Admin Dashboard!" });
    }
}
