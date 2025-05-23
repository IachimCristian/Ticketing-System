using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;

namespace TicketingSystem.Core.Interfaces
{
    public interface ITicketValidationService
    {
        Task<TicketValidationResult> ValidateTicketByQRCodeAsync(string qrCode);
    }
    
    public class TicketValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public Ticket Ticket { get; set; }
        public Event Event { get; set; }
        public Customer Customer { get; set; }
    }
} 