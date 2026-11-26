using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using GearGrid.Data;
using GearGrid.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GearGrid.Pages.Provider.EquipmentPages;

[Authorize(Roles = "Provider")]
public class EditModel : PageModel
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxImageBytes = 2 * 1024 * 1024;

    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public EditModel(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [BindProperty]
    public EquipmentInput Input { get; set; } = new();

    [BindProperty]
    public IFormFile? Image { get; set; }

    public string? CurrentImageUrl { get; private set; }
    public int RentedUnits { get; private set; }
    public SelectList Categories { get; private set; } = null!;

    public class EquipmentInput
    {
        [Required, Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(100), Display(Name = "Serial / asset code")]
        public string SerialOrAssetCode { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, 100000000), Display(Name = "Daily rate")]
        public decimal DailyRate { get; set; }

        [Range(0, 100000000), Display(Name = "Security deposit")]
        public decimal SecurityDeposit { get; set; }

        [Range(1, 100000), Display(Name = "Total quantity")]
        public int TotalQuantity { get; set; }

        [Required]
        public string Status { get; set; } = "Available";

        [Display(Name = "Specific terms (optional)")]
        public string? SpecificTerms { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var company = await GetApprovedCompanyAsync();
        if (company is null)
        {
            return RedirectToPage("/Provider/Index");
        }

        // Ownership check: the item must belong to THIS provider's company
        var item = await _context.Equipment
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EquipmentId == id && e.CompanyId == company.CompanyId);

        if (item is null)
        {
            return NotFound();
        }

        Input = new EquipmentInput
        {
            CategoryId = item.CategoryId,
            Name = item.Name,
            SerialOrAssetCode = item.SerialOrAssetCode,
            Description = item.Description,
            DailyRate = item.DailyRate,
            SecurityDeposit = item.SecurityDeposit,
            TotalQuantity = item.TotalQuantity,
            Status = item.Status == "Maintenance" ? "Maintenance" : "Available",
            SpecificTerms = item.SpecificTerms
        };
        CurrentImageUrl = item.ImageUrl;
        RentedUnits = item.TotalQuantity - item.AvailableQuantity;

        await LoadCategoriesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var company = await GetApprovedCompanyAsync();
        if (company is null)
        {
            return RedirectToPage("/Provider/Index");
        }

        // Never trust the id from the URL: re-check ownership on every request
        var item = await _context.Equipment
            .FirstOrDefaultAsync(e => e.EquipmentId == id && e.CompanyId == company.CompanyId);

        if (item is null)
        {
            return NotFound();
        }

        var rentedUnits = item.TotalQuantity - item.AvailableQuantity;

        if (!await _context.Categories.AnyAsync(c => c.CategoryId == Input.CategoryId))
        {
            ModelState.AddModelError("Input.CategoryId", "Choose a valid category.");
        }

        if (Input.Status is not ("Available" or "Maintenance"))
        {
            ModelState.AddModelError("Input.Status", "Choose Available or Maintenance.");
        }

        // Can't shrink the stock below the units that are currently rented out
        if (Input.TotalQuantity < rentedUnits)
        {
            ModelState.AddModelError("Input.TotalQuantity",
                $"{rentedUnits} unit(s) are currently rented, so the total can't be lower than that.");
        }

        if (Image is { Length: > 0 })
        {
            var extension = Path.GetExtension(Image.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(nameof(Image), "Only .jpg, .jpeg, .png or .webp images are allowed.");
            }
            else if (Image.Length > MaxImageBytes)
            {
                ModelState.AddModelError(nameof(Image), "The image must be 2 MB or smaller.");
            }
        }

        if (!ModelState.IsValid)
        {
            CurrentImageUrl = item.ImageUrl;
            RentedUnits = rentedUnits;
            await LoadCategoriesAsync();
            return Page();
        }

        item.CategoryId = Input.CategoryId;
        item.Name = Input.Name.Trim();
        item.SerialOrAssetCode = Input.SerialOrAssetCode.Trim();
        item.Description = Input.Description.Trim();
        item.DailyRate = Input.DailyRate;
        item.SecurityDeposit = Input.SecurityDeposit;
        item.TotalQuantity = Input.TotalQuantity;
        item.AvailableQuantity = Input.TotalQuantity - rentedUnits;
        item.Status = Input.Status;
        item.SpecificTerms = string.IsNullOrWhiteSpace(Input.SpecificTerms) ? null : Input.SpecificTerms.Trim();

        if (Image is { Length: > 0 })
        {
            var oldUrl = item.ImageUrl;
            item.ImageUrl = await SaveImageAsync(Image);
            DeleteImageFile(oldUrl);
        }

        await _context.SaveChangesAsync();

        TempData["Message"] = $"\"{item.Name}\" was updated.";
        return RedirectToPage("/Provider/Equipment/Index");
    }

    private async Task<Company?> GetApprovedCompanyAsync()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        return await _context.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.OwnerUserId == userId
                                   && c.VerificationStatus == "Approved");
    }

    private async Task LoadCategoriesAsync()
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.CategoryName)
            .ToListAsync();

        Categories = new SelectList(categories, "CategoryId", "CategoryName");
    }

    private async Task<string> SaveImageAsync(IFormFile file)
    {
        var folder = Path.Combine(_environment.WebRootPath, "images", "equipment");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
        await using var stream = System.IO.File.Create(Path.Combine(folder, fileName));
        await file.CopyToAsync(stream);

        return $"/images/equipment/{fileName}";
    }

    private void DeleteImageFile(string? imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl) || !imageUrl.StartsWith("/images/equipment/"))
        {
            return;
        }

        // GetFileName strips any folder tricks, so only files in our folder can be deleted
        var path = Path.Combine(_environment.WebRootPath, "images", "equipment", Path.GetFileName(imageUrl));
        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
        }
    }
}
