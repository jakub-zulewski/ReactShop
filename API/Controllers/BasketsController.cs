using API.Data;
using API.DTOs;
using API.Entities;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class BasketsController(StoreContext storeContext) : BaseApiController
{
    private readonly StoreContext _storeContext = storeContext;
    private readonly string _cookieName = "buyerId";

    [HttpGet]
    public async Task<ActionResult<BasketDTO>> GetBasket()
    {
        var basket = await RetrieveBasket();

        return basket is null ? NotFound() : global::API.Controllers.BasketsController.MapBasketToDTO(basket);
    }

    [HttpPost]
    public async Task<ActionResult<BasketDTO>> AddItemToBasket(int productId, int quantity)
    {
        var basket = await RetrieveBasket() ?? await CreateBasket();
        var product = await _storeContext.Products.FindAsync(productId);

        if (product is null)
            return BadRequest(new ProblemDetails { Title = "Product not found." });

        basket.AddItem(product, quantity);

        var result = await _storeContext.SaveChangesAsync() > 0;

        return result
            ? CreatedAtAction(nameof(GetBasket), global::API.Controllers.BasketsController.MapBasketToDTO(basket))
            : BadRequest(new ProblemDetails { Title = "Problem saving item to basket." });
    }

    [HttpDelete]
    public async Task<ActionResult> RemoveBasketItem(int productId, int quantity)
    {
        var basket = await RetrieveBasket();

        if (basket is null)
            return NotFound();

        basket.RemoveItem(productId, quantity);

        var result = await _storeContext.SaveChangesAsync() > 0;

        return result
            ? Ok()
            : BadRequest(new ProblemDetails { Title = "Problem removing item from the basket." });
    }

    private async Task<Basket> RetrieveBasket()
        => await _storeContext.Baskets
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.BuyerId == Request.Cookies[_cookieName]);

    private async Task<Basket> CreateBasket()
    {
        var buyerId = Guid.NewGuid().ToString();
        var cookieOptions = new CookieOptions
        {
            IsEssential = true,
            Expires = DateTime.Now.AddDays(30)
        };

        Response.Cookies.Append(_cookieName, buyerId, cookieOptions);

        var basket = new Basket { BuyerId = buyerId };

        await _storeContext.Baskets.AddAsync(basket);

        return basket;
    }

    private static BasketDTO MapBasketToDTO(Basket basket)
    {
        return new BasketDTO
        {
            Id = basket.Id,
            BuyerId = basket.BuyerId,
            Items = basket.Items.Select(item => new BasketItemDTO
            {
                ProductId = item.ProductId,
                Name = item.Product.Name,
                Price = item.Product.Price,
                Brand = item.Product.Brand,
                Type = item.Product.Type,
                PictureUrl = item.Product.PictureUrl,
                Quantity = item.Quantity
            }).ToList()
        };
    }
}
