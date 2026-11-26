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
public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public DeleteModel(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public EquipmentModel Item { get; private set; } = null!;
    public bool HasOrders { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var item = await FindOwnedItemAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        Item = item;
        HasOrders = await _context.RentalOrders.AnyAsync(o => o.EquipmentId == id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var item = await FindOwnedItemAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        // Equipment with rental history must stay (orders point to it)
        if (await _context.RentalOrders.AnyAsync(o => o.EquipmentId == id))
        {
            TempData["Message"] = "This item has rental orders, so it can't be deleted. Set it to Maintenance instead.";
            return RedirectToPage("/Provider/Equipment/Index");
        }

        var imageUrl = item.ImageUrl;
        _context.Equipment.Remove(item);
        await _context.SaveChangesAsync();

        if (!string.IsNullOrEmpty(imageUrl) && imageUrl.StartsWith("/images/equipment/"))
        {
            var path = Path.Combine(_environment.WebRootPath, "images", "equipment", Path.GetFileName(imageUrl));
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }

        TempData["Message"] = "The equipment was deleted.";
        return RedirectToPage("/Provider/Equipment/Index");
    }

    // Ownership check lives in ONE place: the item must belong to this provider's approved company
    private async Task<EquipmentModel?> FindOwnedItemAsync(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        return await _context.Equipment
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.EquipmentId == id
                                   && e.Company.OwnerUserId == userId
                                   && e.Company.VerificationStatus == "Approved");
    }
}
