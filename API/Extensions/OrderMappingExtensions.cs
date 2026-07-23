using API.DTOs;
using Core.Models;

namespace API.Extensions;


public static class OrderMappingExtensions
{
    public static OrderDto ToOrderDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            SessionId = order.SessionId,
            CreatedAt = order.CreatedAt,
            TotalAmount = order.TotalAmount,
            Status = order.Status.ToString(),
            CustomerEmail = order.CustomerEmail,
            CustomerName = order.CustomerName,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price,
                ProductName = i.ProductName
            }).ToList()
        };
    }

}