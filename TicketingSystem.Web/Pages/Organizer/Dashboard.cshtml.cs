using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Core.Entities;

namespace TicketingSystem.Web.Pages.Organizer
{
    public class DashboardModel : PageModel
    {
        private readonly IEventRepository _eventRepository;
        
        public DashboardModel(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
        }

        public string Username { get; set; }
        public string ErrorMessage { get; set; }
        public List<Event> OrganizerEvents { get; set; } = new List<Event>();
        
        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is logged in and is an organizer
            var userId = HttpContext.Session.GetString("UserId");
            Username = HttpContext.Session.GetString("Username");
            var userType = HttpContext.Session.GetString("UserType");
            
            if (string.IsNullOrEmpty(userId) || userType != "Organizer")
            {
                return RedirectToPage("/Account/Login");
            }
            
            try
            {
                // Get events for this organizer
                if (Guid.TryParse(userId, out Guid organizerId))
                {
                    var events = await _eventRepository.GetEventsByOrganizerAsync(organizerId);
                    OrganizerEvents = events.ToList();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading organizer data: {ex.Message}";
            }
            
            return Page();
        }
    }
} 