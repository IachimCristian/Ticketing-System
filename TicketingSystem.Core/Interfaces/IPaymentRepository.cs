using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;

namespace TicketingSystem.Core.Interfaces
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetPaymentsByCustomerAsync(Guid customerId);
        Task<Payment> GetPaymentByTransactionIdAsync(string transactionId);
        Task<bool> ProcessRefundAsync(Guid paymentId);
    }
} 