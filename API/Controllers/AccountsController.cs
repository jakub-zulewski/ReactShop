using API.DTOs;
using API.Entities;
using API.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class AccountsController(UserManager<User> userManager, TokenService tokenService)
    : BaseApiController
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly TokenService _tokenService = tokenService;

    [Authorize]
    [HttpGet("currentUser")]
    public async Task<ActionResult<UserDTO>> GetCurrentUser()
    {
        var user = await _userManager.FindByNameAsync(User.Identity.Name);

        return new UserDTO { Email = user.Email, Token = await _tokenService.GenerateToken(user) };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
    {
        var user = await _userManager.FindByNameAsync(loginDTO.Username);

        return user is null || !await _userManager.CheckPasswordAsync(user, loginDTO.Password)
            ? Unauthorized()
            : new UserDTO { Email = user.Email, Token = await _tokenService.GenerateToken(user) };
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterDTO registerDTO)
    {
        var user = new User
        {
            UserName = registerDTO.Username,
            Email = registerDTO.Email
        };
        var result = await _userManager.CreateAsync(user, registerDTO.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);

                return ValidationProblem();
            }
        }

        await _userManager.AddToRoleAsync(user, "Member");

        return CreatedAtAction(nameof(GetCurrentUser), null);
    }
}
