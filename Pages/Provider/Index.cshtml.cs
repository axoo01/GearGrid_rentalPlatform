using System.Security.Claims;
using GearGrid.Data;
using GearGrid.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GearGrid.Pages.Provider;

[Authorize(Roles = "Provider")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public Company Company { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var company = await _context.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.OwnerUserId == userId);

        // No company yet: send the provider to the setup form
        if (company is null)
        {
            return RedirectToPage("/Provider/CompanySetup");
        }

        Company = company;
        return Page();
    }
}