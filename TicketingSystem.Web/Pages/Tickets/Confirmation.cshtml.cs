using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Core.Services;

namespace TicketingSystem.Web.Pages.Tickets
{
    [Authorize]
    public class ConfirmationModel : PageModel
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IEventService _eventService;
        private readonly ILogger<ConfirmationModel> _logger;
        private readonly ITicketPurchaseFacade _ticketPurchaseFacade;
        private readonly IPaymentService _paymentService;

        [BindProperty(SupportsGet = true)]
        public Guid TicketId { get; set; }

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
            ITicketRepository ticketRepository, 
            IEventService eventService, 
            ILogger<ConfirmationModel> logger,
            ITicketPurchaseFacade ticketPurchaseFacade,
            IPaymentService paymentService)
        {
            _ticketRepository = ticketRepository;
            _eventService = eventService;
            _logger = logger;
            _ticketPurchaseFacade = ticketPurchaseFacade;
            _paymentService = paymentService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (TicketId == Guid.Empty)
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
                Ticket = await _ticketRepository.GetByIdAsync(TicketId);

                if (Ticket == null)
                {
                    ErrorMessage = "Ticket not found.";
                    return Page();
                }

                // Check if the ticket belongs to the current user
                if (Ticket.CustomerId.ToString() != userId)
                {
                    ErrorMessage = "You are not authorized to view this ticket.";
                    return Page();
                }

                Event = await _eventService.GetEventByIdAsync(Ticket.EventId);

                if (Event == null)
                {
                    ErrorMessage = "Event not found.";
                    return Page();
                }

                QRCodeUrl = Ticket.QRCode;
                
                // Derive seat information if available
                if (Ticket.SeatId.HasValue)
                {
                    SeatLabel = $"Row {Ticket.SeatRow}, Seat {Ticket.SeatColumn}";
                }

                // Check if ticket can be cancelled (event hasn't started and ticket isn't already cancelled)
                CanBeCancelled = (Event.StartDate > DateTime.Now) && (Ticket.Status == "Sold");
                
                // Calculate refund eligibility based on event policy
                // For example, refund available if event is more than 48 hours away
                TimeSpan timeUntilEvent = Event.StartDate - DateTime.Now;
                CanBeRefunded = CanBeCancelled && timeUntilEvent.TotalHours > 48;
                
                // Calculate refund amount (example: full refund if eligible)
                RefundAmount = CanBeRefunded ? Ticket.Price : 0;
                
                // Check if this is a cancelled ticket with a refund already issued
                RefundIssued = (Ticket.Status == "Cancelled") && Ticket.RefundId.HasValue;
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
            if (TicketId == Guid.Empty)
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
                // Get the ticket
                Ticket = await _ticketRepository.GetByIdAsync(TicketId);

                if (Ticket == null)
                {
                    ErrorMessage = "Ticket not found.";
                    return Page();
                }

                // Check if the ticket belongs to the current user
                if (Ticket.CustomerId.ToString() != userId)
                {
                    ErrorMessage = "You are not authorized to cancel this ticket.";
                    return Page();
                }

                // Get the event
                Event = await _eventService.GetEventByIdAsync(Ticket.EventId);

                if (Event == null)
                {
                    ErrorMessage = "Event not found.";
                    return Page();
                }

                // Cancel the ticket
                await _ticketPurchaseFacade.CancelTicketAsync(TicketId, Guid.Parse(userId));
                
                // Process refund if eligible
                TimeSpan timeUntilEvent = Event.StartDate - DateTime.Now;
                bool isRefundEligible = (timeUntilEvent.TotalHours > 48);
                
                if (isRefundEligible)
                {
                    // Process refund
                    Guid refundId = await _paymentService.ProcessRefundAsync(Ticket.PaymentId, Ticket.Price);
                    
                    // Update ticket with refund information
                    Ticket.RefundId = refundId;
                    await _ticketRepository.UpdateAsync(Ticket);
                    
                    RefundIssued = true;
                    SuccessMessage = "Your ticket has been cancelled and a refund has been issued.";
                }
                else
                {
                    SuccessMessage = "Your ticket has been cancelled. No refund was issued due to the event's refund policy.";
                }
                
                // Refresh ticket data
                Ticket = await _ticketRepository.GetByIdAsync(TicketId);
                
                // Check if ticket can be cancelled (event hasn't started and ticket isn't already cancelled)
                CanBeCancelled = (Event.StartDate > DateTime.Now) && (Ticket.Status == "Sold");
                
                // Calculate refund eligibility based on event policy
                CanBeRefunded = CanBeCancelled && timeUntilEvent.TotalHours > 48;
                
                // Calculate refund amount (example: full refund if eligible)
                RefundAmount = CanBeRefunded ? Ticket.Price : 0;

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling ticket");
                ErrorMessage = "An error occurred while cancelling the ticket.";
                return Page();
            }
        }
    }
} 