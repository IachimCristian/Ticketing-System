using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Core.Entities;

namespace TicketingSystem.Web.Pages.Customer
{
    public class ProfileModel : PageModel
    {
        private readonly IUserRepository<TicketingSystem.Core.Entities.Customer> _customerRepository;
        private readonly ILogger<ProfileModel> _logger;

        public ProfileModel(
            IUserRepository<TicketingSystem.Core.Entities.Customer> customerRepository,
            ILogger<ProfileModel> logger)
        {
            _customerRepository = customerRepository;
            _logger = logger;
        }

        [BindProperty]
        public TicketingSystem.Core.Entities.Customer Customer { get; set; }

        [BindProperty]
        public string CurrentPassword { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

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

                if (Guid.TryParse(userId, out Guid customerId))
                {
                    Customer = await _customerRepository.GetByIdAsync(customerId);
                    if (Customer == null)
                    {
                        ErrorMessage = "Customer not found.";
                        return RedirectToPage("/Dashboard/Index");
                    }
                }
                else
                {
                    ErrorMessage = "Invalid user session.";
                    return RedirectToPage("/Account/Login");
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customer profile");
                ErrorMessage = "Error loading profile. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
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

                if (!Guid.TryParse(userId, out Guid customerId))
                {
                    ErrorMessage = "Invalid user session.";
                    return RedirectToPage("/Account/Login");
                }

                // Ensure the customer being updated belongs to the current user
                if (Customer.Id != customerId)
                {
                    ErrorMessage = "Unauthorized access.";
                    return RedirectToPage("/Dashboard/Index");
                }

                // Validate password change if provided
                if (!string.IsNullOrEmpty(NewPassword))
                {
                    if (string.IsNullOrEmpty(CurrentPassword))
                    {
                        ErrorMessage = "Current password is required to change password.";
                        return Page();
                    }

                    if (NewPassword != ConfirmPassword)
                    {
                        ErrorMessage = "New password and confirmation do not match.";
                        return Page();
                    }

                    if (NewPassword.Length < 6)
                    {
                        ErrorMessage = "New password must be at least 6 characters long.";
                        return Page();
                    }

                    // Get current customer to verify current password
                    var currentCustomer = await _customerRepository.GetByIdAsync(customerId);
                    if (currentCustomer == null || currentCustomer.Password != CurrentPassword)
                    {
                        ErrorMessage = "Current password is incorrect.";
                        return Page();
                    }

                    // Update password
                    Customer.Password = NewPassword;
                }
                else
                {
                    // If not changing password, keep the existing password
                    var currentCustomer = await _customerRepository.GetByIdAsync(customerId);
                    Customer.Password = currentCustomer.Password;
                }

                // Validate required fields
                if (string.IsNullOrEmpty(Customer.Username))
                {
                    ErrorMessage = "Username is required.";
                    return Page();
                }

                if (string.IsNullOrEmpty(Customer.Email))
                {
                    ErrorMessage = "Email is required.";
                    return Page();
                }

                // Check if username or email already exists (for other customers)
                var allCustomers = await _customerRepository.GetAllAsync();
                foreach (var existingCustomer in allCustomers)
                {
                    if (existingCustomer.Id != customerId)
                    {
                        if (existingCustomer.Username.Equals(Customer.Username, StringComparison.OrdinalIgnoreCase))
                        {
                            ErrorMessage = "Username already exists. Please choose a different username.";
                            return Page();
                        }

                        if (existingCustomer.Email.Equals(Customer.Email, StringComparison.OrdinalIgnoreCase))
                        {
                            ErrorMessage = "Email already exists. Please use a different email address.";
                            return Page();
                        }
                    }
                }

                // Update the customer
                await _customerRepository.UpdateAsync(Customer);
                await _customerRepository.SaveChangesAsync();

                // Update session username if it changed
                if (HttpContext.Session.GetString("Username") != Customer.Username)
                {
                    HttpContext.Session.SetString("Username", Customer.Username);
                }

                SuccessMessage = "Profile updated successfully!";
                _logger.LogInformation("Customer profile updated successfully for customer {CustomerId}", customerId);

                // Clear password fields
                CurrentPassword = "";
                NewPassword = "";
                ConfirmPassword = "";

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer profile");
                ErrorMessage = "Error updating profile. Please try again.";
                return Page();
            }
        }
    }
} 