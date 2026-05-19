using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Goatify.Infrastructure;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;

    public AdminController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpPost("make-admin")]
    public async Task<IActionResult> MakeAdmin(string username)
    {
        var user = await _userManager.FindByNameAsync(username);

        if (user == null)
            return NotFound("User not found");

        await _userManager.AddToRoleAsync(user, "Admin");

        return Ok($"{username} is now Admin");
    }

    [HttpPost("remove-admin")]
    public async Task<IActionResult> RemoveAdmin(string username)
    {
        var user = await _userManager.FindByNameAsync(username);

        if (user == null)
            return NotFound("User not found");

        await _userManager.RemoveFromRoleAsync(user, "Admin");

        return Ok($"{username} is not Admin anymore");
    }
}
