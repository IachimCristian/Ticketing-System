using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Core.Services;

namespace TicketingSystem.Web.Pages.Tickets
{
    public class PurchaseModel : PageModel
    {
        private readonly IEventService _eventService;
        private readonly ISeatMapService _seatMapService;
        private readonly TicketPurchaseService _ticketPurchaseService;
        private readonly ILogger<PurchaseModel> _logger;

        [BindProperty(SupportsGet = true)]
        public Guid EventId { get; set; }

        [BindProperty]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a seat.")]
        public int SelectedRow { get; set; }

        [BindProperty]
        [Range(0, int.MaxValue, ErrorMessage = "Please select a seat.")]
        public int SelectedColumn { get; set; }

        [BindProperty]
        public string PaymentMethod { get; set; } = "creditcard";

        [BindProperty]
        [CreditCard(ErrorMessage = "The CardNumber field is not a valid credit card number.")]
        [RequiredIf("PaymentMethod", "creditcard", ErrorMessage = "Card Number is required when using Credit Card payment.")]
        public string CardNumber { get; set; }

        [BindProperty]
        [RequiredIf("PaymentMethod", "creditcard", ErrorMessage = "Expiry Date is required when using Credit Card payment.")]
        public string ExpiryDate { get; set; }

        [BindProperty]
        [RequiredIf("PaymentMethod", "creditcard", ErrorMessage = "CVV is required when using Credit Card payment.")]
        public string Cvv { get; set; }

        public Event Event { get; set; }
        public List<List<SeatViewModel>> SeatLayout { get; set; } = new List<List<SeatViewModel>>();
        public List<SeatSectionViewModel> SeatSections { get; set; } = new List<SeatSectionViewModel>();
        public string ErrorMessage { get; set; }

        public PurchaseModel(
            IEventService eventService,
            ISeatMapService seatMapService,
            TicketPurchaseService ticketPurchaseService,
            ILogger<PurchaseModel> logger)
        {
            _eventService = eventService;
            _seatMapService = seatMapService;
            _ticketPurchaseService = ticketPurchaseService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is logged in
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToPage("/Account/Login", new { returnUrl = $"/Tickets/Purchase/{EventId}" });
            }

            try
            {
                // Get event details
                Event = await _eventService.GetEventByIdAsync(EventId);
                if (Event == null)
                {
                    ErrorMessage = "Event not found.";
                    return Page();
                }

                // Get seat map for the event
                var seatMap = await _seatMapService.GetSeatMapForEventAsync(EventId);
                
                // Map sections to view models
                SeatSections = seatMap.Sections.Select(s => new SeatSectionViewModel
                {
                    Name = s.Name,
                    Color = s.Color,
                    PriceMultiplier = s.PriceMultiplier,
                    BasePrice = Event.TicketPrice,
                    FinalPrice = Event.TicketPrice * s.PriceMultiplier
                }).ToList();

                // Generate seat layout
                for (int row = 1; row <= seatMap.Rows; row++)
                {
                    var rowSeats = new List<SeatViewModel>();
                    for (int col = 0; col < seatMap.Columns; col++)
                    {
                        // Find which section this seat belongs to
                        var section = seatMap.Sections.FirstOrDefault(s => 
                            row >= s.StartRow && row <= s.EndRow && 
                            col >= s.StartColumn && col <= s.EndColumn);

                        var seat = new SeatViewModel
                        {
                            Row = row,
                            Column = col,
                            Label = $"{(char)('A' + col)}{row}",
                            IsAvailable = await _seatMapService.IsSeatAvailableAsync(EventId, row, col),
                            Section = section?.Name ?? "Standard",
                            SectionColor = section?.Color ?? "#e0e0e0",
                            Price = Event.TicketPrice * (section?.PriceMultiplier ?? 1.0m)
                        };
                        rowSeats.Add(seat);
                    }
                    SeatLayout.Add(rowSeats);
                }
                
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading event details for purchase, EventId: {EventId}", EventId);
                ErrorMessage = "An error occurred while loading the event details.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger.LogInformation("Starting ticket purchase process...");

                // Skip validation for credit card fields if PayPal is selected
                if (PaymentMethod.ToLower() == "paypal")
                {
                    ModelState.Remove("CardNumber");
                    ModelState.Remove("ExpiryDate");
                    ModelState.Remove("Cvv");
                }

                if (!ModelState.IsValid)
                {
                    await OnGetAsync();
                    return Page();
                }

                // Get user ID from claims
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToPage("/Account/Login");
                }

                // Get the event to find the ticket price
                var eventDetails = await _eventService.GetEventByIdAsync(EventId);
                if (eventDetails == null)
                {
                    ErrorMessage = "Event not found";
                    await OnGetAsync();
                    return Page();
                }

                // Calculate the final price
                decimal finalPrice = eventDetails.TicketPrice;

                _logger.LogInformation("Processing purchase - Event: {EventId}, Customer: {CustomerId}, Price: {Price}, Payment Method: {PaymentMethod}",
                    EventId, userId, finalPrice, PaymentMethod);

                // Purchase the ticket
                var ticket = await _ticketPurchaseService.PurchaseTicketAsync(
                    EventId,
                    Guid.Parse(userId),
                    finalPrice,
                    SelectedRow,
                    SelectedColumn,
                    PaymentMethod);

                _logger.LogInformation("Ticket purchased successfully. TicketId: {TicketId}", ticket.Id);

                // Redirect to the confirmation page
                return RedirectToPage("/Tickets/Confirmation", new { id = ticket.Id });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation during ticket purchase");
                ErrorMessage = ex.Message;
                await OnGetAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error purchasing ticket");
                ErrorMessage = "An error occurred while purchasing the ticket.";
                await OnGetAsync();
                return Page();
            }
        }
    }

    public class SeatViewModel
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public string Label { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsSelected { get; set; }
        public string Section { get; set; }
        public string SectionColor { get; set; }
        public decimal Price { get; set; }
    }

    public class SeatSectionViewModel
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public decimal PriceMultiplier { get; set; }
        public decimal BasePrice { get; set; }
        public decimal FinalPrice { get; set; }
    }

    public class RequiredIfAttribute : ValidationAttribute
    {
        private readonly string _propertyName;
        private readonly object _desiredValue;

        public RequiredIfAttribute(string propertyName, object desiredValue)
        {
            _propertyName = propertyName;
            _desiredValue = desiredValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var instance = validationContext.ObjectInstance;
            var type = instance.GetType();
            var propertyValue = type.GetProperty(_propertyName).GetValue(instance, null);

            if (propertyValue.ToString().ToLower() == _desiredValue.ToString().ToLower() && string.IsNullOrEmpty(value?.ToString()))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
} 