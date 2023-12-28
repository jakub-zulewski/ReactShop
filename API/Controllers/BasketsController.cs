using API.Configs;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class BasketsController(StoreContext storeContext) : BaseApiController
{
    private readonly StoreContext _storeContext = storeContext;

    [HttpGet]
    public async Task<ActionResult<BasketDTO>> GetBasket()
    {
        var basket = await RetrieveBasket(GetBuyerId());

        return basket is null
            ? NotFound()
            : basket.MapBasketToDTO();
    }

    [HttpPost]
    public async Task<ActionResult<BasketDTO>> AddItemToBasket(int productId, int quantity)
    {
        var basket = await RetrieveBasket(GetBuyerId()) ?? await CreateBasket();
        var product = await _storeContext.Products.FindAsync(productId);

        if (product is null)
            return BadRequest(new ProblemDetails { Title = "Product not found." });

        basket.AddItem(product, quantity);

        var result = await _storeContext.SaveChangesAsync() > 0;

        return result
            ? CreatedAtAction(nameof(GetBasket), basket.MapBasketToDTO())
            : BadRequest(new ProblemDetails { Title = "Problem saving item to basket." });
    }

    [HttpDelete]
    public async Task<ActionResult> RemoveBasketItem(int productId, int quantity)
    {
        var basket = await RetrieveBasket(GetBuyerId());

        if (basket is null)
            return NotFound();

        basket.RemoveItem(productId, quantity);

        var result = await _storeContext.SaveChangesAsync() > 0;

        return result
            ? Ok()
            : BadRequest(new ProblemDetails { Title = "Problem removing item from the basket." });
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

    private async Task<Basket> CreateBasket()
    {
        var buyerId = User.Identity?.Name;

        if (string.IsNullOrEmpty(buyerId))
        {
            buyerId = Guid.NewGuid().ToString();

            var cookieOptions = new CookieOptions
            {
                IsEssential = true,
                Expires = DateTime.Now.AddDays(30)
            };

            Response.Cookies.Append(CookieConstants.BASKET_COOKIE_NAME, buyerId, cookieOptions);
        }

        var basket = new Basket { BuyerId = buyerId };

        await _storeContext.Baskets.AddAsync(basket);

        return basket;
    }

    private string GetBuyerId()
        => User.Identity?.Name ?? Request.Cookies[CookieConstants.BASKET_COOKIE_NAME];
}
