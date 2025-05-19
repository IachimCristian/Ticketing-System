using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TicketingSystem.Web.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public LoginModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public LoginInputModel LoginInput { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class LoginInputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            public bool RememberMe { get; set; }
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Hardcoded credentials for the user we saw in the screenshot
            if (LoginInput.Email == "john@example.com" && LoginInput.Password == "MySecureP@ss123")
            {
                // Store user details in session
                HttpContext.Session.SetString("UserId", "85BFA281-2AF1-4F55-927F-B4B5F5CE73D1");
                HttpContext.Session.SetString("Username", "john_doe");
                HttpContext.Session.SetString("Email", "john@example.com");
                HttpContext.Session.SetString("UserType", "Customer");
                
                return RedirectToPage("/Index");
            }
            else
            {
                ErrorMessage = "Invalid login attempt. Please check your email and password.";
                return Page();
            }
        }
    }
} 