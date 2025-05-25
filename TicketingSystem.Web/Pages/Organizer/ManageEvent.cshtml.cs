using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace TicketingSystem.Web.Pages.Organizer
{
    public class ManageEventModel : PageModel
    {
        private readonly IEventService _eventService;
        private readonly ILogger<ManageEventModel> _logger;
        private readonly IWebHostEnvironment _environment;

        public ManageEventModel(
            IEventService eventService,
            ILogger<ManageEventModel> logger,
            IWebHostEnvironment environment)
        {
            _eventService = eventService;
            _logger = logger;
            _environment = environment;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public Event Event { get; set; }

        [BindProperty]
        public IFormFile EventImage { get; set; }

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
            // Remove validation for ImageUrl and Organizer since we'll preserve them
            ModelState.Remove("Event.ImageUrl");
            ModelState.Remove("Event.Organizer");
            ModelState.Remove("Event.OrganizerId");

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

            // Process uploaded image if any
            if (EventImage != null && EventImage.Length > 0)
            {
                try
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "events");
                    Directory.CreateDirectory(uploadsFolder); // Ensure folder exists
                    
                    var uniqueFileName = $"{Guid.NewGuid()}_{EventImage.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await EventImage.CopyToAsync(fileStream);
                    }
                    
                    // Update the image URL
                    existingEvent.ImageUrl = $"/images/events/{uniqueFileName}";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading event image");
                    ErrorMessage = "Failed to upload image, but event details will be saved.";
                }
            }

            // Update only the fields from the form while preserving other required fields
            existingEvent.Title = Event.Title;
            existingEvent.Description = Event.Description;
            existingEvent.Location = Event.Location;
            existingEvent.StartDate = Event.StartDate;
            existingEvent.EndDate = Event.EndDate;
            existingEvent.Capacity = Event.Capacity;
            existingEvent.TicketPrice = Event.TicketPrice;
            // ImageUrl is handled above and Organizer is preserved from the existing event

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
