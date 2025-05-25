using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Services;

namespace TicketingSystem.Web.Pages.Tickets
{
    [Authorize]
    public class ConfirmationModel : PageModel
    {
        private readonly TicketViewService _ticketViewService;
        private readonly TicketCancellationService _ticketCancellationService;
        private readonly ILogger<ConfirmationModel> _logger;

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        public Ticket Ticket { get; set; }
        public Event Event { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
        public string QRCodeUrl { get; set; }
        public string SeatLabel { get; set; }
        public bool CanBeCancelled { get; set; }
        public bool CanBeRefunded { get; set; }
        public bool RefundIssued { get; set; }
        public decimal RefundAmount { get; set; }

        public ConfirmationModel(
            TicketViewService ticketViewService,
            TicketCancellationService ticketCancellationService,
            ILogger<ConfirmationModel> logger)
        {
            _ticketViewService = ticketViewService;
            _ticketCancellationService = ticketCancellationService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (Id == Guid.Empty)
            {
                ErrorMessage = "Invalid ticket ID.";
                return Page();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                ErrorMessage = "Authentication error. Please sign in again.";
                return RedirectToPage("/Account/Login");
            }

            try
            {
                // Use the view service to get ticket details
                Ticket = await _ticketViewService.GetTicketDetailsAsync(Id, Guid.Parse(userId));
                Event = Ticket.Event;
                QRCodeUrl = Ticket.QRCode;

                // Derive seat information if available
                if (Ticket.SeatId.HasValue)
                {
                    SeatLabel = $"Row {Ticket.SeatRow}, Seat {Ticket.SeatColumn}";
                }

                // Check if ticket can be cancelled (event hasn't started and ticket isn't already cancelled)
                CanBeCancelled = (Event.StartDate > DateTime.Now) && (Ticket.Status == "Sold");
                
                // Calculate refund eligibility based on event policy
                TimeSpan timeUntilEvent = Event.StartDate - DateTime.Now;
                CanBeRefunded = CanBeCancelled && timeUntilEvent.TotalHours > 48;
                
                // Calculate refund amount (example: full refund if eligible)
                RefundAmount = CanBeRefunded ? Ticket.Price : 0;
                
                // Check if this is a cancelled ticket with a refund already issued
                RefundIssued = (Ticket.Status == "Cancelled") && Ticket.RefundId.HasValue;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Access denied or invalid ticket");
                ErrorMessage = ex.Message;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticket information");
                ErrorMessage = "An error occurred while retrieving ticket information.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCancelTicketAsync()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToPage("/Account/Login");
                }

                // Use the cancellation service to cancel the ticket
                await _ticketCancellationService.CancelTicketAsync(Id, Guid.Parse(userId));
                
                TempData["SuccessMessage"] = "Your ticket has been cancelled. If eligible, a refund will be processed.";
                return RedirectToPage("/Dashboard/Index");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to cancel ticket");
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage("/Dashboard/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling ticket");
                TempData["ErrorMessage"] = "An error occurred while cancelling the ticket.";
                return RedirectToPage("/Dashboard/Index");
            }
        }
    }
} 