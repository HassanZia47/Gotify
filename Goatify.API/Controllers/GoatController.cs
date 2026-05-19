using Goatify.Core.Enums;
using Goatify.Core.Models;
using Goatify.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Goatify.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GoatController : ControllerBase
{
    private readonly GoatifyDbContext _context;
    private readonly ILogger<GoatController> _logger;

    public GoatController(
        GoatifyDbContext context,
        ILogger<GoatController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET ALL
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var goats = await _context.Goats
                .AsNoTracking()
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return Ok(goats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching goats");

            return StatusCode(500,
                "Something went wrong while fetching goats.");
        }
    }

    // GET BY ID
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            var goat = await _context.Goats
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (goat == null)
                return NotFound($"Goat with ID {id} not found.");

            return Ok(goat);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error fetching goat {Id}", id);

            return StatusCode(500,
                "Something went wrong while fetching goat.");
        }
    }

    // CREATE
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Goat goat)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (goat.PurchasePrice <= 0)
                return BadRequest("Purchase price must be greater than zero.");

            if (goat.Status == Core.Enums.GoatStatus.Sold
                && !goat.SalePrice.HasValue)
                return BadRequest("Please enter Sale price.");

            if (goat.SalePrice.HasValue && goat.SalePrice.Value <= 0)
                return BadRequest("Sale price must be greater than zero.");

            if (goat.Status == Core.Enums.GoatStatus.Sold
                && !goat.SalePrice.HasValue)
                return BadRequest("Please enter Sale date.");


            goat.CreatedDateTime = DateTime.UtcNow;

            _context.Goats.Add(goat);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Goat created successfully with ID {Id}",
                goat.Id);

            return CreatedAtAction(
                nameof(Get),
                new { id = goat.Id },
                goat);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating goat");

            return StatusCode(500,
                "Something went wrong while creating goat.");
        }
    }

    // UPDATE
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Goat updated)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var goat = await _context.Goats
                .FirstOrDefaultAsync(x => x.Id == id);

            if (goat == null)
                return NotFound(
                    $"Goat with ID {id} not found.");

            if (updated.PurchasePrice <= 0)
                return BadRequest("Purchase price must be greater than zero.");

            if (updated.Status == Core.Enums.GoatStatus.Sold
                && !updated.SalePrice.HasValue)
                return BadRequest("Please enter Sale price.");

            if (updated.SalePrice.HasValue && updated.SalePrice.Value <= 0)
                return BadRequest("Sale price must be greater than zero.");

            if (updated.Status == Core.Enums.GoatStatus.Sold
                && !updated.SalePrice.HasValue)
                return BadRequest("Please enter Sale date.");

            goat.Gender = updated.Gender;
            goat.Status = updated.Status;
            goat.PurchasePrice = updated.PurchasePrice;
            goat.SalePrice = updated.SalePrice;
            goat.PurchaseDate = updated.PurchaseDate;
            goat.SellDate = updated.SellDate;
            goat.Notes = updated.Notes;
            goat.ImageUrl = updated.ImageUrl;

            goat.ModifiedDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Goat {Id} updated successfully",
                id);

            return Ok(goat);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating goat {Id}",
                id);

            return StatusCode(500,
                "Something went wrong while updating goat.");
        }
    }

    // SOFT DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var goat = await _context.Goats
                .FirstOrDefaultAsync(x => x.Id == id);

            if (goat == null)
                return NotFound(
                    $"Goat with ID {id} not found.");

            if (goat.DeletedFlag)
                return BadRequest(
                    "Goat is already deleted.");

            goat.DeletedFlag = true;
            goat.DeletedDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Goat {Id} soft deleted",
                id);

            return Ok(
                "Goat deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting goat {Id}",
                id);

            return StatusCode(500,
                "Something went wrong while deleting goat.");
        }
    }

    // RESTORE
    [HttpPost("restore/{id}")]
    public async Task<IActionResult> Restore(int id)
    {
        try
        {
            var goat = await _context.Goats
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (goat == null)
                return NotFound(
                    $"Goat with ID {id} not found.");

            if (!goat.DeletedFlag)
                return BadRequest(
                    "Goat is not deleted.");

            goat.DeletedFlag = false;
            goat.DeletedDateTime = null;
            goat.ModifiedDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Goat {Id} restored",
                id);

            return Ok("Goat restored successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error restoring goat {Id}",
                id);

            return StatusCode(500,
                "Something went wrong while restoring goat.");
        }
    }

    // GET DELETED
    [HttpGet("deleted")]
    public async Task<IActionResult> GetDeleted()
    {
        try
        {
            var goats = await _context.Goats
                .IgnoreQueryFilters()
                .Where(x => x.DeletedFlag)
                .OrderByDescending(x => x.DeletedDateTime)
                .ToListAsync();

            return Ok(goats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error fetching deleted goats");

            return StatusCode(500,
                "Error fetching deleted goats.");
        }
    }

    // PAGINATION + SEARCH
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(int page = 1, int pageSize = 10, string? search = null, GoatStatus? status = null)
    {
        try
        {
            var query = _context.Goats.AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();

                query = query.Where(x => (x.Notes != null && x.Notes.ToLower().Contains(term)));
            }

            // STATUS FILTER
            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            var total = await query.CountAsync();

            var goats = await query
                .OrderByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new PagedResult<Goat>
            {
                Items = goats,
                TotalCount = total
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching paged goats");

            return StatusCode(
                500,
                "Error loading goats.");
        }
    }

    // DELETED PAGINATION
    [HttpGet("deleted-paged")]
    public async Task<IActionResult> GetDeletedPaged(int page = 1, int pageSize = 10, string? search = null, GoatStatus? status = null)
    {
        try
        {
            var query = _context.Goats.IgnoreQueryFilters().Where(x => x.DeletedFlag);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(x => (x.Notes != null && x.Notes.ToLower().Contains(term)));
            }

            // STATUS FILTER
            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.DeletedDateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new PagedResult<Goat>
            {
                Items = items,
                TotalCount = total
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error fetching deleted goats");

            return StatusCode(500,
                "Error loading deleted goats.");
        }
    }

    // =========================
    // GET MEDICATIONS (PAGED PER GOAT)
    // =========================
    [HttpGet("{id}/medications-paged")]
    public async Task<IActionResult> GetGoatMedicationsPaged(
        int id,
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        try
        {
            var query = _context.GoatMedications
                .Where(x => x.GoatId == id)
                .Include(x => x.Medication)
                .Select(x => x.Medication!)
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();

                query = query.Where(x =>
                    (x.Name != null && x.Name.ToLower().Contains(term)) ||
                    (x.Type != null && x.Type.ToLower().Contains(term)) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(term)));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return Ok(new PagedResult<Medication>
            {
                Items = items,
                TotalCount = total
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error fetching medications for goat {Id}", id);

            return StatusCode(500,
                "Error loading medications.");
        }
    }

    // =========================
    // REMOVE MEDICATION FROM GOAT
    // =========================
    [HttpDelete("{id}/medication/{medicationId}")]
    public async Task<IActionResult> RemoveMedicationFromGoat(
        int id,
        int medicationId)
    {
        try
        {
            var link = await _context.GoatMedications
                .FirstOrDefaultAsync(x =>
                    x.GoatId == id &&
                    x.MedicationId == medicationId);

            if (link == null)
                return NotFound("Link not found.");

            _context.GoatMedications.Remove(link);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Removed medication {MedicationId} from goat {GoatId}",
                medicationId, id);

            return Ok("Medication removed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error removing medication from goat {Id}", id);

            return StatusCode(500,
                "Error removing medication.");
        }
    }
}