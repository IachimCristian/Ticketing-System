using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;

namespace TicketingSystem.Core.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment> GetByIdAsync(Guid id);
        Task<IEnumerable<Payment>> GetByCustomerIdAsync(Guid customerId);
        Task<IEnumerable<Payment>> GetRefundsForPaymentAsync(Guid paymentId);
        Task<Payment> AddAsync(Payment payment);
        Task UpdateAsync(Payment payment);
    }
} 