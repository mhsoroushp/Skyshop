using Core.Interfaces;
using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class PaymentRepository : IPaymentRepository
{
    private readonly BookContext _context;

    public PaymentRepository(BookContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(Guid id)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Payment?> GetByOrderIdAsync(Guid orderId)
    {
        return await _context.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.OrderId == orderId);
    }

    public async Task<Payment?> GetByStripeIntentIdAsync(string stripePaymentIntentId)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.StripePaymentIntentId == stripePaymentIntentId);
    }

    public async Task AddAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment);
        await SaveChangesAsync();
    }

    public async Task UpdateAsync(Payment payment)
    {
        _context.Payments.Update(payment);
        await SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var payment = await GetByIdAsync(id);
        if (payment != null)
        {
            _context.Payments.Remove(payment);
            await SaveChangesAsync();
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
