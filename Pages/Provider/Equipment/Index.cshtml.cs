using System.Security.Claims;
using GearGrid.Data;
using GearGrid.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

// The folder name makes "Equipment" a namespace here, so the model gets an alias
using EquipmentModel = GearGrid.Models.Equipment;

namespace GearGrid.Pages.Provider.EquipmentPages;

[Authorize(Roles = "Provider")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IReadOnlyList<EquipmentModel> Items { get; private set; } = Array.Empty<EquipmentModel>();

    [TempData]
    public string? Message { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var company = await _context.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.OwnerUserId == userId);

        if (company is null)
        {
            return RedirectToPage("/Provider/CompanySetup");
        }

        // Locked until an admin approves the company
        if (company.VerificationStatus != "Approved")
        {
            return RedirectToPage("/Provider/Index");
        }

        // Only THIS company's equipment: multi-tenant isolation
        Items = await _context.Equipment
            .AsNoTracking()
            .Include(e => e.Category)
            .Where(e => e.CompanyId == company.CompanyId)
            .OrderByDescending(e => e.EquipmentId)
            .ToListAsync();

        return Page();
    }
}
