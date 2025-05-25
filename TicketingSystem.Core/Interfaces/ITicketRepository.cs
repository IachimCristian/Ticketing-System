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
        Task<IEnumerable<Ticket>> GetTicketsByStatusAsync(string status);
        Task<bool> IsTicketAvailableAsync(Guid eventId);
        Task<Ticket> GetTicketByNumberAsync(string ticketNumber);
        Task<Ticket> GetTicketByQRCodeAsync(string qrCode);
        Task<bool> ValidateTicketAsync(string qrCode);
        Task<Ticket> GetTicketBySeatAsync(Guid eventId, int seatRow, int seatColumn);
    }
} 