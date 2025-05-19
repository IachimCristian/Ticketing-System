using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace TicketingSystem.Web.Pages.Tickets
{
    public class ConfirmationModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public Guid TicketId { get; set; }

        public void OnGet()
        {
            // In a real application, load the ticket details from a database
            // using the TicketId parameter
        }
    }
} 