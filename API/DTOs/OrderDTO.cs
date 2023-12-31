using System.ComponentModel.DataAnnotations;

using API.Entities.OrderAggregate;

namespace API.DTOs;

public class OrderDTO
{
    public int Id { get; set; }
    public string BuyerId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public List<OrderItemDTO> OrderItems { get; set; }
    public long Subtotal { get; set; }
    public long DeliveryFee { get; set; }
    public string OrderStatus { get; set; }
    public long Total { get; set; }
    public ShippingAddress ShippingAddress { get; set; }
}
