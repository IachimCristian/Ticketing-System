using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Linq;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Web.Pages.Customer
{
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
                // Check if user is logged in and is a customer
                var userId = HttpContext.Session.GetString("UserId");
                var userType = HttpContext.Session.GetString("UserType");
                
                if (string.IsNullOrEmpty(userId) || userType != "Customer")
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
                _logger.LogInformation("OnPostAsync called");
                
                // Check if user is logged in and is a customer
                var userId = HttpContext.Session.GetString("UserId");
                var userType = HttpContext.Session.GetString("UserType");
                
                _logger.LogInformation("UserId: {UserId}, UserType: {UserType}", userId, userType);
                
                if (string.IsNullOrEmpty(userId) || userType != "Customer")
                {
                    _logger.LogWarning("User not authorized - redirecting to login");
                    return RedirectToPage("/Account/Login");
                }

                if (!ModelState.IsValid)
                {
                    // Remove Customer validation error since we don't need the navigation property
                    ModelState.Remove("Preferences.Customer");
                    
                    if (!ModelState.IsValid)
                    {
                        _logger.LogWarning("ModelState is invalid");
                        foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                        {
                            _logger.LogWarning("ModelState error: {Error}", error.ErrorMessage);
                        }
                        return Page();
                    }
                }

                var customerId = Guid.Parse(userId);
                
                _logger.LogInformation("Updating preferences for customer {CustomerId}", customerId);
                _logger.LogInformation("Preferences ID: {PreferencesId}", Preferences?.Id);
                _logger.LogInformation("EmailTicketPurchase: {EmailTicketPurchase}", Preferences?.EmailTicketPurchase);
                
                // Ensure the preferences belong to the current user
                Preferences.CustomerId = customerId;

                await _notificationService.UpdateNotificationPreferencesAsync(Preferences);
                
                _logger.LogInformation("Preferences updated successfully");
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