using System;
using System.Collections.Generic;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Core.Services.Strategies;

namespace TicketingSystem.Core.Services
{
    public class PaymentStrategyFactory
    {
        private readonly Dictionary<string, IPaymentStrategy> _strategies;
        
        public PaymentStrategyFactory()
        {
            _strategies = new Dictionary<string, IPaymentStrategy>(StringComparer.OrdinalIgnoreCase)
            {
                { "creditcard", new CreditCardPaymentStrategy() },
                { "paypal", new PayPalPaymentStrategy() }
            };
        }
        
        public IPaymentStrategy CreatePaymentStrategy(string paymentMethod)
        {
            if (string.IsNullOrEmpty(paymentMethod) || !_strategies.ContainsKey(paymentMethod))
            {
                throw new ArgumentException($"Unsupported payment method: {paymentMethod}");
            }
            
            return _strategies[paymentMethod];
        }
        
        public IEnumerable<string> GetSupportedPaymentMethods()
        {
            return _strategies.Keys;
        }
    }
} 