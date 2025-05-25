using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Web.Pages.Events
{
    public class IndexModel : PageModel
    {
        private readonly IEventService _eventService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IEventService eventService, ILogger<IndexModel> logger)
        {
            _eventService = eventService;
            _logger = logger;
        }

        public List<EventViewModel> Events { get; set; } = new List<EventViewModel>();
        public string SearchTerm { get; set; }
        public string DebugInfo { get; set; } // Added for debugging

        public async Task OnGetAsync(string searchTerm = null)
        {
            try
            {
                SearchTerm = searchTerm;
                IEnumerable<Event> events = new List<Event>();

                try
                {
                    _logger.LogInformation($"Current DateTime: {DateTime.Now}, UTC: {DateTime.UtcNow}");
                    
                    if (!string.IsNullOrWhiteSpace(SearchTerm))
                    {
                        // If there's a search term, use search functionality
                        events = await _eventService.SearchEventsAsync(SearchTerm);
                    }
                    else
                    {
                        // Otherwise get all upcoming events
                        events = await _eventService.GetUpcomingEventsAsync();
                        _logger.LogInformation($"Retrieved {events.Count()} events from service");
                        foreach (var evt in events)
                        {
                            _logger.LogInformation($"Event: {evt.Id}, Title: {evt.Title}, StartDate: {evt.StartDate}, IsActive: {evt.IsActive}");
                        }
                    }
                    
                    // Store debug info
                    DebugInfo = $"Found {events.Count()} events. Current time: {DateTime.Now.ToString()}";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Database error fetching events: {Message}", ex.Message);
                    // Continue with empty events collection
                    ModelState.AddModelError(string.Empty, "Could not retrieve events from the database. The database schema may need to be updated.");
                    DebugInfo = $"Error: {ex.Message}";
                }

                // Map database events to view models
                Events = events.Select(e => new EventViewModel
                {
                    Id = e.Id,
                    Title = e.Title,
                    Date = e.StartDate,
                    Location = e.Location,
                    Description = e.Description,
                    TicketPrice = e.TicketPrice,
                    Capacity = e.Capacity,
                    ImageUrl = e.ImageUrl,
                    // Default to full capacity if tickets collection is null
                    AvailableSeatCount = e.Capacity, 
                    // Determine status based on availability and dates
                    Status = DetermineEventStatus(e)
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in page processing: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, "An error occurred while processing the page.");
                DebugInfo = $"Fatal error: {ex.Message}";
            }
        }

        private int CalculateAvailableSeats(Event e)
        {
            // If Tickets is null, we assume all seats are available
            if (e.Tickets == null)
                return e.Capacity;
                
            int soldTickets = e.Tickets.Count(t => t.Status != "Cancelled");
            return e.Capacity - soldTickets;
        }

        private string DetermineEventStatus(Event e)
        {
            if (!e.IsActive)
                return "Cancelled";

            // Check if event has already passed
            if (e.StartDate < DateTime.Now)
                return "Completed";
            
            // For events with no ticket data, just show as Available
            if (e.Tickets == null || !e.Tickets.Any())
                return "Available";
                
            int availableSeats = CalculateAvailableSeats(e);
            
            if (availableSeats <= 0)
                return "Sold Out";
            
            if (availableSeats <= 10)
                return "Limited";
            
            return "Available";
        }
    }

    public class EventViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public decimal TicketPrice { get; set; }
        public int Capacity { get; set; }
        public int AvailableSeatCount { get; set; }
        public string Status { get; set; }
        public string ImageUrl { get; set; }
    }
} 