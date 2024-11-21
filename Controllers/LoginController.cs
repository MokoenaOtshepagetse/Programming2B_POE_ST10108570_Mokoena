using Claimed.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

public class LoginController : Controller
{
    private readonly ClaimsDbContext _context;

    public LoginController(ClaimsDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View(); // Display the login page.
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // If there are model errors, redisplay the login view with errors.
            return View("Index", model);
        }

        try
        {
            // Authentication logic
            var user = _context.Users.FirstOrDefault(u => u.Username == model.Username && u.PasswordHash == model.Password);

            if (user == null)
            {
                ModelState.AddModelError("LoginError", "Invalid credentials.");
                return View("Index", model);
            }

            if (user.IsAdmin)
            {
                // Admin login successful
                // Set authentication cookie or token for admin.
                return RedirectToAction("AdminDashboard", "Admin");
            }
            else
            {
                // Regular user login successful
                // Set authentication cookie or token for user.
                return RedirectToAction("UserDashboard", "User");
            }
        }
        catch (Exception ex)
        {
            // Handle any exceptions that might occur during authentication.
            ModelState.AddModelError("LoginError", "An error occurred during login. Please try again.");
            return View("Index", model);
        }
    }
}