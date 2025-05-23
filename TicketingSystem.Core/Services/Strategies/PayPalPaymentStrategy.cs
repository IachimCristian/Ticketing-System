using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Core.Services.Strategies
{
    public class PayPalPaymentStrategy : IPaymentStrategy
    {
        public string Name => "PayPal";
        
        public async Task<Payment> ProcessPaymentAsync(Customer customer, decimal amount)
        {
            // In a real application, this would interact with PayPal's API
            // For this example, we'll just simulate a successful payment
            
            var payment = new Payment
            {
                Amount = amount,
                CustomerId = customer.Id,
                Customer = customer,
                PaymentMethod = Name,
                TransactionDate = DateTime.UtcNow,
                Description = $"PayPal payment for {amount:C}",
                Status = "Completed"
            };
            
            // Simulate processing delay
            await Task.Delay(700);
            
            return payment;
        }
        
        public async Task<bool> RefundPaymentAsync(Payment payment)
        {
            if (payment.Status != "Completed")
            {
                return false;
            }
            
            // Simulate refund processing
            await Task.Delay(700);
            
            payment.Status = "Refunded";
            return true;
        }
    }
} 