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

        public async Task OnGetAsync(string searchTerm = null)
        {
            try
            {
                SearchTerm = searchTerm;
                IEnumerable<Event> events = new List<Event>();

                try
                {
                    if (!string.IsNullOrWhiteSpace(SearchTerm))
                    {
                        // If there's a search term, use search functionality
                        events = await _eventService.SearchEventsAsync(SearchTerm);
                    }
                    else
                    {
                        // Otherwise get all upcoming events
                        events = await _eventService.GetUpcomingEventsAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Database error fetching events: {Message}", ex.Message);
                    // Continue with empty events collection
                    ModelState.AddModelError(string.Empty, "Could not retrieve events from the database. The database schema may need to be updated.");
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
                    AvailableSeatCount = CalculateAvailableSeats(e),
                    // Determine status based on ticket availability
                    Status = DetermineEventStatus(e)
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in page processing: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, "An error occurred while processing the page.");
            }
        }

        private int CalculateAvailableSeats(Event e)
        {
            int soldTickets = e.Tickets?.Count(t => t.Status != "Cancelled") ?? 0;
            return e.Capacity - soldTickets;
        }

        private string DetermineEventStatus(Event e)
        {
            if (!e.IsActive)
                return "Cancelled";

            // Check if event has already passed
            if (e.StartDate < DateTime.Now)
                return "Completed";
            
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
    }
} 