using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace TicketingSystem.Web.Pages.Tickets
{
    public class PurchaseModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public Guid EventId { get; set; }

        [BindProperty]
        public int SeatNumber { get; set; } = 1;

        [BindProperty]
        public string PaymentMethod { get; set; } = "CreditCard";

        [BindProperty]
        [CreditCard]
        public string CardNumber { get; set; }

        [BindProperty]
        public string ExpiryDate { get; set; }

        [BindProperty]
        public string Cvv { get; set; }

        public EventViewModel Event { get; set; }
        public string Section { get; set; } = "A";

        public IActionResult OnGet()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login");
            }

            // Mock data for event
            Event = new EventViewModel
            {
                Id = EventId,
                Title = "Summer Music Festival",
                Date = DateTime.Now.AddDays(15),
                Location = "Downtown Arena",
                Price = 79.99m
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Re-populate the Event property for the view
                Event = new EventViewModel
                {
                    Id = EventId,
                    Title = "Summer Music Festival",
                    Date = DateTime.Now.AddDays(15),
                    Location = "Downtown Arena",
                    Price = 79.99m
                };

                return Page();
            }

            // In a real application, we would:
            // 1. Process the payment
            // 2. Create a ticket record in the database
            // 3. Send confirmation email
            
            // Generate a mock ticket ID
            var ticketId = Guid.NewGuid();
            
            // Redirect to the confirmation page with the ticket ID
            return RedirectToPage("/Tickets/Confirmation", new { ticketId });
        }
    }

    public class EventViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public decimal Price { get; set; }
    }
} 