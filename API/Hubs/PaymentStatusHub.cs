using Microsoft.AspNetCore.SignalR;

namespace API.Hubs;

public class PaymentStatusHub : Hub
{
    public static string OrderGroup(Guid orderId) => $"order-{orderId}";

    public Task JoinOrderGroup(string orderId)
    {
        if (!Guid.TryParse(orderId, out var parsedOrderId))
            throw new HubException("Invalid order id");

        return Groups.AddToGroupAsync(Context.ConnectionId, OrderGroup(parsedOrderId));
    }

    public Task LeaveOrderGroup(string orderId)
    {
        if (!Guid.TryParse(orderId, out var parsedOrderId))
            throw new HubException("Invalid order id");

        return Groups.RemoveFromGroupAsync(Context.ConnectionId, OrderGroup(parsedOrderId));
    }
}