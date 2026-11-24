using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using GearGrid.Data;
using GearGrid.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GearGrid.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public RegisterModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public RegistrationInput Input { get; set; } = new();

    public class RegistrationInput
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Renter";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.Role is not ("Renter" or "Provider"))
        {
            ModelState.AddModelError("Input.Role", "Choose a valid account type.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var email = Input.Email.Trim();
        if (await _context.Users.AnyAsync(user => user.Email == email))
        {
            ModelState.AddModelError("Input.Email", "An account with this email already exists.");
            return Page();
        }

        var user = new User
        {
            FullName = Input.FullName.Trim(),
            Email = email,
            PhoneNumber = Input.PhoneNumber.Trim(),
            Role = Input.Role
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, Input.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await HttpContext.SignInAsync(
            CreateCookiePrincipal(user),
            new AuthenticationProperties { IsPersistent = false });

        return RedirectToPage("/Index");
    }

    private static ClaimsPrincipal CreateCookiePrincipal(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        return new ClaimsPrincipal(new ClaimsIdentity(
            claims,
            Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme));
    }
}
