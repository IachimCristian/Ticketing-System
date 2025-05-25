using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Web.Pages.Customer
{
    [Authorize(Policy = "CustomerOnly")]
    public class NotificationPreferencesModel : PageModel
    {
        private readonly ICustomerNotificationService _notificationService;
        private readonly ILogger<NotificationPreferencesModel> _logger;

        [BindProperty]
        public NotificationPreferences Preferences { get; set; }
        
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public NotificationPreferencesModel(
            ICustomerNotificationService notificationService,
            ILogger<NotificationPreferencesModel> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToPage("/Account/Login");
                }

                var customerId = Guid.Parse(userId);
                Preferences = await _notificationService.GetNotificationPreferencesAsync(customerId);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading notification preferences");
                ErrorMessage = "An error occurred while loading your notification preferences.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToPage("/Account/Login");
                }

                var customerId = Guid.Parse(userId);
                
                // Ensure the preferences belong to the current user
                if (Preferences.CustomerId != customerId)
                {
                    ErrorMessage = "Invalid request.";
                    return Page();
                }

                await _notificationService.UpdateNotificationPreferencesAsync(Preferences);
                
                SuccessMessage = "Your notification preferences have been updated successfully.";
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification preferences");
                ErrorMessage = "An error occurred while updating your notification preferences.";
                return Page();
            }
        }
    }
} 