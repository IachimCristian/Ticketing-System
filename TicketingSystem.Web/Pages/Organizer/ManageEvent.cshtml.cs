using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Web.Pages.Organizer
{
    public class ManageEventModel : PageModel
    {
        private readonly IEventService _eventService;
        private readonly ILogger<ManageEventModel> _logger;

        public ManageEventModel(IEventService eventService, ILogger<ManageEventModel> logger)
        {
            _eventService = eventService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public Event Event { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (Id == Guid.Empty)
                return NotFound();

            Event = await _eventService.GetEventByIdAsync(Id);

            if (Event == null)
            {
                ErrorMessage = "Event not found.";
                return Page();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
        if (!ModelState.IsValid)
            {
                foreach (var kvp in ModelState)
                {
                    var key = kvp.Key;
                    var errors = kvp.Value.Errors;

                    foreach (var error in errors)
                    {
                        _logger.LogError("ModelState error for {Key}: {Error}", key, error.ErrorMessage);
                    }
                }

                return Page();
            }



            var existingEvent = await _eventService.GetEventByIdAsync(Event.Id);
            if (existingEvent == null)
            {
                _logger.LogError("Event not found with ID: {Id}", Event.Id);
                ErrorMessage = "Event not found.";
                return Page();
            }

            existingEvent.Title = Event.Title;
            existingEvent.Description = Event.Description;
            existingEvent.Location = Event.Location;
            existingEvent.StartDate = Event.StartDate;
            existingEvent.EndDate = Event.EndDate;
            existingEvent.Capacity = Event.Capacity;
            existingEvent.TicketPrice = Event.TicketPrice;
            existingEvent.ImageUrl = Event.ImageUrl;

            var updated = await _eventService.UpdateEventAsync(existingEvent);

            if (!updated)
            {
                _logger.LogError("Event update failed.");
                ErrorMessage = "Could not update.";
                return Page();
            }

            _logger.LogInformation("Event updated successfully.");
            return RedirectToPage("/Organizer/Dashboard");
        }

    }
}
