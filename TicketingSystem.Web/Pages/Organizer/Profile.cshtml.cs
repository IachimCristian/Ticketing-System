using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Core.Entities;

namespace TicketingSystem.Web.Pages.Organizer
{
    public class ProfileModel : PageModel
    {
        private readonly IUserRepository<TicketingSystem.Core.Entities.Organizer> _organizerRepository;
        private readonly ILogger<ProfileModel> _logger;

        public ProfileModel(
            IUserRepository<TicketingSystem.Core.Entities.Organizer> organizerRepository,
            ILogger<ProfileModel> logger)
        {
            _organizerRepository = organizerRepository;
            _logger = logger;
        }

        [BindProperty]
        public TicketingSystem.Core.Entities.Organizer Organizer { get; set; }

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
                // Check if user is logged in and is an organizer
                var userId = HttpContext.Session.GetString("UserId");
                var userType = HttpContext.Session.GetString("UserType");

                if (string.IsNullOrEmpty(userId) || userType != "Organizer")
                {
                    return RedirectToPage("/Account/Login");
                }

                if (Guid.TryParse(userId, out Guid organizerId))
                {
                    Organizer = await _organizerRepository.GetByIdAsync(organizerId);
                    if (Organizer == null)
                    {
                        ErrorMessage = "Organizer not found.";
                        return RedirectToPage("/Organizer/Dashboard");
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
                _logger.LogError(ex, "Error loading organizer profile");
                ErrorMessage = "Error loading profile. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // Check if user is logged in and is an organizer
                var userId = HttpContext.Session.GetString("UserId");
                var userType = HttpContext.Session.GetString("UserType");

                if (string.IsNullOrEmpty(userId) || userType != "Organizer")
                {
                    return RedirectToPage("/Account/Login");
                }

                if (!Guid.TryParse(userId, out Guid organizerId))
                {
                    ErrorMessage = "Invalid user session.";
                    return RedirectToPage("/Account/Login");
                }

                // Ensure the organizer being updated belongs to the current user
                if (Organizer.Id != organizerId)
                {
                    ErrorMessage = "Unauthorized access.";
                    return RedirectToPage("/Organizer/Dashboard");
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

                    // Get current organizer to verify current password
                    var currentOrganizer = await _organizerRepository.GetByIdAsync(organizerId);
                    if (currentOrganizer == null || currentOrganizer.Password != CurrentPassword)
                    {
                        ErrorMessage = "Current password is incorrect.";
                        return Page();
                    }

                    // Update password
                    Organizer.Password = NewPassword;
                }
                else
                {
                    // If not changing password, keep the existing password
                    var currentOrganizer = await _organizerRepository.GetByIdAsync(organizerId);
                    Organizer.Password = currentOrganizer.Password;
                }

                // Validate required fields
                if (string.IsNullOrEmpty(Organizer.Username))
                {
                    ErrorMessage = "Username is required.";
                    return Page();
                }

                if (string.IsNullOrEmpty(Organizer.Email))
                {
                    ErrorMessage = "Email is required.";
                    return Page();
                }

                if (string.IsNullOrEmpty(Organizer.OrganizationName))
                {
                    ErrorMessage = "Organization name is required.";
                    return Page();
                }

                // Check if username or email already exists (for other organizers)
                var allOrganizers = await _organizerRepository.GetAllAsync();
                foreach (var existingOrganizer in allOrganizers)
                {
                    if (existingOrganizer.Id != organizerId)
                    {
                        if (existingOrganizer.Username.Equals(Organizer.Username, StringComparison.OrdinalIgnoreCase))
                        {
                            ErrorMessage = "Username already exists. Please choose a different username.";
                            return Page();
                        }

                        if (existingOrganizer.Email.Equals(Organizer.Email, StringComparison.OrdinalIgnoreCase))
                        {
                            ErrorMessage = "Email already exists. Please use a different email address.";
                            return Page();
                        }
                    }
                }

                // Update the organizer
                await _organizerRepository.UpdateAsync(Organizer);
                await _organizerRepository.SaveChangesAsync();

                // Update session username if it changed
                if (HttpContext.Session.GetString("Username") != Organizer.Username)
                {
                    HttpContext.Session.SetString("Username", Organizer.Username);
                }

                SuccessMessage = "Profile updated successfully!";
                _logger.LogInformation("Organizer profile updated successfully for organizer {OrganizerId}", organizerId);

                // Clear password fields
                CurrentPassword = "";
                NewPassword = "";
                ConfirmPassword = "";

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating organizer profile");
                ErrorMessage = "Error updating profile. Please try again.";
                return Page();
            }
        }
    }
} 