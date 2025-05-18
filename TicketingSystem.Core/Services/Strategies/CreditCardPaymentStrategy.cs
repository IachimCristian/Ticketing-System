using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Core.Services.Strategies
{
    public class CreditCardPaymentStrategy : IPaymentStrategy
    {
        public string Name => "Credit Card";
        
        public async Task<Payment> ProcessPaymentAsync(Customer customer, decimal amount)
        {
            // In a real application, this would interact with a payment gateway
            // For this example, we'll just simulate a successful payment
            
            var payment = new Payment
            {
                Amount = amount,
                CustomerId = customer.Id,
                Customer = customer,
                PaymentMethod = Name,
                TransactionId = $"CC-{Guid.NewGuid().ToString().Substring(0, 8)}",
                Status = "Completed"
            };
            
            // Simulate processing delay
            await Task.Delay(500);
            
            return payment;
        }
        
        public async Task<bool> RefundPaymentAsync(Payment payment)
        {
            if (payment.Status != "Completed")
            {
                return false;
            }
            
            // Simulate refund processing
            await Task.Delay(500);
            
            payment.Status = "Refunded";
            return true;
        }
    }
} 