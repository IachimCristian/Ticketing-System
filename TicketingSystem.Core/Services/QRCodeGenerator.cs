using System;

namespace TicketingSystem.Core.Services
{
    public class QRCodeGenerator
    {
        private static readonly Lazy<QRCodeGenerator> _instance = new Lazy<QRCodeGenerator>(() => new QRCodeGenerator());
        
        public static QRCodeGenerator Instance => _instance.Value;
        
        private QRCodeGenerator() { }
        
        public string GenerateQRCode(Guid ticketId, string eventName, DateTime eventDate)
        {
            // In a real application, this would generate an actual QR code image or data
            // For the purpose of this example, we'll just return a string representation
            string data = $"{ticketId}|{eventName}|{eventDate.ToString("yyyy-MM-dd")}";
            string encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(data));
            return encoded;
        }
        
        public bool ValidateQRCode(string qrCode, Guid ticketId)
        {
            try
            {
                string decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(qrCode));
                string[] parts = decoded.Split('|');
                return parts.Length > 0 && Guid.Parse(parts[0]) == ticketId;
            }
            catch
            {
                return false;
            }
        }
    }
} 