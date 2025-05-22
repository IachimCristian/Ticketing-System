using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        public IndexModel(IEventService eventService)
        {
            _eventService = eventService;
        }

        public List<EventViewModel> Events { get; set; } = new List<EventViewModel>();
        public string SearchTerm { get; set; }

        public async Task OnGetAsync(string searchTerm = null)
        {
            SearchTerm = searchTerm;
            IEnumerable<Event> events;

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

            // Map database events to view models
            Events = events.Select(e => new EventViewModel
            {
                Id = e.Id,
                Title = e.Title,
                Date = e.StartDate,
                Location = e.Location,
                Description = e.Description,
                TicketPrice = e.TicketPrice,
                ImageUrl = e.ImageUrl,
                // Determine status based on ticket availability and capacity
                Status = DetermineEventStatus(e)
            }).ToList();
        }

        private string DetermineEventStatus(Event e)
        {
            if (!e.IsActive)
                return "Cancelled";

            // In a real app, you'd check ticket counts against capacity
            // For now, we'll use a simplified approach based on date
            if (e.StartDate < DateTime.Now)
                return "Completed";
            
            if (e.Capacity <= 10)
                return "Limited";
            
            if (e.Capacity <= 0)
                return "Sold Out";
            
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
        public string ImageUrl { get; set; }
        public decimal TicketPrice { get; set; }
        public string Status { get; set; }
    }
} 