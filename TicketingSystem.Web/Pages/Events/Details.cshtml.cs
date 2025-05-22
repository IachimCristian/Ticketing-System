using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Web.Pages.Events
{
    public class DetailsModel : PageModel
    {
        private readonly IEventService _eventService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(IEventService eventService, ILogger<DetailsModel> logger)
        {
            _eventService = eventService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }
        
        public Event Event { get; set; }
        public bool EventNotFound { get; set; }
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (Id == Guid.Empty)
            {
                return NotFound();
            }

            try
            {
                Event = await _eventService.GetEventByIdAsync(Id);
                
                if (Event == null)
                {
                    EventNotFound = true;
                    return Page();
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching event details for ID: {EventId}", Id);
                ErrorMessage = "An error occurred while trying to retrieve event details.";
                return Page();
            }
        }
    }
} 