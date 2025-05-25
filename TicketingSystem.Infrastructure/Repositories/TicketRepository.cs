using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Core.Services;
using TicketingSystem.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace TicketingSystem.Infrastructure.Repositories
{
    public class TicketRepository : Repository<Ticket>, ITicketRepository
    {
        private readonly ILogger<TicketRepository> _logger;

        public TicketRepository(AppDbContext context, ILogger<TicketRepository> logger) : base(context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByCustomerAsync(Guid customerId)
        {
            try
            {
                var tickets = await _dbSet
                    .Include(t => t.Event)
                    .Include(t => t.Customer)
                    .Include(t => t.Payment)
                    .Where(t => t.CustomerId == customerId)
                    .OrderByDescending(t => t.PurchaseDate)
                    .AsNoTracking()
                    .ToListAsync();

                if (tickets == null)
                {
                    _logger.LogWarning("No tickets found for customer {CustomerId}", customerId);
                    return new List<Ticket>();
                }

                // Ensure Event is loaded for each ticket
                foreach (var ticket in tickets.Where(t => t.Event == null && t.EventId != Guid.Empty))
                {
                    ticket.Event = await _context.Set<Event>()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == ticket.EventId);
                }

                return tickets;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tickets for customer {CustomerId}", customerId);
                return new List<Ticket>();
            }
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByEventAsync(Guid eventId)
        {
            try
            {
                return await _dbSet
                    .Include(t => t.Customer)
                    .Include(t => t.Payment)
                    .Where(t => t.EventId == eventId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tickets for event {EventId}", eventId);
                return new List<Ticket>();
            }
        }

        public async Task<Ticket> GetTicketByQRCodeAsync(string qrCode)
        {
            try
            {
                return await _dbSet
                    .Include(t => t.Event)
                    .Include(t => t.Customer)
                    .Include(t => t.Payment)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.QRCode == qrCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticket by QR code {QRCode}", qrCode);
                return null;
            }
        }

        public async Task<bool> ValidateTicketAsync(string qrCode)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating ticket with QR code {QRCode}", qrCode);
                return false;
            }
        }

        public override async Task<Ticket> GetByIdAsync(Guid id)
        {
            try
            {
                var ticket = await _dbSet
                    .Include(t => t.Event)
                    .Include(t => t.Customer)
                    .Include(t => t.Payment)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (ticket == null)
                {
                    _logger.LogWarning("No ticket found with ID {TicketId}", id);
                    return null;
                }

                ticket.Status ??= "Unknown";
                ticket.TicketNumber ??= "N/A";
                
                if (ticket.Event == null && ticket.EventId != Guid.Empty)
                {
                    _logger.LogWarning("Event not loaded for ticket {TicketId}, attempting to load", id);
                    ticket.Event = await _context.Set<Event>()
                        .FirstOrDefaultAsync(e => e.Id == ticket.EventId);
                }

                return ticket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticket {TicketId}", id);
                return null;
            }
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByStatusAsync(string status)
        {
            try
            {
                return await _dbSet
                    .Include(t => t.Event)
                    .Include(t => t.Customer)
                    .Include(t => t.Payment)
                    .Where(t => t.Status == status)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tickets with status {Status}", status);
                return new List<Ticket>();
            }
        }

        public async Task<bool> IsTicketAvailableAsync(Guid eventId)
        {
            try
            {
                var availableTickets = await _dbSet
                    .AsNoTracking()
                    .CountAsync(t => t.EventId == eventId && t.Status == "Available");
                return availableTickets > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking ticket availability for event {EventId}", eventId);
                return false;
            }
        }

        public async Task<Ticket> GetTicketByNumberAsync(string ticketNumber)
        {
            try
            {
                return await _dbSet
                    .Include(t => t.Event)
                    .Include(t => t.Customer)
                    .Include(t => t.Payment)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticket by number {TicketNumber}", ticketNumber);
                return null;
            }
        }

        public async Task<Ticket> GetTicketBySeatAsync(Guid eventId, int seatRow, int seatColumn)
        {
            try
            {
                return await _dbSet
                    .Include(t => t.Event)
                    .Include(t => t.Customer)
                    .Include(t => t.Payment)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.EventId == eventId && 
                                            t.SeatRow == seatRow && 
                                            t.SeatColumn == seatColumn &&
                                            t.Status == "Sold");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticket by seat for event {EventId}, row {SeatRow}, column {SeatColumn}", 
                    eventId, seatRow, seatColumn);
                return null;
            }
        }
    }
} 