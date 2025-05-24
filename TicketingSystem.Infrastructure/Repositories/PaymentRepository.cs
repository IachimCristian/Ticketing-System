using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Infrastructure.Data;

namespace TicketingSystem.Infrastructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;

        public PaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Payment> GetByIdAsync(Guid id)
        {
            return await _context.Payments
                .Include(p => p.Customer)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Payment>> GetByCustomerIdAsync(Guid customerId)
        {
            return await _context.Payments
                .Where(p => p.CustomerId == customerId)
                .OrderByDescending(p => p.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetRefundsForPaymentAsync(Guid paymentId)
        {
            return await _context.Payments
                .Where(p => p.RelatedPaymentId == paymentId)
                .OrderByDescending(p => p.TransactionDate)
                .ToListAsync();
        }

        public async Task<Payment> AddAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
        }
    }
} 