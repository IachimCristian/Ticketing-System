using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;

namespace TicketingSystem.Web.Pages.Account
{
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync()
        {
            // Sign out the user
            await HttpContext.SignOutAsync("Cookies");
            
            // Clear all session data
            HttpContext.Session.Clear();
            
            return RedirectToPage("/Index");
        }
    }
} 