using Goatify.Core.Enums;
using Goatify.Core.Models;
using Goatify.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class BreedingController : ControllerBase
{
    private readonly GoatifyDbContext _context;
    private readonly ILogger<BreedingController> _logger;

    public BreedingController(GoatifyDbContext context, ILogger<BreedingController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var meds = await _context.Breedings
                .AsNoTracking()
                .OrderBy(x => x.BreedingDate)
                .ToListAsync();

            _logger.LogInformation(
                "Fetched {Count} Breedings successfully",
                meds.Count);

            return Ok(meds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while fetching all Breedings");

            return StatusCode(
                500,
                "Something went wrong while fetching Breedings.");
        }
    }


    #region GET PAGED
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(int page = 1, int pageSize = 10, string? search = null)
    {
        try
        {
            var query = _context.Breedings
                .Where(x => !x.DeletedFlag);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.Notes.Contains(search));
            }

            var total = await query.CountAsync();

            var items = await query
                .AsNoTracking()
                .OrderByDescending(x => x.BreedingDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new PagedResult<Breeding>
            {
                Items = items,
                TotalCount = total
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching breeding paged");
            return StatusCode(500, "Error loading breeding records.");
        }
    }
    #endregion

    #region GET SINGLE
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            var item = await _context.Breedings
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
                return NotFound($"Breeding {id} not found");

            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching breeding {Id}", id);
            return StatusCode(500, "Error loading record.");
        }
    }
    #endregion

    #region CREATE
    [HttpPost]
    public async Task<IActionResult> Create(Breeding model)
    {
        try
        {
            _context.Breedings.Add(model);
            await _context.SaveChangesAsync();
            return Ok(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating breeding");
            return StatusCode(500, "Error creating breeding.");
        }
    }
    #endregion

    #region UPDATE
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Breeding model)
    {
        try
        {
            var existing = await _context.Breedings.FirstOrDefaultAsync(x => x.Id == id);

            if (existing == null)
                return NotFound();

            existing.MaleGoatId = model.MaleGoatId;
            existing.FemaleGoatId = model.FemaleGoatId;
            existing.BreedingDate = model.BreedingDate;
            existing.ExpectedDeliveryDate = model.ExpectedDeliveryDate;
            existing.Notes = model.Notes;
            existing.ModifiedDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating breeding");
            return StatusCode(500, "Error updating record.");
        }
    }
    #endregion

    #region DELETE (SOFT)
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var item = await _context.Breedings.FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
                return NotFound();

            item.DeletedFlag = true;
            item.DeletedDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting breeding");
            return StatusCode(500, "Error deleting record.");
        }
    }
    #endregion

    #region RESTORE
    [HttpPost("restore/{id}")]
    public async Task<IActionResult> Restore(int id)
    {
        try
        {
            var item = await _context.Breedings
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
                return NotFound();

            item.DeletedFlag = false;
            item.DeletedDateTime = null;

            await _context.SaveChangesAsync();

            return Ok("Restored");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring breeding");
            return StatusCode(500, "Error restoring record.");
        }
    }
    #endregion

    #region DELETED PAGED
    [HttpGet("deleted-paged")]
    public async Task<IActionResult> DeletedPaged(int page = 1, int pageSize = 10, string? search = null)
    {
        try
        {
            var query = _context.Breedings
                .IgnoreQueryFilters()
                .Where(x => x.DeletedFlag);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(x => x.Notes.Contains(search));

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.DeletedDateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new PagedResult<Breeding>
            {
                Items = items,
                TotalCount = total
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading deleted breeding");
            return StatusCode(500, "Error loading deleted records.");
        }
    }
    #endregion

    [HttpGet("search")]
    public async Task<IActionResult> Search(
    string? query,
    string? gender,
    int page = 1,
    int pageSize = 20)
    {
        try
        {
            var q = _context.Goats
                .AsNoTracking()
                .Where(x => !x.DeletedFlag);

            // gender filter
            if (!string.IsNullOrWhiteSpace(gender))
            {
                if (gender.ToLower() == "male")
                    q = q.Where(x => x.Gender == GoatGender.Male);

                if (gender.ToLower() == "female")
                    q = q.Where(x => x.Gender == GoatGender.Female);
            }

            // search filter
            if (!string.IsNullOrWhiteSpace(query))
            {
                q = q.Where(x =>
                    x.Id.ToString().Contains(query) ||
                    x.Notes!.Contains(query));
            }

            var total = await q.CountAsync();

            var items = await q
                .OrderByDescending(x => x.Id)
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
            _logger.LogError(ex, "Error searching goats");
            return StatusCode(500, "Error searching goats.");
        }
    }

    [HttpGet("{id}/breeding-paged")]
    public async Task<IActionResult> GetBreedingPaged(
    int id,
    int page = 1,
    int pageSize = 10,
    string? search = null)
    {
        try
        {
            var query = _context.Breedings
                .AsNoTracking()
                .Where(x =>
                    !x.DeletedFlag &&
                    (x.MaleGoatId == id || x.FemaleGoatId == id));

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.Notes != null &&
                    x.Notes.Contains(search));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.BreedingDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new PagedResult<Breeding>
            {
                Items = items,
                TotalCount = total
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error loading breeding history for goat {GoatId}",
                id);

            return StatusCode(
                500,
                "Error loading breeding history.");
        }
    }
}