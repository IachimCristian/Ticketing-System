using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Core.Entities;

namespace TicketingSystem.Web.Pages.Organizer
{
    public class ScanTicketModel : PageModel
    {
        private readonly ITicketValidationService _ticketValidationService;
        private readonly ITicketRepository _ticketRepository;
        
        public ScanTicketModel(
            ITicketValidationService ticketValidationService,
            ITicketRepository ticketRepository)
        {
            _ticketValidationService = ticketValidationService ?? throw new ArgumentNullException(nameof(ticketValidationService));
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
        }

        [BindProperty]
        public string QRCode { get; set; }
        
        [TempData]
        public string StatusMessage { get; set; }
        
        [TempData]
        public string StatusMessageType { get; set; } // success, danger, warning
        
        public Ticket ValidatedTicket { get; set; }
        public Event TicketEvent { get; set; }
        public TicketingSystem.Core.Entities.Customer TicketCustomer { get; set; }
        
        public IActionResult OnGet()
        {
            // Check if user is logged in and is an organizer
            var userId = HttpContext.Session.GetString("UserId");
            var userType = HttpContext.Session.GetString("UserType");
            
            if (string.IsNullOrEmpty(userId) || userType != "Organizer")
            {
                return RedirectToPage("/Account/Login");
            }

            return Page();
        }
        
        public async Task<IActionResult> OnPostValidateAsync()
        {
            // Check if user is logged in and is an organizer
            var userId = HttpContext.Session.GetString("UserId");
            var userType = HttpContext.Session.GetString("UserType");
            
            if (string.IsNullOrEmpty(userId) || userType != "Organizer")
            {
                return RedirectToPage("/Account/Login");
            }
            
            if (string.IsNullOrEmpty(QRCode))
            {
                StatusMessage = "Please enter a QR code.";
                StatusMessageType = "warning";
                return Page();
            }
            
            try
            {
                var validationResult = await _ticketValidationService.ValidateTicketByQRCodeAsync(QRCode);
                
                if (validationResult.IsValid)
                {
                    ValidatedTicket = validationResult.Ticket;
                    TicketEvent = validationResult.Event;
                    TicketCustomer = validationResult.Customer;
                    
                    StatusMessage = "Ticket is valid! Event: " + TicketEvent?.Title;
                    StatusMessageType = "success";
                }
                else
                {
                    StatusMessage = validationResult.Message;
                    StatusMessageType = "danger";
                }
                
                return Page();
            }
            catch (Exception ex)
            {
                StatusMessage = "Error validating ticket: " + ex.Message;
                StatusMessageType = "danger";
                return Page();
            }
        }
        
        public async Task<IActionResult> OnPostMarkUsedAsync(Guid ticketId)
        {
            // Check if user is logged in and is an organizer
            var userId = HttpContext.Session.GetString("UserId");
            var userType = HttpContext.Session.GetString("UserType");
            
            if (string.IsNullOrEmpty(userId) || userType != "Organizer")
            {
                return RedirectToPage("/Account/Login");
            }
            
            try
            {
                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                {
                    StatusMessage = "Ticket not found.";
                    StatusMessageType = "danger";
                    return Page();
                }
                
                ticket.Status = "Used";
                await _ticketRepository.UpdateAsync(ticket);
                await _ticketRepository.SaveChangesAsync();
                
                StatusMessage = "Ticket marked as used successfully!";
                StatusMessageType = "success";
                
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                StatusMessage = "Error marking ticket as used: " + ex.Message;
                StatusMessageType = "danger";
                return Page();
            }
        }
    }
} 