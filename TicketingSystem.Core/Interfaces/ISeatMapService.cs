using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using TicketingSystem.Core.Entities;

namespace TicketingSystem.Core.Interfaces
{
    public interface ISeatMapService
    {
        Task<SeatMap> GetSeatMapForEventAsync(Guid eventId);
        Task<bool> IsSeatAvailableAsync(Guid eventId, int row, int column);
        Task<IEnumerable<Seat>> GetAvailableSeatsAsync(Guid eventId);
        Task<Seat> ReserveSeatAsync(Guid eventId, int row, int column, Guid customerId);
        Task<bool> ReleaseSeatReservationAsync(Guid eventId, int row, int column);
    }
    
    public class Seat
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public string Label { get; set; } // e.g., "A12"
        public string Section { get; set; } // e.g., "Premium"
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsReserved { get; set; }
    }
} 