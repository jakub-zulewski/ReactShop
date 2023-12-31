using API.DTOs;
using API.Entities;

using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class BasketExtensions
{
    public static BasketDTO MapBasketToDTO(this Basket basket)
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

    public static IQueryable<Basket> RetrieveBasketWithItems(
        this IQueryable<Basket> query, string buyerId)
        => query.Include(x => x.Items).ThenInclude(x => x.Product).Where(x => x.BuyerId == buyerId);
}
