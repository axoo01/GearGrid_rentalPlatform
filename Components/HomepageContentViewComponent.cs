using GearGrid.Data;
using GearGrid.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GearGrid.Components;

public sealed class HomepageContentViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public HomepageContentViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync(
        IReadOnlyList<Equipment>? featuredEquipment = null,
        bool? hasEquipmentListings = null)
    {
        if (hasEquipmentListings is null)
        {
            hasEquipmentListings = await _context.Equipment.AnyAsync();
        }

        if (featuredEquipment is null)
        {
            featuredEquipment = await _context.Equipment
                .AsNoTracking()
                .Where(equipment => equipment.Status == "Available" && equipment.AvailableQuantity > 0)
                .Include(equipment => equipment.Category)
                .Include(equipment => equipment.Company)
                .OrderByDescending(equipment => equipment.EquipmentId)
                .Take(3)
                .ToListAsync();
        }

        return View(new HomepageContentViewModel(featuredEquipment, hasEquipmentListings.Value));
    }
}

public sealed record HomepageContentViewModel(
    IReadOnlyList<Equipment> FeaturedEquipment,
    bool HasEquipmentListings);
