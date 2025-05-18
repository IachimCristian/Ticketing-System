using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Infrastructure.Data;

namespace TicketingSystem.Infrastructure.Repositories
{
    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        public PaymentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByCustomerAsync(Guid customerId)
        {
            return await _dbSet
                .Include(p => p.Tickets)
                .Where(p => p.CustomerId == customerId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<Payment> GetPaymentByTransactionIdAsync(string transactionId)
        {
            return await _dbSet
                .Include(p => p.Tickets)
                .FirstOrDefaultAsync(p => p.TransactionId == transactionId);
        }

        public async Task<bool> ProcessRefundAsync(Guid paymentId)
        {
            var payment = await _dbSet.FindAsync(paymentId);
            if (payment == null || payment.Status != "Completed")
            {
                return false;
            }

            payment.Status = "Refunded";
            await SaveChangesAsync();
            return true;
        }
    }
} 