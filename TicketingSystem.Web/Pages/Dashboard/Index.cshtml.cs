using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;

namespace TicketingSystem.Web.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        public List<PurchasedEvent> PurchasedEvents { get; set; }
        public List<EventViewModel> UpcomingEvents { get; set; }

        public void OnGet()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                Response.Redirect("/Account/Login");
                return;
            }

            // Mock data for purchased events
            PurchasedEvents = new List<PurchasedEvent>
            {
                new PurchasedEvent
                {
                    Id = Guid.NewGuid(),
                    EventName = "Summer Music Festival",
                    Date = DateTime.Now.AddDays(15),
                    Status = "Upcoming",
                    TicketType = "VIP"
                },
                new PurchasedEvent
                {
                    Id = Guid.NewGuid(),
                    EventName = "Tech Conference 2025",
                    Date = DateTime.Now.AddDays(30),
                    Status = "Upcoming",
                    TicketType = "Standard"
                },
                new PurchasedEvent
                {
                    Id = Guid.NewGuid(),
                    EventName = "Comedy Night",
                    Date = DateTime.Now.AddDays(-5),
                    Status = "Past",
                    TicketType = "Premium"
                }
            };

            // Mock data for upcoming events
            UpcomingEvents = new List<EventViewModel>
            {
                new EventViewModel
                {
                    Id = Guid.NewGuid(),
                    Title = "Comedy Night",
                    Date = DateTime.Now.AddDays(10),
                    Location = "City Theater"
                },
                new EventViewModel
                {
                    Id = Guid.NewGuid(),
                    Title = "Jazz Festival",
                    Date = DateTime.Now.AddDays(20),
                    Location = "Central Park"
                },
                new EventViewModel
                {
                    Id = Guid.NewGuid(),
                    Title = "Art Exhibition",
                    Date = DateTime.Now.AddDays(15),
                    Location = "City Gallery"
                }
            };
        }
    }

    public class PurchasedEvent
    {
        public Guid Id { get; set; }
        public string EventName { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public string TicketType { get; set; }
    }

    public class EventViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
    }
} 