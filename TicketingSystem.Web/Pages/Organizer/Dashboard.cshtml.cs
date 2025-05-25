using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TicketingSystem.Web.Pages.Organizer
{
    [Authorize(Policy = "OrganizerOnly")]
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
            // Get user info from claims
            Username = User.Identity.Name;
            
            try
            {
                // Get events for this organizer
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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