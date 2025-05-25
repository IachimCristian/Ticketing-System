using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Core.Services;

namespace TicketingSystem.Web.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class RefundsModel : PageModel
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IPaymentService _paymentService;
        private readonly INotificationSubject _notificationService;
        private readonly ILogger<RefundsModel> _logger;

        public List<RefundViewModel> PendingRefunds { get; set; } = new();
        public List<RefundViewModel> ProcessedRefunds { get; set; } = new();
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public RefundsModel(
            ITicketRepository ticketRepository,
            IPaymentService paymentService,
            INotificationSubject notificationService,
            ILogger<RefundsModel> logger)
        {
            _ticketRepository = ticketRepository;
            _paymentService = paymentService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Get all cancelled tickets that might need refunds
                var tickets = await _ticketRepository.GetTicketsByStatusAsync("Cancelled");
                
                foreach (var ticket in tickets)
                {
                    var refundViewModel = new RefundViewModel
                    {
                        TicketId = ticket.Id,
                        TicketNumber = ticket.TicketNumber,
                        EventTitle = ticket.Event?.Title ?? "Unknown Event",
                        CustomerEmail = ticket.Customer?.Email ?? "Unknown Customer",
                        Amount = ticket.Price,
                        PurchaseDate = ticket.PurchaseDate,
                        CancelDate = ticket.CancelDate ?? DateTime.UtcNow,
                        ProcessDate = ticket.RefundProcessDate,
                        IsApproved = ticket.RefundStatus == "Approved",
                        IsPending = string.IsNullOrEmpty(ticket.RefundStatus)
                    };

                    if (refundViewModel.IsPending)
                    {
                        PendingRefunds.Add(refundViewModel);
                    }
                    else
                    {
                        ProcessedRefunds.Add(refundViewModel);
                    }
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading refunds");
                ErrorMessage = "An error occurred while loading refunds.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostIssueRefundAsync(Guid ticketId)
        {
            try
            {
                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                {
                    ErrorMessage = "Ticket not found.";
                    return RedirectToPage();
                }

                if (ticket.Status != "Cancelled")
                {
                    ErrorMessage = "Only cancelled tickets can be refunded.";
                    return RedirectToPage();
                }

                if (!ticket.PaymentId.HasValue)
                {
                    ErrorMessage = "No payment record found for this ticket.";
                    return RedirectToPage();
                }

                // Process the refund
                var refundId = await _paymentService.ProcessRefundAsync(ticket.PaymentId.Value, ticket.Price);
                
                // Create a new ticket instance with only the fields we need to update
                var ticketToUpdate = new Ticket
                {
                    Id = ticket.Id,
                    TicketNumber = ticket.TicketNumber,
                    EventId = ticket.EventId,
                    CustomerId = ticket.CustomerId,
                    Price = ticket.Price,
                    Status = ticket.Status,
                    CancelDate = ticket.CancelDate,
                    PurchaseDate = ticket.PurchaseDate,
                    PaymentId = ticket.PaymentId,
                    RefundId = refundId,
                    RefundStatus = "Approved",
                    RefundProcessDate = DateTime.UtcNow,
                    QRCode = ticket.QRCode,
                    SeatRow = ticket.SeatRow,
                    SeatColumn = ticket.SeatColumn,
                    SeatId = ticket.SeatId
                };
                
                await _ticketRepository.UpdateAsync(ticketToUpdate);
                await _ticketRepository.SaveChangesAsync();

                // Notify customer
                await _notificationService.NotifyObserversAsync("RefundIssued", new
                {
                    TicketId = ticket.Id,
                    TicketNumber = ticket.TicketNumber,
                    EventTitle = ticket.Event?.Title ?? "Unknown Event",
                    CustomerEmail = ticket.Customer?.Email ?? "Unknown Email",
                    Amount = ticket.Price,
                    RefundId = refundId
                });

                SuccessMessage = $"Refund processed successfully for ticket {ticket.TicketNumber}.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for ticket {TicketId}", ticketId);
                ErrorMessage = "An error occurred while processing the refund.";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostDenyRefundAsync(Guid ticketId)
        {
            try
            {
                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                {
                    ErrorMessage = "Ticket not found.";
                    return RedirectToPage();
                }

                // Create a new ticket instance with only the fields we need to update
                var ticketToUpdate = new Ticket
                {
                    Id = ticket.Id,
                    TicketNumber = ticket.TicketNumber,
                    EventId = ticket.EventId,
                    CustomerId = ticket.CustomerId,
                    Price = ticket.Price,
                    Status = ticket.Status,
                    CancelDate = ticket.CancelDate,
                    PurchaseDate = ticket.PurchaseDate,
                    PaymentId = ticket.PaymentId,
                    RefundId = ticket.RefundId,
                    RefundStatus = "Denied",
                    RefundProcessDate = DateTime.UtcNow,
                    QRCode = ticket.QRCode,
                    SeatRow = ticket.SeatRow,
                    SeatColumn = ticket.SeatColumn,
                    SeatId = ticket.SeatId
                };
                
                await _ticketRepository.UpdateAsync(ticketToUpdate);
                await _ticketRepository.SaveChangesAsync();

                // Notify customer
                await _notificationService.NotifyObserversAsync("RefundDenied", new
                {
                    TicketId = ticket.Id,
                    TicketNumber = ticket.TicketNumber,
                    EventTitle = ticket.Event?.Title ?? "Unknown Event",
                    CustomerEmail = ticket.Customer?.Email ?? "Unknown Email",
                    Amount = ticket.Price
                });

                SuccessMessage = $"Refund denied for ticket {ticket.TicketNumber}.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error denying refund for ticket {TicketId}", ticketId);
                ErrorMessage = "An error occurred while denying the refund.";
                return RedirectToPage();
            }
        }
    }

    public class RefundViewModel
    {
        public Guid TicketId { get; set; }
        public string TicketNumber { get; set; }
        public string EventTitle { get; set; }
        public string CustomerEmail { get; set; }
        public decimal Amount { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime CancelDate { get; set; }
        public DateTime? ProcessDate { get; set; }
        public bool IsApproved { get; set; }
        public bool IsPending { get; set; }
    }
} 