using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TicketingSystem.Web.Pages.Dashboard
{
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
                // Get user info from session (consistent with rest of application)
                var userId = HttpContext.Session.GetString("UserId");
                var userType = HttpContext.Session.GetString("UserType");
                var username = HttpContext.Session.GetString("Username");

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userType))
                {
                    _logger.LogWarning("User session is invalid - redirecting to login");
                    return RedirectToPage("/Account/Login");
                }

                Username = username ?? "Guest";
                UserType = userType;
                
                // Redirect organizers to organizer dashboard
                if (UserType == "Organizer")
                {
                    return RedirectToPage("/Organizer/Dashboard");
                }
                
                // Redirect admins to admin dashboard
                if (UserType == "Admin")
                {
                    return RedirectToPage("/Admin/Index");
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
                            TicketNumber = t.TicketNumber ?? "N/A",
                            SeatRow = t.SeatRow,
                            SeatColumn = t.SeatColumn,
                            SeatInfo = GetSeatDisplayText(t.SeatRow, t.SeatColumn)
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
                            TicketPrice = e.TicketPrice,
                            ImageUrl = e.ImageUrl
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
                    ErrorMessage = "Invalid user session. Please log in again.";
                    return RedirectToPage("/Account/Login");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                ErrorMessage = "Error loading dashboard data. Please try refreshing the page.";
            }
            
            return Page();
        }

        private string GetSeatDisplayText(int? seatRow, int? seatColumn)
        {
            if (seatRow.HasValue && seatColumn.HasValue)
            {
                return $"Row {seatRow}, Seat {seatColumn + 1}";
            }
            else if (seatRow.HasValue)
            {
                return $"Row {seatRow}";
            }
            else
            {
                return "General Admission";
            }
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
        public int? SeatRow { get; set; }
        public int? SeatColumn { get; set; }
        public string SeatInfo { get; set; }
    }

    public class EventViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public decimal TicketPrice { get; set; }
        public string ImageUrl { get; set; }
    }
} 