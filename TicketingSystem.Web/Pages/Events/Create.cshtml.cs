using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Core.Services;

namespace TicketingSystem.Web.Pages.Events
{
    public class CreateModel : PageModel
    {
        private readonly IEventService _eventService;

        public CreateModel(IEventService eventService)
        {
            _eventService = eventService;
        }

        [BindProperty]
        public EventInputModel EventInput { get; set; } = new EventInputModel();

        public class EventInputModel
        {
            [Required]
            public string Title { get; set; }
            
            public string Description { get; set; }
            
            [Required]
            [DataType(DataType.DateTime)]
            public DateTime StartDate { get; set; } = DateTime.Now.AddDays(1);
            
            [Required]
            [DataType(DataType.DateTime)]
            public DateTime EndDate { get; set; } = DateTime.Now.AddDays(1).AddHours(2);
            
            public string Location { get; set; }
            
            [Range(1, 10000)]
            public int Capacity { get; set; } = 100;
            
            [Range(0, 10000)]
            public decimal TicketPrice { get; set; } = 0;
            
            public string ImageUrl { get; set; }
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var organizerId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(organizerId))
            {
                ModelState.AddModelError(string.Empty, "You must be logged in as an organizer to create events.");
                return Page();
            }

            try
            {
                // Use the EventBuilder from Core to create the event (Builder pattern)
                var newEvent = new EventBuilder()
                    .WithTitle(EventInput.Title)
                    .WithDescription(EventInput.Description)
                    .WithDates(EventInput.StartDate, EventInput.EndDate)
                    .AtLocation(EventInput.Location)
                    .WithCapacity(EventInput.Capacity)
                    .WithTicketPrice(EventInput.TicketPrice)
                    .WithImage(EventInput.ImageUrl)
                    .ByOrganizer(Guid.Parse(organizerId))
                    .IsActive(true)
                    .Build();

                // Use the service to create the event
                var createdEvent = await _eventService.CreateEventAsync(newEvent);

                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToPage("/Events/Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to create event: {ex.Message}");
                return Page();
            }
        }
    }
} 