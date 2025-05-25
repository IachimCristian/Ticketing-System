using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TicketingSystem.Web.Pages.Dashboard
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IEventService _eventService;
        
        public List<PurchasedEvent> PurchasedEvents { get; set; } = new List<PurchasedEvent>();
        public List<EventViewModel> UpcomingEvents { get; set; } = new List<EventViewModel>();
        
        public string Username { get; set; }
        public string UserType { get; set; }
        public string ErrorMessage { get; set; }

        public IndexModel(ITicketRepository ticketRepository, IEventService eventService)
        {
            _ticketRepository = ticketRepository;
            _eventService = eventService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get user info from claims
            Username = User.Identity.Name;
            UserType = User.FindFirst("UserType")?.Value;
            
            // Redirect organizers to organizer dashboard
            if (UserType == "Organizer")
            {
                return RedirectToPage("/Organizer/Dashboard");
            }
            
            try
            {
                // Get user's purchased tickets and upcoming events
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userId, out Guid customerId))
                {
                    var tickets = await _ticketRepository.GetTicketsByCustomerAsync(customerId);
                    
                    PurchasedEvents = tickets.Select(t => new PurchasedEvent
                    {
                        Id = t.Id,
                        EventId = t.EventId,
                        EventName = t.Event?.Title ?? "Unknown Event",
                        Date = t.Event?.StartDate ?? DateTime.Now,
                        Status = t.Event?.StartDate > DateTime.Now ? "Upcoming" : "Past",
                        TicketType = "Standard",
                        TicketNumber = t.TicketNumber
                    }).ToList();
                }
                
                // Get real upcoming events
                var upcomingEvents = await _eventService.GetUpcomingEventsAsync();
                UpcomingEvents = upcomingEvents.Select(e => new EventViewModel
                {
                    Id = e.Id,
                    Title = e.Title,
                    Date = e.StartDate,
                    Location = e.Location,
                    TicketPrice = e.TicketPrice
                }).ToList();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading dashboard data: {ex.Message}";
            }
            
            return Page();
        }
    }

    public class PurchasedEvent
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public string EventName { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public string TicketType { get; set; }
        public string TicketNumber { get; set; }
    }

    public class EventViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public decimal TicketPrice { get; set; }
    }
} 