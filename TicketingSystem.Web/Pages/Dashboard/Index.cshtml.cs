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
using Microsoft.Extensions.Logging;

namespace TicketingSystem.Web.Pages.Dashboard
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IEventService _eventService;
        private readonly ICustomerNotificationService _notificationService;
        private readonly ILogger<IndexModel> _logger;
        
        public List<PurchasedEvent> PurchasedEvents { get; set; } = new List<PurchasedEvent>();
        public List<EventViewModel> UpcomingEvents { get; set; } = new List<EventViewModel>();
        public List<CustomerNotification> RecentNotifications { get; set; } = new List<CustomerNotification>();
        public int UnreadNotificationCount { get; set; }
        
        public string Username { get; set; }
        public string UserType { get; set; }
        public string ErrorMessage { get; set; }

        public IndexModel(
            ITicketRepository ticketRepository, 
            IEventService eventService,
            ICustomerNotificationService notificationService,
            ILogger<IndexModel> logger)
        {
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Get user info from claims
                var userIdentity = User?.Identity;
                if (userIdentity == null || !userIdentity.IsAuthenticated)
                {
                    _logger.LogWarning("User identity is null or not authenticated");
                    return RedirectToPage("/Account/Login");
                }

                Username = userIdentity.Name ?? "Guest";
                UserType = User.FindFirst("UserType")?.Value;
                
                // Redirect organizers to organizer dashboard
                if (UserType == "Organizer")
                {
                    return RedirectToPage("/Organizer/Dashboard");
                }
                
                // Get user's purchased tickets and upcoming events
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    throw new InvalidOperationException("User ID not found in claims");
                }

                if (Guid.TryParse(userId, out Guid customerId))
                {
                    var tickets = await _ticketRepository.GetTicketsByCustomerAsync(customerId);
                    
                    if (tickets != null)
                    {
                        PurchasedEvents = tickets.Select(t => new PurchasedEvent
                        {
                            Id = t.Id,
                            EventId = t.EventId,
                            EventName = t.Event?.Title ?? "Unknown Event",
                            Date = t.Event?.StartDate ?? DateTime.Now,
                            Status = t.Event?.StartDate > DateTime.Now ? "Upcoming" : "Past",
                            TicketType = "Standard",
                            TicketNumber = t.TicketNumber ?? "N/A"
                        }).ToList();
                    }
                    
                    // Get upcoming events
                    var upcomingEvents = await _eventService.GetUpcomingEventsAsync();
                    if (upcomingEvents != null)
                    {
                        UpcomingEvents = upcomingEvents.Select(e => new EventViewModel
                        {
                            Id = e.Id,
                            Title = e.Title ?? "Untitled Event",
                            Date = e.StartDate,
                            Location = e.Location ?? "TBD",
                            TicketPrice = e.TicketPrice
                        }).ToList();
                    }

                    // Get notification data for customers
                    if (UserType == "Customer")
                    {
                        UnreadNotificationCount = await _notificationService.GetUnreadCountAsync(customerId);
                        var recentNotifications = await _notificationService.GetRecentNotificationsAsync(customerId, 5);
                        RecentNotifications = recentNotifications.ToList();
                    }
                }
                else
                {
                    _logger.LogError("Failed to parse user ID: {UserId}", userId);
                    throw new InvalidOperationException("Invalid user ID format");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                ErrorMessage = "Error loading dashboard data. Please try refreshing the page.";
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