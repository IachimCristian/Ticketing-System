using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Core.Entities;

namespace TicketingSystem.Web.Pages
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

        public List<FeaturedEventViewModel> FeaturedEvents { get; set; } = new List<FeaturedEventViewModel>();

        public async Task OnGetAsync()
        {
            try
            {
                // Get upcoming events sorted by start date
                var upcomingEvents = await _eventService.GetUpcomingEventsAsync();
                
                // Take the first 3 active events for the featured section
                FeaturedEvents = upcomingEvents
                    .Where(e => e.IsActive)
                    .OrderBy(e => e.StartDate)
                    .Take(3)
                    .Select(e => new FeaturedEventViewModel
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Date = e.StartDate.ToString("MMMM d, yyyy"),
                        ImageUrl = e.ImageUrl
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading featured events");
            }
        }
    }

    public class FeaturedEventViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Date { get; set; }
        public string ImageUrl { get; set; }
    }
}
