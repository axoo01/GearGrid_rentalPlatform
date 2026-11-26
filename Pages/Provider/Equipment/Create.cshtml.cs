using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using GearGrid.Data;
using GearGrid.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

// The folder name makes "Equipment" a namespace here, so the model gets an alias
using EquipmentModel = GearGrid.Models.Equipment;

namespace GearGrid.Pages.Provider.EquipmentPages;

[Authorize(Roles = "Provider")]
public class CreateModel : PageModel
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxImageBytes = 2 * 1024 * 1024; // 2 MB

    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public CreateModel(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [BindProperty]
    public EquipmentInput Input { get; set; } = new();

    [BindProperty]
    public IFormFile? Image { get; set; }

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
        public int TotalQuantity { get; set; } = 1;

        [Display(Name = "Specific terms (optional)")]
        public string? SpecificTerms { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var company = await GetApprovedCompanyAsync();
        if (company is null)
        {
            return RedirectToPage("/Provider/Index");
        }

        await LoadCategoriesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var company = await GetApprovedCompanyAsync();
        if (company is null)
        {
            return RedirectToPage("/Provider/Index");
        }

        if (!await _context.Categories.AnyAsync(c => c.CategoryId == Input.CategoryId))
        {
            ModelState.AddModelError("Input.CategoryId", "Choose a valid category.");
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
            await LoadCategoriesAsync();
            return Page();
        }

        string? imageUrl = null;
        if (Image is { Length: > 0 })
        {
            imageUrl = await SaveImageAsync(Image);
        }

        var equipment = new EquipmentModel
        {
            CompanyId = company.CompanyId,          // always from the logged-in user, never from the form
            CategoryId = Input.CategoryId,
            Name = Input.Name.Trim(),
            SerialOrAssetCode = Input.SerialOrAssetCode.Trim(),
            Description = Input.Description.Trim(),
            DailyRate = Input.DailyRate,
            SecurityDeposit = Input.SecurityDeposit,
            TotalQuantity = Input.TotalQuantity,
            AvailableQuantity = Input.TotalQuantity, // everything is free when first added
            Status = "Available",
            SpecificTerms = string.IsNullOrWhiteSpace(Input.SpecificTerms) ? null : Input.SpecificTerms.Trim(),
            ImageUrl = imageUrl
        };

        _context.Equipment.Add(equipment);
        await _context.SaveChangesAsync();

        TempData["Message"] = $"\"{equipment.Name}\" was added.";
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

        // Random name: avoids clashes and ignores whatever the user named the file
        var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
        var fullPath = Path.Combine(folder, fileName);

        await using var stream = System.IO.File.Create(fullPath);
        await file.CopyToAsync(stream);

        return $"/images/equipment/{fileName}";
    }
}
