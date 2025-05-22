using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Core.Entities;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace TicketingSystem.Web.Pages.Tickets
{
    public class ConfirmationModel : PageModel
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IEventService _eventService;
        private readonly ILogger<ConfirmationModel> _logger;

        public ConfirmationModel(
            ITicketRepository ticketRepository,
            IEventService eventService,
            ILogger<ConfirmationModel> logger)
        {
            _ticketRepository = ticketRepository;
            _eventService = eventService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public Guid TicketId { get; set; }

        public Ticket Ticket { get; set; }
        public Event Event { get; set; }
        public string ErrorMessage { get; set; }
        public string QRCodeUrl { get; set; }
        public string SeatLabel { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is logged in
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid userId))
            {
                return RedirectToPage("/Account/Login");
            }

            try
            {
                // Load the ticket details
                Ticket = await _ticketRepository.GetByIdAsync(TicketId);
                
                if (Ticket == null)
                {
                    ErrorMessage = "Ticket not found.";
                    return Page();
                }

                // Verify the ticket belongs to the current user
                if (Ticket.CustomerId != userId)
                {
                    ErrorMessage = "You are not authorized to view this ticket.";
                    return Page();
                }

                // Load the event details
                Event = await _eventService.GetEventByIdAsync(Ticket.EventId);
                if (Event == null)
                {
                    ErrorMessage = "Event information not found.";
                    return Page();
                }

                // Generate QR code URL (in a real app this would be more secure)
                QRCodeUrl = $"/api/qrcode/{Ticket.QRCode}";

                // For the purpose of this demo, we'll assume seat information may be encoded in the QR code
                // In a real application, you would have a more structured way to store and retrieve seat information
                // This is a simple placeholder implementation
                if (!string.IsNullOrEmpty(Ticket.QRCode))
                {
                    var parts = Ticket.QRCode.Split('-');
                    if (parts.Length >= 3 && parts[0] == "SEAT")
                    {
                        if (int.TryParse(parts[1], out int row) && int.TryParse(parts[2], out int col))
                        {
                            SeatLabel = $"{(char)('A' + col)}{row}";
                        }
                    }
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading ticket details for confirmation, TicketId: {TicketId}", TicketId);
                ErrorMessage = "An error occurred while loading the ticket details.";
                return Page();
            }
        }
    }
} 