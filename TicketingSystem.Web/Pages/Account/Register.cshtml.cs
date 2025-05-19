using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json;

namespace TicketingSystem.Web.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public RegisterModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public RegisterInputModel RegisterInput { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class RegisterInputModel
        {
            [Required]
            public string UserType { get; set; } = "customer";

            // Common fields
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
            public string Username { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            // Customer specific fields
            [Phone]
            public string Phone { get; set; }
            
            public string Address { get; set; }

            // Organizer specific fields
            public string OrganizationName { get; set; }
            
            public string Description { get; set; }
            
            [Phone]
            public string ContactPhone { get; set; }
        }

        // Response classes to match API responses
        public class UserResponse
        {
            public Guid Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string Message { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Add conditional validation based on user type
            if (RegisterInput.UserType.ToLower() == "organizer")
            {
                if (string.IsNullOrWhiteSpace(RegisterInput.OrganizationName))
                {
                    ModelState.AddModelError("RegisterInput.OrganizationName", "Organization Name is required for organizers.");
                }
                if (string.IsNullOrWhiteSpace(RegisterInput.ContactPhone))
                {
                    ModelState.AddModelError("RegisterInput.ContactPhone", "Contact Phone is required for organizers.");
                }
            }
            else if (RegisterInput.UserType.ToLower() == "customer")
            {
                if (string.IsNullOrWhiteSpace(RegisterInput.Phone))
                {
                    ModelState.AddModelError("RegisterInput.Phone", "Phone is required for customers.");
                }
                if (string.IsNullOrWhiteSpace(RegisterInput.Address))
                {
                    ModelState.AddModelError("RegisterInput.Address", "Address is required for customers.");
                }
            }
            
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Get the API client
                var client = _clientFactory.CreateClient("TicketingAPI");
                HttpResponseMessage response;

                // Hash the password before sending to API
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(RegisterInput.Password);

                // Send a POST request to the appropriate API endpoint based on user type
                if (RegisterInput.UserType.ToLower() == "customer")
                {
                    var customerDto = new
                    {
                        Username = RegisterInput.Username,
                        Email = RegisterInput.Email,
                        Password = passwordHash, // Send hashed password
                        Phone = RegisterInput.Phone,
                        Address = RegisterInput.Address
                    };

                    response = await client.PostAsJsonAsync("api/Users/customers", customerDto);
                }
                else if (RegisterInput.UserType.ToLower() == "organizer")
                {
                    var organizerDto = new
                    {
                        Username = RegisterInput.Username,
                        Email = RegisterInput.Email,
                        Password = passwordHash, // Send hashed password
                        OrganizationName = RegisterInput.OrganizationName,
                        Description = RegisterInput.Description,
                        ContactPhone = RegisterInput.ContactPhone
                    };

                    response = await client.PostAsJsonAsync("api/Users/organizers", organizerDto);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid user type selected.");
                    return Page();
                }

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        // Read the response content as a strongly-typed object
                        var userResponse = await response.Content.ReadFromJsonAsync<UserResponse>();

                        // Set session variables
                        HttpContext.Session.SetString("UserId", userResponse.Id.ToString());
                        HttpContext.Session.SetString("Username", userResponse.Username);
                        HttpContext.Session.SetString("Email", userResponse.Email);
                        HttpContext.Session.SetString("UserType", 
                            RegisterInput.UserType.ToLower() == "customer" ? "Customer" : "Organizer");

                        return RedirectToPage("/Index");
                    }
                    catch (Exception)
                    {
                        // If we can't parse the response but it was successful, still proceed
                        HttpContext.Session.SetString("Username", RegisterInput.Username);
                        HttpContext.Session.SetString("Email", RegisterInput.Email);
                        HttpContext.Session.SetString("UserType", 
                            RegisterInput.UserType.ToLower() == "customer" ? "Customer" : "Organizer");

                        return RedirectToPage("/Index");
                    }
                }
                else
                {
                    // If the API returned an error, show it to the user
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, $"Registration failed: {errorContent}");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred during registration: {ex.Message}");
                return Page();
            }
        }
    }
} 