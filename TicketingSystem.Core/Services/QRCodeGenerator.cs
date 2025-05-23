using System;

namespace TicketingSystem.Core.Services
{
    public class QRCodeGenerator
    {
        private static QRCodeGenerator _instance;
        public static QRCodeGenerator Instance => _instance ??= new QRCodeGenerator();
        
        private QRCodeGenerator() { }
        
        public string GenerateQRCode(Guid ticketId, string eventTitle, DateTime eventDate)
        {
            // In a real-world scenario, you might want to use a proper QR code library
            // and generate an actual image or a string that represents a QR code
            // For now, we'll create a unique string that can be used to validate tickets
            
            string dateCode = eventDate.ToString("yyyyMMdd");
            string eventCode = eventTitle.Substring(0, Math.Min(eventTitle.Length, 3)).ToUpper();
            string uniqueCode = ticketId.ToString().Substring(0, 8);
            
            // Format: EVENT_CODE-DATE-TICKET_ID_PART
            return $"{eventCode}-{dateCode}-{uniqueCode}";
        }
        
        public bool ValidateQRCode(string qrCode, out Guid ticketId)
        {
            ticketId = Guid.Empty;
            
            // Simple validation - check format
            if (string.IsNullOrEmpty(qrCode) || !qrCode.Contains("-") || qrCode.Split('-').Length != 3)
            {
                return false;
            }
            
            try
            {
                string[] parts = qrCode.Split('-');
                string uniqueCode = parts[2];
                
                // In a real implementation, we'd decode the ticketId from the QR code
                // For this sample, we'll assume the uniqueCode is part of the ticket's GUID
                // This is just a simulation for educational purposes
                
                // Create a sample GUID that contains the uniqueCode to simulate retrieval
                string guidTemplate = $"{uniqueCode}-0000-0000-0000-000000000000";
                ticketId = Guid.Parse(guidTemplate);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
} 