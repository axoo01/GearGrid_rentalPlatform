using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using GearGrid.Data;
using GearGrid.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GearGrid.Pages.Provider;

[Authorize(Roles = "Provider")]
public class CompanySetupModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CompanySetupModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public CompanyInput Input { get; set; } = new();

    public class CompanyInput
    {
        [Required, MaxLength(150), Display(Name = "Company name")]
        public string CompanyName { get; set; } = string.Empty;

        [Required, MaxLength(50), Display(Name = "Registration number (TIN / RDB)")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required, MaxLength(20), Phone, Display(Name = "Business phone")]
        public string BusinessPhone { get; set; } = string.Empty;

        [Required, MaxLength(150), EmailAddress, Display(Name = "Business email")]
        public string BusinessEmail { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required, MaxLength(250), Display(Name = "Physical address")]
        public string PhysicalAddress { get; set; } = string.Empty;

        [Required, Display(Name = "Default terms & conditions")]
        public string DefaultTermsAndConditions { get; set; } = string.Empty;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> OnGetAsync()
    {
        // One company per provider: if it already exists, go to the dashboard
        if (await _context.Companies.AnyAsync(c => c.OwnerUserId == CurrentUserId))
        {
            return RedirectToPage("/Provider/Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (await _context.Companies.AnyAsync(c => c.OwnerUserId == CurrentUserId))
        {
            return RedirectToPage("/Provider/Index");
        }

        var company = new Company
        {
            OwnerUserId = CurrentUserId,
            CompanyName = Input.CompanyName.Trim(),
            RegistrationNumber = Input.RegistrationNumber.Trim(),
            Description = Input.Description.Trim(),
            BusinessPhone = Input.BusinessPhone.Trim(),
            BusinessEmail = Input.BusinessEmail.Trim(),
            City = Input.City.Trim(),
            PhysicalAddress = Input.PhysicalAddress.Trim(),
            DefaultTermsAndConditions = Input.DefaultTermsAndConditions.Trim(),
            VerificationStatus = "Pending",   // set by the server, never by the form
            CreatedAt = DateTime.UtcNow
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

        return RedirectToPage("/Provider/Index");
    }
}