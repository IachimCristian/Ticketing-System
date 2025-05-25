using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Core.Services;

namespace TicketingSystem.Web.Pages.Admin
{
    public class TestNotificationsModel : PageModel
    {
        private readonly EventReminderService _eventReminderService;
        private readonly ICustomerNotificationService _customerNotificationService;
        private readonly IUserRepository<TicketingSystem.Core.Entities.Customer> _customerRepository;
        private readonly ILogger<TestNotificationsModel> _logger;

        public TestNotificationsModel(
            EventReminderService eventReminderService,
            ICustomerNotificationService customerNotificationService,
            IUserRepository<TicketingSystem.Core.Entities.Customer> customerRepository,
            ILogger<TestNotificationsModel> logger)
        {
            _eventReminderService = eventReminderService;
            _customerNotificationService = customerNotificationService;
            _customerRepository = customerRepository;
            _logger = logger;
        }

        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public void OnGet()
        {
            // Check if user is admin
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Admin")
            {
                ErrorMessage = "Access denied. Admin privileges required.";
            }
        }

        public async Task<IActionResult> OnPostSendEventRemindersAsync()
        {
            try
            {
                // Check if user is admin
                var userType = HttpContext.Session.GetString("UserType");
                if (userType != "Admin")
                {
                    ErrorMessage = "Access denied. Admin privileges required.";
                    return Page();
                }

                _logger.LogInformation("Admin triggered event reminders test");
                await _eventReminderService.SendEventRemindersAsync();
                
                SuccessMessage = "Event reminders have been sent! Check the terminal/logs to see the notification output.";
                _logger.LogInformation("Event reminders test completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending event reminders");
                ErrorMessage = $"Error sending event reminders: {ex.Message}";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSendTestNotificationAsync(string customerEmail, string notificationType)
        {
            try
            {
                // Check if user is admin
                var userType = HttpContext.Session.GetString("UserType");
                if (userType != "Admin")
                {
                    ErrorMessage = "Access denied. Admin privileges required.";
                    return Page();
                }

                if (string.IsNullOrEmpty(customerEmail) || string.IsNullOrEmpty(notificationType))
                {
                    ErrorMessage = "Please provide both customer email and notification type.";
                    return Page();
                }

                // Find customer by email
                var customers = await _customerRepository.GetAllAsync();
                var customer = customers.FirstOrDefault(c => c.Email.Equals(customerEmail, StringComparison.OrdinalIgnoreCase));

                if (customer == null)
                {
                    ErrorMessage = $"Customer with email '{customerEmail}' not found.";
                    return Page();
                }

                // Create test notification based on type
                string title, message;
                switch (notificationType.ToLower())
                {
                    case "test":
                        title = "Test Notification";
                        message = "This is a test notification from the admin panel. The notification system is working correctly!";
                        break;
                    case "welcome":
                        title = "Welcome to Ticksy!";
                        message = "Welcome to Ticksy! We're excited to have you as part of our community. Explore upcoming events and book your tickets today!";
                        break;
                    case "promotion":
                        title = "Special Promotion Alert!";
                        message = "🎉 Limited time offer! Get 20% off your next ticket purchase. Use code SAVE20 at checkout. Offer valid until the end of the month!";
                        break;
                    default:
                        ErrorMessage = "Invalid notification type.";
                        return Page();
                }

                // Send the test notification
                await _customerNotificationService.CreateNotificationAsync(
                    customer.Id,
                    title,
                    message,
                    "Test",
                    "Medium",
                    null,
                    null,
                    "/Dashboard",
                    "View Dashboard"
                );

                SuccessMessage = $"Test notification '{notificationType}' sent successfully to {customerEmail}! The customer can view it in their notifications page.";
                _logger.LogInformation("Test notification sent to customer {Email} of type {Type}", customerEmail, notificationType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test notification");
                ErrorMessage = $"Error sending test notification: {ex.Message}";
            }

            return Page();
        }
    }
} 