using API.DTOs;
using API.Entities.OrderAggregate;

using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class OrderExtensions
{
    public static IQueryable<OrderDTO> ProjectOrderToOrderDTO(this IQueryable<Order> query)
        => query.AsNoTracking().Select(order => new OrderDTO
        {
            Id = order.Id,
            BuyerId = order.BuyerId,
            DeliveryFee = order.DeliveryFee,
            OrderDate = order.OrderDate,
            OrderStatus = order.OrderStatus.ToString(),
            ShippingAddress = order.ShippingAddress,
            Subtotal = order.Subtotal,
            Total = order.GetTotal(),
            OrderItems = order.OrderItems.Select(item => new OrderItemDTO
            {
                PictureUrl = item.ItemOrdered.PictureUrl,
                Name = item.ItemOrdered.Name,
                Price = item.Price,
                ProductId = item.ItemOrdered.ProductId,
                Quantity = item.Quantity
            }).ToList()
        });
}
