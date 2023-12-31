using API.Data;
using API.DTOs;
using API.Entities;
using API.Entities.OrderAggregate;
using API.Extensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize]
public class OrdersController(StoreContext storeContext) : BaseApiController
{
    private readonly StoreContext _storeContext = storeContext;

    [HttpGet]
    public async Task<ActionResult<List<OrderDTO>>> GetOrders()
        => await _storeContext.Orders
            .ProjectOrderToOrderDTO()
            .Where(x => x.BuyerId == User.Identity.Name)
            .ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDTO>> GetOrder(int id)
        => await _storeContext.Orders
            .ProjectOrderToOrderDTO()
            .Where(x => x.BuyerId == User.Identity.Name && x.Id == id)
            .FirstOrDefaultAsync();

    [HttpPost]
    public async Task<ActionResult<int>> CreateOrder(CreateOrderDTO createOrderDTO)
    {
        var basket = await _storeContext.Baskets
            .RetrieveBasketWithItems(User.Identity.Name)
            .FirstOrDefaultAsync();

        if (basket is null)
            return BadRequest(new ProblemDetails { Title = "Could not locate basket." });

        var items = new List<OrderItem>();

        foreach (var item in basket.Items)
        {
            var productItem = await _storeContext.Products.FindAsync(item.ProductId);
            var itemOrdered = new ProductItemOrdered
            {
                ProductId = productItem.Id,
                Name = productItem.Name,
                PictureUrl = productItem.PictureUrl
            };
            var orderItem = new OrderItem
            {
                ItemOrdered = itemOrdered,
                Price = productItem.Price,
                Quantity = item.Quantity
            };

            items.Add(orderItem);

            productItem.QuantityInStock -= item.Quantity;
        }

        var subtotal = items.Sum(x => x.Price * x.Quantity);
        var deliveryFee = subtotal > 10000 ? 0 : 1000;
        var order = new Order
        {
            OrderItems = items,
            BuyerId = User.Identity.Name,
            ShippingAddress = createOrderDTO.ShippingAddress,
            Subtotal = subtotal,
            DeliveryFee = deliveryFee
        };

        await _storeContext.Orders.AddAsync(order);

        _storeContext.Baskets.Remove(basket);

        if (createOrderDTO.SaveAddress)
        {
            var user = await _storeContext.Users
                .FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);

            user.Address = new UserAddress
            {
                FullName = createOrderDTO.ShippingAddress.FullName,
                Address1 = createOrderDTO.ShippingAddress.Address1,
                Address2 = createOrderDTO.ShippingAddress.Address2,
                City = createOrderDTO.ShippingAddress.City,
                Country = createOrderDTO.ShippingAddress.Country,
                State = createOrderDTO.ShippingAddress.State,
                Zip = createOrderDTO.ShippingAddress.Zip
            };

            _storeContext.Users.Update(user);
        }

        var result = await _storeContext.SaveChangesAsync() > 0;

        return result
            ? CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order.Id)
            : BadRequest("Problem creating order.");
    }
}
