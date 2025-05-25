using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Web.Pages.Customer
{
    public class NotificationsModel : PageModel
    {
        private readonly ICustomerNotificationService _notificationService;
        private readonly ILogger<NotificationsModel> _logger;

        public IEnumerable<CustomerNotification> Notifications { get; set; } = new List<CustomerNotification>();
        public int UnreadCount { get; set; }
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public NotificationsModel(
            ICustomerNotificationService notificationService,
            ILogger<NotificationsModel> logger)
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
                
                Notifications = await _notificationService.GetCustomerNotificationsAsync(customerId);
                UnreadCount = await _notificationService.GetUnreadCountAsync(customerId);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading notifications");
                ErrorMessage = "An error occurred while loading your notifications.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostMarkReadAsync(Guid id)
        {
            try
            {
                // Check if user is logged in and is a customer
                var userType = HttpContext.Session.GetString("UserType");
                if (userType != "Customer")
                {
                    return new JsonResult(new { success = false });
                }

                await _notificationService.MarkAsReadAsync(id);
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return new JsonResult(new { success = false });
            }
        }

        public async Task<IActionResult> OnPostMarkAllReadAsync()
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
                await _notificationService.MarkAllAsReadAsync(customerId);
                
                SuccessMessage = "All notifications marked as read.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                ErrorMessage = "An error occurred while marking notifications as read.";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnGetUnreadCountAsync()
        {
            try
            {
                // Check if user is logged in and is a customer
                var userId = HttpContext.Session.GetString("UserId");
                var userType = HttpContext.Session.GetString("UserType");
                
                if (string.IsNullOrEmpty(userId) || userType != "Customer")
                {
                    return new JsonResult(new { count = 0 });
                }

                var customerId = Guid.Parse(userId);
                var count = await _notificationService.GetUnreadCountAsync(customerId);
                
                return new JsonResult(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                return new JsonResult(new { count = 0 });
            }
        }
    }
} 