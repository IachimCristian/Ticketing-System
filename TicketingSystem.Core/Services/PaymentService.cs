using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Core.Entities;

namespace TicketingSystem.Core.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(IPaymentRepository paymentRepository, ILogger<PaymentService> logger)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Guid> ProcessPaymentAsync(Guid customerId, decimal amount, string description)
        {
            _logger.LogInformation("Processing payment for customer {CustomerId}, amount: {Amount}", customerId, amount);
            
            try
            {
                // In a real application, you would integrate with a payment gateway here
                // For demo purposes, we'll create a payment record and assume it's successful
                
                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customerId,
                    Amount = amount,
                    Status = "Completed",
                    TransactionDate = DateTime.UtcNow,
                    Description = description,
                    PaymentMethod = "Credit Card" // Default for demo
                };
                
                await _paymentRepository.AddAsync(payment);
                
                _logger.LogInformation("Payment processed successfully. Payment ID: {PaymentId}", payment.Id);
                
                return payment.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for customer {CustomerId}", customerId);
                throw new ApplicationException("Payment processing failed", ex);
            }
        }

        public async Task<Guid> ProcessRefundAsync(Guid originalPaymentId, decimal amount)
        {
            _logger.LogInformation("Processing refund for payment {PaymentId}, amount: {Amount}", originalPaymentId, amount);
            
            try
            {
                // Get original payment
                var originalPayment = await _paymentRepository.GetByIdAsync(originalPaymentId);
                
                if (originalPayment == null)
                {
                    throw new ArgumentException("Original payment not found", nameof(originalPaymentId));
                }
                
                // In a real application, you would integrate with a payment gateway here
                // For demo purposes, we'll create a refund record and assume it's successful
                
                var refund = new Payment
                {
                    Id = Guid.NewGuid(),
                    CustomerId = originalPayment.CustomerId,
                    Amount = -amount, // Negative amount indicates a refund
                    Status = "Completed",
                    TransactionDate = DateTime.UtcNow,
                    Description = $"Refund for payment {originalPaymentId}",
                    PaymentMethod = originalPayment.PaymentMethod,
                    RelatedPaymentId = originalPaymentId
                };
                
                await _paymentRepository.AddAsync(refund);
                
                _logger.LogInformation("Refund processed successfully. Refund ID: {RefundId}", refund.Id);
                
                return refund.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for payment {PaymentId}", originalPaymentId);
                throw new ApplicationException("Refund processing failed", ex);
            }
        }

        public async Task<bool> ValidatePaymentAsync(Guid paymentId)
        {
            try
            {
                var payment = await _paymentRepository.GetByIdAsync(paymentId);
                return payment != null && payment.Status == "Completed";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating payment {PaymentId}", paymentId);
                return false;
            }
        }
    }
} 