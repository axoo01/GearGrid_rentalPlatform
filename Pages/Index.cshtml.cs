using GearGrid.Data;
using GearGrid.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GearGrid.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IReadOnlyList<Category> Categories { get; private set; } = Array.Empty<Category>();

    public IReadOnlyList<Equipment> FeaturedEquipment { get; private set; } = Array.Empty<Equipment>();

    public bool HasEquipmentListings { get; private set; }

    public async Task OnGetAsync()
    {
        Categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(category => category.CategoryName)
            .ToListAsync();

        HasEquipmentListings = await _context.Equipment.AnyAsync();

        FeaturedEquipment = await _context.Equipment
            .AsNoTracking()
            .Where(equipment => equipment.Status == "Available" && equipment.AvailableQuantity > 0)
            .Include(equipment => equipment.Category)
            .Include(equipment => equipment.Company)
            .OrderByDescending(equipment => equipment.EquipmentId)
            .Take(3)
            .ToListAsync();
    }
}
