using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TicketingSystem.Web.Pages.Events
{
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        public CreateModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public EventInputModel EventInput { get; set; }

        public class EventInputModel
        {
            [Required]
            public string Title { get; set; }
            public string Description { get; set; }
            [Required]
            [DataType(DataType.DateTime)]
            public DateTime StartDate { get; set; }
            [Required]
            [DataType(DataType.DateTime)]
            public DateTime EndDate { get; set; }
            public string Location { get; set; }
            public int Capacity { get; set; }
            public decimal TicketPrice { get; set; }
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
                ModelState.AddModelError(string.Empty, "Organizer not logged in.");
                return Page();
            }

            var eventDto = new
            {
                title = EventInput.Title,
                description = EventInput.Description,
                startDate = EventInput.StartDate,
                endDate = EventInput.EndDate,
                location = EventInput.Location,
                capacity = EventInput.Capacity,
                ticketPrice = EventInput.TicketPrice,
                imageUrl = EventInput.ImageUrl,
                organizerId = organizerId
            };

            using var client = new HttpClient();
            var response = await client.PostAsJsonAsync("http://localhost:5071/api/Events", eventDto);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToPage("/Events/Index");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Failed to create event: {error}");
                return Page();
            }
        }
    }
} 