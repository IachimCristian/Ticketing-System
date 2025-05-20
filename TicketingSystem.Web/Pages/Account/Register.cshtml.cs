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
using System.Diagnostics;

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
        public RegisterInputModel RegisterInput { get; set; } = new RegisterInputModel();

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

        // Response class for API
        private class UserResponse
        {
            public Guid Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Clear validation errors for fields that don't apply to the selected user type
            if (RegisterInput.UserType.ToLower() == "customer")
            {
                // Clear validation errors for organizer fields
                ModelState.Remove("RegisterInput.OrganizationName");
                ModelState.Remove("RegisterInput.Description");
                ModelState.Remove("RegisterInput.ContactPhone");
                
                // Add customer-specific validation
                if (string.IsNullOrWhiteSpace(RegisterInput.Phone))
                {
                    ModelState.AddModelError("RegisterInput.Phone", "Phone is required for customers.");
                }
                if (string.IsNullOrWhiteSpace(RegisterInput.Address))
                {
                    ModelState.AddModelError("RegisterInput.Address", "Address is required for customers.");
                }
            }
            else if (RegisterInput.UserType.ToLower() == "organizer")
            {
                // Clear validation errors for customer fields
                ModelState.Remove("RegisterInput.Phone");
                ModelState.Remove("RegisterInput.Address");
                
                // Add organizer-specific validation
                if (string.IsNullOrWhiteSpace(RegisterInput.OrganizationName))
                {
                    ModelState.AddModelError("RegisterInput.OrganizationName", "Organization Name is required for organizers.");
                }
                if (string.IsNullOrWhiteSpace(RegisterInput.ContactPhone))
                {
                    ModelState.AddModelError("RegisterInput.ContactPhone", "Contact Phone is required for organizers.");
                }
            }
            
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Create a simple HttpClient that will connect directly to the working endpoint
                using var client = new HttpClient();
                
                HttpResponseMessage response = null;
                string endpoint = "";
                object requestData = null;
                
                // Create the request based on user type
                if (RegisterInput.UserType.ToLower() == "customer")
                {
                    endpoint = "http://localhost:5071/api/Users/customers";
                    requestData = new
                    {
                        Username = RegisterInput.Username,
                        Email = RegisterInput.Email,
                        Password = RegisterInput.Password,
                        Phone = RegisterInput.Phone,
                        Address = RegisterInput.Address
                    };
                }
                else if (RegisterInput.UserType.ToLower() == "organizer")
                {
                    endpoint = "http://localhost:5071/api/Users/organizers";
                    requestData = new
                    {
                        Username = RegisterInput.Username,
                        Email = RegisterInput.Email,
                        Password = RegisterInput.Password,
                        OrganizationName = RegisterInput.OrganizationName,
                        Description = RegisterInput.Description,
                        ContactPhone = RegisterInput.ContactPhone
                    };
                }
                
                // Log the request for debugging
                Debug.WriteLine($"Sending request to: {endpoint}");
                Debug.WriteLine($"Request data: {JsonSerializer.Serialize(requestData)}");
                
                // Send the request
                response = await client.PostAsJsonAsync(endpoint, requestData);
                
                // Log response for debugging
                Debug.WriteLine($"Response status: {response.StatusCode}");
                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Response content: {responseContent}");
                
                if (response.IsSuccessStatusCode)
                {
                    // Try to parse the response to get the user ID
                    try 
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };
                        
                        var userResponse = JsonSerializer.Deserialize<UserResponse>(responseContent, options);
                        
                        if (userResponse != null && userResponse.Id != Guid.Empty)
                        {
                            HttpContext.Session.SetString("UserId", userResponse.Id.ToString());
                            Debug.WriteLine($"Set user ID in session: {userResponse.Id}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to parse user ID from response: {ex.Message}");
                        // Continue anyway - we'll use the form data for session
                    }
                    
                    // Set session values from form data
                    HttpContext.Session.SetString("Username", RegisterInput.Username);
                    HttpContext.Session.SetString("Email", RegisterInput.Email);
                    HttpContext.Session.SetString("UserType", 
                        RegisterInput.UserType.ToLower() == "customer" ? "Customer" : "Organizer");

                    // Display success message
                    TempData["SuccessMessage"] = "Registration successful! You are now logged in.";
                    
                    return RedirectToPage("/Index");
                }
                else
                {
                    // If API call failed
                    ModelState.AddModelError(string.Empty, $"Registration failed: {responseContent}");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex.ToString()}");
                ModelState.AddModelError(string.Empty, $"An error occurred during registration: {ex.Message}");
                return Page();
            }
        }
    }
} 