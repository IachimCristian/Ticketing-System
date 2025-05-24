using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Core.Services;
using TicketingSystem.Infrastructure.Data;

namespace TicketingSystem.Infrastructure.Repositories
{
    public class TicketRepository : Repository<Ticket>, ITicketRepository
    {
        public TicketRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByCustomerAsync(Guid customerId)
        {
            return await _dbSet
                .Include(t => t.Event)
                .Where(t => t.CustomerId == customerId)
                .OrderByDescending(t => t.PurchaseDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByEventAsync(Guid eventId)
        {
            return await _dbSet
                .Where(t => t.EventId == eventId)
                .ToListAsync();
        }

        public async Task<Ticket> GetTicketByQRCodeAsync(string qrCode)
        {
            return await _dbSet
                .FirstOrDefaultAsync(t => t.QRCode == qrCode);
        }

        public async Task<bool> ValidateTicketAsync(string qrCode)
        {
            var ticket = await GetTicketByQRCodeAsync(qrCode);
            if (ticket == null || ticket.Status != "Sold")
            {
                return false;
            }

            Guid extractedTicketId;
            bool isValid = QRCodeGenerator.Instance.ValidateQRCode(qrCode, out extractedTicketId);
            
            return isValid;
        }
    }
} 