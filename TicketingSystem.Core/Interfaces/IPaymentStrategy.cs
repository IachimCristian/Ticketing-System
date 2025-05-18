using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;

namespace TicketingSystem.Core.Interfaces
{
    public interface IPaymentStrategy
    {
        string Name { get; }
        Task<Payment> ProcessPaymentAsync(Customer customer, decimal amount);
        Task<bool> RefundPaymentAsync(Payment payment);
    }
} 