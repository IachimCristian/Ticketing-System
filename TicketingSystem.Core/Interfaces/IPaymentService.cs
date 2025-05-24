using System;
using System.Threading.Tasks;

namespace TicketingSystem.Core.Interfaces
{
    public interface IPaymentService
    {
        Task<Guid> ProcessPaymentAsync(Guid customerId, decimal amount, string description);
        Task<Guid> ProcessRefundAsync(Guid originalPaymentId, decimal amount);
        Task<bool> ValidatePaymentAsync(Guid paymentId);
    }
} 