using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;

namespace TicketingSystem.Core.Interfaces
{
    public interface ITicketRepository : IRepository<Ticket>
    {
        Task<IEnumerable<Ticket>> GetTicketsByCustomerAsync(Guid customerId);
        Task<IEnumerable<Ticket>> GetTicketsByEventAsync(Guid eventId);
        Task<Ticket> GetTicketByQRCodeAsync(string qrCode);
        Task<bool> ValidateTicketAsync(string qrCode);
    }
} 