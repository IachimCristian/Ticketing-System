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
        
        // Filter properties
        public string SearchTerm { get; set; }
        public string LocationFilter { get; set; }
        public string PriceRange { get; set; }
        public string DateFilter { get; set; }
        public string SortBy { get; set; } = "date-asc"; // Default sort
        
        // Available filter options
        public List<string> AvailableLocations { get; set; } = new List<string>();
        
        // Helper property to check if any filters are active
        public bool HasActiveFilters => !string.IsNullOrEmpty(SearchTerm) || 
                                       !string.IsNullOrEmpty(LocationFilter) || 
                                       !string.IsNullOrEmpty(PriceRange) || 
                                       !string.IsNullOrEmpty(DateFilter);

        public async Task OnGetAsync(string searchTerm = null, string location = null, 
                                   string priceRange = null, string dateFilter = null, 
                                   string sortBy = "date-asc")
        {
            try
            {
                // Set filter values
                SearchTerm = searchTerm;
                LocationFilter = location;
                PriceRange = priceRange;
                DateFilter = dateFilter;
                SortBy = sortBy;
                
                // Get all events first
                IEnumerable<Event> events;
                
                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    events = await _eventService.SearchEventsAsync(SearchTerm);
                }
                else
                {
                    events = await _eventService.GetUpcomingEventsAsync();
                }
                
                // Get unique locations for filter dropdown
                AvailableLocations = events.Select(e => e.Location)
                                          .Distinct()
                                          .OrderBy(l => l)
                                          .ToList();
                
                // Apply filters
                var filteredEvents = ApplyFilters(events);
                
                // Map to view models
                var eventViewModels = filteredEvents.Select(e => new EventViewModel
                {
                    Id = e.Id,
                    Title = e.Title,
                    Date = e.StartDate,
                    Location = e.Location,
                    Description = e.Description,
                    TicketPrice = e.TicketPrice,
                    Capacity = e.Capacity,
                    ImageUrl = e.ImageUrl,
                    AvailableSeatCount = CalculateAvailableSeats(e),
                    Status = DetermineEventStatus(e)
                }).ToList();
                
                // Apply sorting
                Events = ApplySorting(eventViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading events");
                ModelState.AddModelError(string.Empty, "An error occurred while loading events.");
            }
        }
        
        private IEnumerable<Event> ApplyFilters(IEnumerable<Event> events)
        {
            // Location filter
            if (!string.IsNullOrEmpty(LocationFilter))
            {
                events = events.Where(e => e.Location.Equals(LocationFilter, StringComparison.OrdinalIgnoreCase));
            }
            
            // Price range filter
            if (!string.IsNullOrEmpty(PriceRange))
            {
                events = PriceRange switch
                {
                    "free" => events.Where(e => e.TicketPrice == 0),
                    "0-25" => events.Where(e => e.TicketPrice > 0 && e.TicketPrice <= 25),
                    "25-50" => events.Where(e => e.TicketPrice > 25 && e.TicketPrice <= 50),
                    "50-100" => events.Where(e => e.TicketPrice > 50 && e.TicketPrice <= 100),
                    "100+" => events.Where(e => e.TicketPrice > 100),
                    _ => events
                };
            }
            
            // Date filter
            if (!string.IsNullOrEmpty(DateFilter))
            {
                var now = DateTime.Now.Date;
                events = DateFilter switch
                {
                    "today" => events.Where(e => e.StartDate.Date == now),
                    "tomorrow" => events.Where(e => e.StartDate.Date == now.AddDays(1)),
                    "this-week" => events.Where(e => e.StartDate.Date >= now && e.StartDate.Date <= now.AddDays(7)),
                    "this-month" => events.Where(e => e.StartDate.Month == now.Month && e.StartDate.Year == now.Year),
                    "next-month" => events.Where(e => e.StartDate.Month == now.AddMonths(1).Month && e.StartDate.Year == now.AddMonths(1).Year),
                    _ => events
                };
            }
            
            return events;
        }
        
        private List<EventViewModel> ApplySorting(List<EventViewModel> events)
        {
            return SortBy switch
            {
                "date-desc" => events.OrderByDescending(e => e.Date).ToList(),
                "price-asc" => events.OrderBy(e => e.TicketPrice).ToList(),
                "price-desc" => events.OrderByDescending(e => e.TicketPrice).ToList(),
                "name-asc" => events.OrderBy(e => e.Title).ToList(),
                "name-desc" => events.OrderByDescending(e => e.Title).ToList(),
                _ => events.OrderBy(e => e.Date).ToList() // Default: date-asc
            };
        }

        private int CalculateAvailableSeats(Event e)
        {
            if (e.Tickets == null)
                return e.Capacity;
                
            int soldTickets = e.Tickets.Count(t => t.Status == "Sold");
            return e.Capacity - soldTickets;
        }

        private string DetermineEventStatus(Event e)
        {
            if (!e.IsActive)
                return "Cancelled";

            if (e.StartDate < DateTime.Now)
                return "Completed";
            
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