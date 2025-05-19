using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace TicketingSystem.Web.Pages.Events
{
    public class DetailsModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; }

        public void OnGet()
        {
            // For now, use mock data. Replace with real data fetching later.
            EventDate = DateTime.Now.AddDays(10);
            Location = "Main Auditorium, City Center";
        }
    }
} 