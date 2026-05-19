using Goatify.Core.Models;
using Goatify.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Goatify.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicationController : ControllerBase
{
    private readonly GoatifyDbContext _context;
    private readonly ILogger<MedicationController> _logger;

    public MedicationController(
        GoatifyDbContext context,
        ILogger<MedicationController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // =========================
    // GET ALL
    // =========================
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var meds = await _context.Medications
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync();

            _logger.LogInformation(
                "Fetched {Count} medications successfully",
                meds.Count);

            return Ok(meds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while fetching all medications");

            return StatusCode(
                500,
                "Something went wrong while fetching medications.");
        }
    }

    // =========================
    // GET BY ID
    // =========================
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            var med = await _context.Medications
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (med == null)
            {
                _logger.LogWarning(
                    "Medication with ID {MedicationId} was not found",
                    id);

                return NotFound(
                    $"Medication with ID {id} not found.");
            }

            _logger.LogInformation(
                "Medication with ID {MedicationId} fetched successfully",
                id);

            return Ok(med);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while fetching medication with ID {MedicationId}",
                id);

            return StatusCode(
                500,
                "Something went wrong while fetching medication.");
        }
    }

    // =========================
    // CREATE
    // =========================
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Medication medication)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "Invalid medication create request received");

                return BadRequest(ModelState);
            }

            _context.Medications.Add(medication);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Medication created successfully with ID {MedicationId}",
                medication.Id);

            return CreatedAtAction(
                nameof(Get),
                new { id = medication.Id },
                medication);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while creating medication");

            return StatusCode(
                500,
                "Something went wrong while creating medication.");
        }
    }

    // =========================
    // UPDATE
    // =========================
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Medication medication)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "Invalid medication update request received for ID {MedicationId}",
                    id);

                return BadRequest(ModelState);
            }

            var existing = await _context.Medications
                .FirstOrDefaultAsync(x => x.Id == id);

            if (existing == null)
            {
                _logger.LogWarning(
                    "Medication with ID {MedicationId} not found for update",
                    id);

                return NotFound(
                    $"Medication with ID {id} not found.");
            }

            existing.Name = medication.Name;
            existing.Type = medication.Type;
            existing.Notes = medication.Notes;
            existing.ModifiedDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Medication with ID {MedicationId} updated successfully",
                id);

            return Ok(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while updating medication with ID {MedicationId}",
                id);

            return StatusCode(
                500,
                "Something went wrong while updating medication.");
        }
    }

    // =========================
    // DELETE (SOFT DELETE)
    // =========================
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var med = await _context.Medications
                .FirstOrDefaultAsync(x => x.Id == id);

            if (med == null)
            {
                _logger.LogWarning(
                    "Medication with ID {MedicationId} not found for delete",
                    id);

                return NotFound(
                    $"Medication with ID {id} not found.");
            }

            if (med.DeletedFlag)
            {
                _logger.LogWarning(
                    "Medication with ID {MedicationId} is already deleted",
                    id);

                return BadRequest(
                    "Medication is already deleted.");
            }

            med.DeletedFlag = true;
            med.DeletedDateTime = DateTime.UtcNow;
            med.ModifiedDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Medication with ID {MedicationId} soft deleted successfully",
                id);

            return Ok("Medication deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while deleting medication with ID {MedicationId}",
                id);

            return StatusCode(
                500,
                "Something went wrong while deleting medication.");
        }
    }

    // =========================
    // RESTORE
    // =========================
    [HttpPost("restore/{id}")]
    public async Task<IActionResult> Restore(int id)
    {
        try
        {
            var med = await _context.Medications
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (med == null)
            {
                _logger.LogWarning(
                    "Medication with ID {MedicationId} not found for restore",
                    id);

                return NotFound(
                    $"Medication with ID {id} not found.");
            }

            if (!med.DeletedFlag)
            {
                _logger.LogWarning(
                    "Medication with ID {MedicationId} is not deleted",
                    id);

                return BadRequest(
                    "Medication is not deleted.");
            }

            med.DeletedFlag = false;
            med.DeletedDateTime = null;
            med.ModifiedDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Medication with ID {MedicationId} restored successfully",
                id);

            return Ok("Medication restored successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while restoring medication with ID {MedicationId}",
                id);

            return StatusCode(
                500,
                "Something went wrong while restoring medication.");
        }
    }

    // =========================
    // DELETED LIST
    // =========================
    [HttpGet("deleted")]
    public async Task<IActionResult> GetDeleted()
    {
        try
        {
            var meds = await _context.Medications
                .IgnoreQueryFilters()
                .Where(x => x.DeletedFlag)
                .AsNoTracking()
                .OrderByDescending(x => x.DeletedDateTime)
                .ToListAsync();

            _logger.LogInformation(
                "Fetched {Count} deleted medications successfully",
                meds.Count);

            return Ok(meds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while fetching deleted medications");

            return StatusCode(
                500,
                "Something went wrong while fetching deleted medications.");
        }
    }

    // =========================
    // PAGED LIST
    // =========================
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        try
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _context.Medications.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.Name.Contains(search) ||
                    x.Type.Contains(search));
            }

            var total = await query.CountAsync();

            var items = await query
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation(
                "Fetched paged medications successfully. Page: {Page}, PageSize: {PageSize}, Total: {Total}",
                page,
                pageSize,
                total);

            return Ok(new PagedResult<Medication>
            {
                Items = items,
                TotalCount = total
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while fetching paged medications");

            return StatusCode(
                500,
                "Something went wrong while loading medications.");
        }
    }

    // =========================
    // DELETED PAGED
    // =========================
    [HttpGet("deleted-paged")]
    public async Task<IActionResult> GetDeletedPaged(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        try
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _context.Medications
                .IgnoreQueryFilters()
                .Where(x => x.DeletedFlag);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.Name.Contains(search) ||
                    x.Type.Contains(search));
            }

            var total = await query.CountAsync();

            var items = await query
                .AsNoTracking()
                .OrderByDescending(x => x.DeletedDateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation(
                "Fetched deleted paged medications successfully. Page: {Page}, PageSize: {PageSize}, Total: {Total}",
                page,
                pageSize,
                total);

            return Ok(new PagedResult<Medication>
            {
                Items = items,
                TotalCount = total
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while fetching deleted paged medications");

            return StatusCode(
                500,
                "Something went wrong while loading deleted medications.");
        }
    }

    [HttpPost("{id}/assign-goats")]
    public async Task<IActionResult> AssignGoats(int id, [FromBody] List<int> goatIds)
    {
        try
        {
            if (goatIds == null || !goatIds.Any())
                return BadRequest("No goats selected.");

            var medication = await _context.Medications
                .Include(x => x.GoatMedications)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (medication == null)
            {
                _logger.LogWarning("Medication {MedicationId} not found", id);
                return NotFound("Medication not found.");
            }

            // existing goat ids already linked
            var existingGoatIds = await _context.GoatMedications
                .Where(x => x.MedicationId == id)
                .Select(x => x.GoatId)
                .ToListAsync();

            // filter only NEW goats
            var newGoats = goatIds
                .Where(gid => !existingGoatIds.Contains(gid))
                .ToList();

            if (!newGoats.Any())
            {
                _logger.LogInformation(
                    "No new goats to add for medication {MedicationId}",
                    id);

                return Ok("No new goats to add.");
            }

            foreach (var goatId in newGoats)
            {
                _context.GoatMedications.Add(new GoatMedication
                {
                    GoatId = goatId,
                    MedicationId = id,
                    AssignedDate = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Added {Count} new goats to medication {MedicationId}",
                newGoats.Count,
                id);

            return Ok(new
            {
                message = "New goats linked successfully",
                added = newGoats.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error assigning goats to medication {MedicationId}",
                id);

            return StatusCode(500,
                "Something went wrong while assigning goats.");
        }
    }

    [HttpGet("{id}/goats")]
    public async Task<IActionResult> GetMedicationGoats(int id)
    {
        try
        {
            var goatIds = await _context.GoatMedications
                .Where(x => x.MedicationId == id)
                .Select(x => x.GoatId)
                .ToListAsync();

            var goats = await _context.Goats
                .AsNoTracking()
                .Where(x => goatIds.Contains(x.Id))
                .ToListAsync();

            return Ok(goats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching goats for medication {Id}", id);
            return StatusCode(500, "Error loading linked goats");
        }
    }

    [HttpGet("{id}/goats-paged")]
    public async Task<IActionResult> GetMedicationGoatsPaged(
    int id,
    int page = 1,
    int pageSize = 10,
    string? search = null)
    {
        try
        {
            var query = _context.GoatMedications
                .Where(x => x.MedicationId == id)
                .Join(_context.Goats,
                    gm => gm.GoatId,
                    g => g.Id,
                    (gm, g) => new { gm, g });

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.g.Notes.Contains(search) ||
                    x.g.Id.ToString().Contains(search));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.gm.AssignedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => x.g)
                .AsNoTracking()
                .ToListAsync();

            return Ok(new PagedResult<Goat>
            {
                Items = items,
                TotalCount = total
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading medication goats paged");
            return StatusCode(500, "Error loading linked goats");
        }
    }

    [HttpDelete("{id}/goat/{goatId}")]
    public async Task<IActionResult> RemoveGoat(int id, int goatId)
    {
        try
        {
            var link = await _context.GoatMedications
                .FirstOrDefaultAsync(x =>
                    x.MedicationId == id &&
                    x.GoatId == goatId);

            if (link == null)
            {
                _logger.LogWarning(
                    "Goat link not found for Medication {MedicationId}, Goat {GoatId}",
                    id, goatId);

                return NotFound("Goat link not found.");
            }

            _context.GoatMedications.Remove(link);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Goat {GoatId} removed from Medication {MedicationId}",
                goatId, id);

            return Ok("Goat removed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error removing goat {GoatId} from medication {MedicationId}",
                goatId, id);

            return StatusCode(500,
                "Something went wrong while removing goat.");
        }
    }

    // =========================
    // GET ALL GOAT MEDICATION
    // =========================
    [HttpGet("GetAllGoatMedication")]
    public async Task<IActionResult> GetAllGoatMedication()
    {
        try
        {
            var meds = await _context.GoatMedications
                .AsNoTracking()
                .OrderBy(x => x.AssignedDate)
                .ToListAsync();

            _logger.LogInformation(
                "Fetched {Count} goat medications successfully",
                meds.Count);

            return Ok(meds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while fetching all goat medications");

            return StatusCode(
                500,
                "Something went wrong while fetching goat medications.");
        }
    }
}