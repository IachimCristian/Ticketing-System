using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;

namespace TicketingSystem.Web.Pages.Events
{
    public class IndexModel : PageModel
    {
        public List<EventViewModel> Events { get; set; }

        public void OnGet()
        {
            Events = new List<EventViewModel>
            {
                new EventViewModel
                {
                    Id = Guid.NewGuid(),
                    Title = "Music Festival",
                    Date = new DateTime(2023, 6, 15),
                    Location = "Downtown Arena",
                    Description = "Join us for a night of amazing music with top artists around the world!",
                    Status = "Available"
                },
                new EventViewModel
                {
                    Id = Guid.NewGuid(),
                    Title = "Tech Conference",
                    Date = new DateTime(2023, 7, 10),
                    Location = "Convention Center",
                    Description = "Learn about the latest technologies and network with industry experts.",
                    Status = "Limited"
                },
                new EventViewModel
                {
                    Id = Guid.NewGuid(),
                    Title = "Food Festival",
                    Date = new DateTime(2023, 8, 5),
                    Location = "City Park",
                    Description = "Sample delicious food from various cuisines and enjoy live entertainment.",
                    Status = "Sold Out"
                }
            };
        }
    }

    public class EventViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }
} 