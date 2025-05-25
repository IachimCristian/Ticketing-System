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
            // For this demo, we'll simulate a realistic PayPal payment flow
            
            // Simulate PayPal API call delay
            await Task.Delay(1200);
            
            // Generate a realistic PayPal transaction ID
            var transactionId = $"PAY-{Guid.NewGuid().ToString("N")[..16].ToUpper()}";
            
            var payment = new Payment
            {
                Amount = amount,
                CustomerId = customer.Id,
                Customer = customer,
                PaymentMethod = Name,
                TransactionDate = DateTime.UtcNow,
                Description = $"PayPal payment for event ticket - Transaction ID: {transactionId}",
                Status = "Completed"
            };
            
            // Simulate a small chance of payment failure for realism
            var random = new Random();
            if (random.Next(1, 101) <= 5) // 5% chance of failure
            {
                payment.Status = "Failed";
                payment.Description += " - Payment declined by PayPal";
                throw new InvalidOperationException("PayPal payment was declined. Please try again or use a different payment method.");
            }
            
            return payment;
        }
        
        public async Task<bool> RefundPaymentAsync(Payment payment)
        {
            if (payment.Status != "Completed")
            {
                return false;
            }
            
            // Simulate PayPal refund processing
            await Task.Delay(800);
            
            // Generate refund transaction ID
            var refundId = $"REF-{Guid.NewGuid().ToString("N")[..16].ToUpper()}";
            
            payment.Status = "Refunded";
            payment.Description += $" - Refunded via PayPal (Refund ID: {refundId})";
            
            return true;
        }
    }
} 