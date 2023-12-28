using API.Configs;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountsController(
    UserManager<User> userManager, TokenService tokenService, StoreContext storeContext)
    : BaseApiController
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly TokenService _tokenService = tokenService;
    private readonly StoreContext _storeContext = storeContext;

    [Authorize]
    [HttpGet("currentUser")]
    public async Task<ActionResult<UserDTO>> GetCurrentUser()
    {
        var user = await _userManager.FindByNameAsync(User.Identity.Name);
        var userBasket = await RetrieveBasket(user.UserName);

        return new UserDTO
        {
            Email = user.Email,
            Token = await _tokenService.GenerateToken(user),
            Basket = userBasket?.MapBasketToDTO()
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
    {
        var user = await _userManager.FindByNameAsync(loginDTO.Username);

        if (user is null || !await _userManager.CheckPasswordAsync(user, loginDTO.Password))
            return Unauthorized();

        var userBasket = await RetrieveBasket(loginDTO.Username);
        var anonymousBasket = await RetrieveBasket(
            Request.Cookies[CookieConstants.BASKET_COOKIE_NAME]);

        if (anonymousBasket is not null)
        {
            if (userBasket is not null)
                _storeContext.Baskets.Remove(userBasket);

            anonymousBasket.BuyerId = user.UserName;

            Response.Cookies.Delete(CookieConstants.BASKET_COOKIE_NAME);

            await _storeContext.SaveChangesAsync();
        }

        return new UserDTO
        {
            Email = user.Email,
            Token = await _tokenService.GenerateToken(user),
            Basket = anonymousBasket is not null
                ? anonymousBasket.MapBasketToDTO()
                : userBasket?.MapBasketToDTO()
        };
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

    private async Task<Basket> RetrieveBasket(string buyerId)
    {
        if (string.IsNullOrEmpty(buyerId))
        {
            Response.Cookies.Delete(CookieConstants.BASKET_COOKIE_NAME);

            return null;
        }

        return await _storeContext.Baskets
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.BuyerId == buyerId);
    }
}
