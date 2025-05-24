using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
<<<<<<< HEAD
using Microsoft.AspNetCore.Http; 
=======
using Microsoft.Extensions.Logging;
>>>>>>> a1f2cea (Fixed issues)
using System;
using System.Linq;
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
        public int AvailableSeats { get; private set; }

        // ✅ Proprietate adăugată pentru a obține ID-ul 
        public Guid? CurrentUserId
        {
            get
            {
                var userIdStr = HttpContext.Session.GetString("UserId");
                return Guid.TryParse(userIdStr, out var userId) ? userId : (Guid?)null;
            }
        }

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

                // Calculate available seats
                AvailableSeats = CalculateAvailableSeats(Event);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching event details for ID: {EventId}", Id);
                ErrorMessage = "An error occurred while trying to retrieve event details.";
                return Page();
            }
        }

        private int CalculateAvailableSeats(Event e)
        {
            int soldTickets = e.Tickets?.Count(t => t.Status != "Cancelled") ?? 0;
            return e.Capacity - soldTickets;
        }
    }
}
