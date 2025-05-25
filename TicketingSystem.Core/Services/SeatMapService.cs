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
        private readonly ITicketRepository _ticketRepository;
        
        public SeatMapService(
            IRepository<SeatMap> seatMapRepository,
            IEventService eventService,
            ITicketRepository ticketRepository)
        {
            _seatMapRepository = seatMapRepository ?? throw new ArgumentNullException(nameof(seatMapRepository));
            _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
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
                        StartRow = 1, 
                        EndRow = 3, 
                        StartColumn = 15, 
                        EndColumn = 19
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
            try
            {
                // Check if there's already a sold ticket for this seat
                var existingTicket = await _ticketRepository.GetTicketBySeatAsync(eventId, row, column);
                
                // Seat is available if no ticket exists or if the existing ticket is cancelled
                return existingTicket == null || existingTicket.Status == "Cancelled";
            }
            catch (Exception)
            {
                // If there's an error, assume seat is unavailable for safety
                return false;
            }
        }
        
        public async Task<IEnumerable<Seat>> GetAvailableSeatsAsync(Guid eventId)
        {
            var seatMap = await GetSeatMapForEventAsync(eventId);
            if (seatMap == null)
            {
                return new List<Seat>();
            }
            
            var availableSeats = new List<Seat>();
            
            // Get all sold tickets for this event to check seat availability
            var soldTickets = await _ticketRepository.GetTicketsByEventAsync(eventId);
            var occupiedSeats = soldTickets
                .Where(t => t.Status == "Sold" && t.SeatRow.HasValue && t.SeatColumn.HasValue)
                .Select(t => new { Row = t.SeatRow.Value, Column = t.SeatColumn.Value })
                .ToHashSet();
            
            // Get event details once to avoid multiple database calls
            var @event = await _eventService.GetEventByIdAsync(eventId);
            decimal basePrice = @event?.TicketPrice ?? 0;
            
            for (int row = 1; row <= seatMap.Rows; row++)
            {
                for (int col = 0; col < seatMap.Columns; col++)
                {
                    // Check if this seat is occupied
                    bool isOccupied = occupiedSeats.Contains(new { Row = row, Column = col });
                    
                    if (!isOccupied)
                    {
                        // Find which section this seat belongs to
                        var section = seatMap.Sections.FirstOrDefault(s => 
                            row >= s.StartRow && row <= s.EndRow && 
                            col >= s.StartColumn && col <= s.EndColumn);
                        
                        decimal price = basePrice;
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
            // Check if seat is available first
            if (!await IsSeatAvailableAsync(eventId, row, column))
            {
                return null;
            }
            
            var seatMap = await GetSeatMapForEventAsync(eventId);
            if (seatMap == null)
            {
                return null;
            }
            
            // Find which section this seat belongs to
            var section = seatMap.Sections.FirstOrDefault(s => 
                row >= s.StartRow && row <= s.EndRow && 
                column >= s.StartColumn && column <= s.EndColumn);
            
            // Get event details once to avoid multiple database calls
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
            // In a real implementation, this would update the seat availability in the database
            // For now, we'll just return true since seat availability is determined by ticket status
            return true;
        }
    }
} 