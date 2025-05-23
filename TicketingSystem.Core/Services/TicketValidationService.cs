using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Core.Services
{
    public class TicketValidationService : ITicketValidationService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly QRCodeGenerator _qrCodeGenerator;
        
        public TicketValidationService(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _qrCodeGenerator = QRCodeGenerator.Instance;
        }
        
        public async Task<TicketValidationResult> ValidateTicketByQRCodeAsync(string qrCode)
        {
            var result = new TicketValidationResult
            {
                IsValid = false,
                Message = "Invalid QR code"
            };
            
            if (string.IsNullOrEmpty(qrCode))
            {
                return result;
            }
            
            try
            {
                // Try to extract the ticket ID from QR code
                if (!_qrCodeGenerator.ValidateQRCode(qrCode, out Guid ticketId))
                {
                    result.Message = "QR code format is invalid";
                    return result;
                }
                
                // Find the ticket in the database
                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                {
                    // Try to find by QR code string
                    ticket = await _ticketRepository.GetTicketByQRCodeAsync(qrCode);
                    
                    if (ticket == null)
                    {
                        result.Message = "Ticket not found";
                        return result;
                    }
                }
                
                // Check ticket status
                if (ticket.Status == "Cancelled")
                {
                    result.Message = "Ticket has been cancelled";
                    return result;
                }
                
                if (ticket.Status == "Used")
                {
                    result.Message = "Ticket has already been used";
                    return result;
                }
                
                // Check if event has already passed
                if (ticket.Event != null && ticket.Event.EndDate < DateTime.UtcNow)
                {
                    result.Message = "Event has already ended";
                    return result;
                }
                
                // Ticket is valid
                result.IsValid = true;
                result.Message = "Ticket is valid";
                result.Ticket = ticket;
                result.Event = ticket.Event;
                result.Customer = ticket.Customer;
                
                return result;
            }
            catch (Exception ex)
            {
                result.Message = $"Error validating ticket: {ex.Message}";
                return result;
            }
        }
    }
} 