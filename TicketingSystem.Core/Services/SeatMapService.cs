using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Core.Services
{
    public class SeatMapService : ISeatMapService
    {
        private readonly IRepository<SeatMap> _seatMapRepository;
        private readonly IEventService _eventService;
        
        public SeatMapService(
            IRepository<SeatMap> seatMapRepository,
            IEventService eventService)
        {
            _seatMapRepository = seatMapRepository ?? throw new ArgumentNullException(nameof(seatMapRepository));
            _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
        }
        
        public async Task<SeatMap> GetSeatMapForEventAsync(Guid eventId)
        {
            // For now, we'll create a new seat map on demand instead of querying the repository
            // This avoids issues if SeatMap isn't set up in the database yet
            
            // Create a theater/airplane style layout with rows and columns
            return new SeatMap
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Rows = 4,
                Columns = 20,
                Sections = new List<SeatSection>
                {
                    new SeatSection 
                    { 
                        Id = Guid.NewGuid(),
                        Name = "Standard", 
                        PriceMultiplier = 1.0m,
                        Color = "#3498db", // Blue
                        StartRow = 1, 
                        EndRow = 4, 
                        StartColumn = 0, 
                        EndColumn = 14
                    },
                    new SeatSection 
                    { 
                        Id = Guid.NewGuid(),
                        Name = "Premium", 
                        PriceMultiplier = 1.5m,
                        Color = "#9b59b6", // Purple
                        StartRow = 3, 
                        EndRow = 3, 
                        StartColumn = 8, 
                        EndColumn = 12
                    },
                    new SeatSection 
                    { 
                        Id = Guid.NewGuid(),
                        Name = "VIP", 
                        PriceMultiplier = 2.0m,
                        Color = "#e74c3c", // Red
                        StartRow = 4, 
                        EndRow = 4, 
                        StartColumn = 15, 
                        EndColumn = 19
                    }
                }
            };
        }
        
        public async Task<bool> IsSeatAvailableAsync(Guid eventId, int row, int column)
        {
            var seatMap = await GetSeatMapForEventAsync(eventId);
            if (seatMap == null)
            {
                return false;
            }
            
            // In a real implementation, this would check the actual availability
            // For now, we'll use the simplified method in SeatMap
            return seatMap.IsSeatAvailable(row, column);
        }
        
        public async Task<IEnumerable<Seat>> GetAvailableSeatsAsync(Guid eventId)
        {
            var seatMap = await GetSeatMapForEventAsync(eventId);
            if (seatMap == null)
            {
                return new List<Seat>();
            }
            
            var availableSeats = new List<Seat>();
            
            // In a real implementation, this would decode the SeatLayout JSON
            // to determine which seats are available
            // For demonstration purposes, we'll create a simple grid
            for (int row = 1; row <= seatMap.Rows; row++)
            {
                for (int col = 0; col < seatMap.Columns; col++)
                {
                    if (seatMap.IsSeatAvailable(row, col))
                    {
                        // Find which section this seat belongs to
                        var section = seatMap.Sections.FirstOrDefault(s => 
                            row >= s.StartRow && row <= s.EndRow && 
                            col >= s.StartColumn && col <= s.EndColumn);
                        
                        var @event = await _eventService.GetEventByIdAsync(eventId);
                        decimal price = @event?.TicketPrice ?? 0;
                        if (section != null)
                        {
                            price *= section.PriceMultiplier;
                        }
                        
                        availableSeats.Add(new Seat
                        {
                            Row = row,
                            Column = col,
                            Label = seatMap.GetSeatLabel(row, col),
                            Section = section?.Name ?? "Standard",
                            Price = price,
                            IsAvailable = true,
                            IsReserved = false
                        });
                    }
                }
            }
            
            return availableSeats;
        }
        
        public async Task<Seat> ReserveSeatAsync(Guid eventId, int row, int column, Guid customerId)
        {
            var seatMap = await GetSeatMapForEventAsync(eventId);
            if (seatMap == null || !seatMap.IsSeatAvailable(row, column))
            {
                return null;
            }
            
            // In a real implementation, this would update the seat availability in the database
            // Find which section this seat belongs to
            var section = seatMap.Sections.FirstOrDefault(s => 
                row >= s.StartRow && row <= s.EndRow && 
                column >= s.StartColumn && column <= s.EndColumn);
            
            var @event = await _eventService.GetEventByIdAsync(eventId);
            decimal price = @event?.TicketPrice ?? 0;
            if (section != null)
            {
                price *= section.PriceMultiplier;
            }
            
            return new Seat
            {
                Row = row,
                Column = column,
                Label = seatMap.GetSeatLabel(row, column),
                Section = section?.Name ?? "Standard",
                Price = price,
                IsAvailable = false,
                IsReserved = true
            };
        }
        
        public async Task<bool> ReleaseSeatReservationAsync(Guid eventId, int row, int column)
        {
            var seatMap = await GetSeatMapForEventAsync(eventId);
            if (seatMap == null)
            {
                return false;
            }
            
            // In a real implementation, this would update the seat availability in the database
            return true;
        }
    }
} 