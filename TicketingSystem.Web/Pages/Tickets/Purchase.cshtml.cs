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
        private readonly TicketPurchaseFacade _ticketPurchaseFacade;
        private readonly IUserRepository<Customer> _customerRepository;
        private readonly ILogger<PurchaseModel> _logger;

        public PurchaseModel(
            IEventService eventService,
            ISeatMapService seatMapService,
            TicketPurchaseFacade ticketPurchaseFacade,
            IUserRepository<Customer> customerRepository,
            ILogger<PurchaseModel> logger)
        {
            _eventService = eventService;
            _seatMapService = seatMapService;
            _ticketPurchaseFacade = ticketPurchaseFacade;
            _customerRepository = customerRepository;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public Guid EventId { get; set; }

        [BindProperty]
        public int SelectedRow { get; set; }

        [BindProperty]
        public int SelectedColumn { get; set; }

        [BindProperty]
        public string PaymentMethod { get; set; } = "CreditCard";

        [BindProperty]
        [CreditCard(ErrorMessage = "The CardNumber field is not a valid credit card number.")]
        [RequiredIf("PaymentMethod", "CreditCard", ErrorMessage = "Card Number is required when using Credit Card payment.")]
        public string CardNumber { get; set; }

        [BindProperty]
        [RequiredIf("PaymentMethod", "CreditCard", ErrorMessage = "Expiry Date is required when using Credit Card payment.")]
        public string ExpiryDate { get; set; }

        [BindProperty]
        [RequiredIf("PaymentMethod", "CreditCard", ErrorMessage = "CVV is required when using Credit Card payment.")]
        public string Cvv { get; set; }

        public Event Event { get; set; }
        public List<List<SeatViewModel>> SeatLayout { get; set; } = new List<List<SeatViewModel>>();
        public List<SeatSectionViewModel> SeatSections { get; set; } = new List<SeatSectionViewModel>();
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is logged in
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid userId))
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
            // Skip validation for credit card fields if PayPal is selected
            if (PaymentMethod == "PayPal")
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

            // Get user ID from session
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid customerId))
            {
                return RedirectToPage("/Account/Login");
            }

            try
            {
                // Reserve the selected seat
                var seat = await _seatMapService.ReserveSeatAsync(EventId, SelectedRow, SelectedColumn, customerId);
                if (seat == null)
                {
                    ErrorMessage = "The selected seat is not available.";
                    await OnGetAsync();
                    return Page();
                }

                // Get the event to find the ticket price
                var eventDetails = await _eventService.GetEventByIdAsync(EventId);
                if (eventDetails == null)
                {
                    ErrorMessage = "Event not found";
                    await OnGetAsync();
                    return Page();
                }
                
                // Get the seat section price multiplier
                var seatMap = await _seatMapService.GetSeatMapForEventAsync(EventId);
                var section = seatMap.Sections.FirstOrDefault(s => 
                    SelectedRow >= s.StartRow && SelectedRow <= s.EndRow && 
                    SelectedColumn >= s.StartColumn && SelectedColumn <= s.EndColumn);
                
                // Calculate the final price
                decimal priceMultiplier = section?.PriceMultiplier ?? 1.0m;
                decimal finalPrice = eventDetails.TicketPrice * priceMultiplier;

                // Purchase the ticket
                var ticket = await _ticketPurchaseFacade.PurchaseTicketAsync(
                    EventId, 
                    customerId, 
                    finalPrice,
                    SelectedRow,
                    SelectedColumn,
                    null);
                
                // Redirect to the confirmation page with the ticket ID
                return RedirectToPage("/Tickets/Confirmation", new { ticketId = ticket.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error purchasing ticket, EventId: {EventId}, CustomerId: {CustomerId}", EventId, customerId);
                ErrorMessage = $"An error occurred while purchasing the ticket: {ex.Message}";
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

        public RequiredIfAttribute(string propertyName, object desiredValue, string errorMessage = "")
        {
            _propertyName = propertyName;
            _desiredValue = desiredValue;
            ErrorMessage = errorMessage;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var instance = validationContext.ObjectInstance;
            var type = instance.GetType();
            var propertyValue = type.GetProperty(_propertyName).GetValue(instance, null);

            if (propertyValue?.ToString() == _desiredValue.ToString())
            {
                if (value == null || (value is string stringValue && string.IsNullOrWhiteSpace(stringValue)))
                {
                    return new ValidationResult(ErrorMessage);
                }
            }

            return ValidationResult.Success;
        }
    }
} 