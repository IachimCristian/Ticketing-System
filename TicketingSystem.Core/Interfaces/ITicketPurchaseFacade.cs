using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;

namespace TicketingSystem.Core.Interfaces
{
    public interface ITicketPurchaseFacade
    {
        Task<Ticket> PurchaseTicketAsync(
            Guid eventId,
            Guid customerId,
            decimal price,
            int? seatRow,
            int? seatColumn,
            string paymentMethod);
            
        Task<bool> CancelTicketAsync(Guid ticketId, Guid customerId);
    }
} 