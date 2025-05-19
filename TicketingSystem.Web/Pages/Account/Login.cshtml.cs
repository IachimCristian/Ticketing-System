using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using TicketingSystem.Core.Services;

namespace TicketingSystem.Web.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IAuthenticationService _authService;

        public LoginModel(IAuthenticationService authService)
        {
            _authService = authService;
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

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Authenticate user using the service
                var user = await _authService.AuthenticateAsync(LoginInput.Email, LoginInput.Password);
                
                if (user != null)
                {
                    // Get user type (Admin, Customer, Organizer)
                    string userType = _authService.GetUserType(user);
                    
                    // Store user details in session
                    HttpContext.Session.SetString("UserId", user.Id.ToString());
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("Email", user.Email);
                    HttpContext.Session.SetString("UserType", userType);
                    
                    return RedirectToPage("/Index");
                }
                
                ErrorMessage = "Invalid login attempt. Please check your email and password.";
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred during login. Please try again later.";
                return Page();
            }
        }
    }
} 