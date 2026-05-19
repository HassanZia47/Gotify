using Goatify.Core.Models;
using Goatify.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Goatify.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BillController : ControllerBase
{
    private readonly GoatifyDbContext _context;
    private readonly ILogger<BillController> _logger;

    public BillController(
        GoatifyDbContext context,
        ILogger<BillController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var bills = await _context.Bills
                .AsNoTracking()
                .OrderByDescending(x => x.BillDate)
                .ToListAsync();

            _logger.LogInformation(
                "Fetched {Count} bills successfully",
                bills.Count);

            return Ok(bills);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while fetching all bills");

            return StatusCode(
                500,
                "Something went wrong while fetching bills.");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            var bill = await _context.Bills
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (bill == null)
            {
                _logger.LogWarning(
                    "Bill with ID {BillId} was not found",
                    id);

                return NotFound(
                    $"Bill with ID {id} not found.");
            }

            _logger.LogInformation(
                "Bill with ID {BillId} fetched successfully",
                id);

            return Ok(bill);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while fetching bill with ID {BillId}",
                id);

            return StatusCode(
                500,
                "Something went wrong while fetching the bill.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Bill bill)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "Invalid bill create request received");

                return BadRequest(ModelState);
            }

            _context.Bills.Add(bill);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Bill created successfully with ID {BillId}",
                bill.Id);

            return CreatedAtAction(
                nameof(Get),
                new { id = bill.Id },
                bill);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while creating bill");

            return StatusCode(
                500,
                "Something went wrong while creating the bill.");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] Bill bill)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "Invalid bill update request received for ID {BillId}",
                    id);

                return BadRequest(ModelState);
            }

            var existing = await _context.Bills
                .FirstOrDefaultAsync(x => x.Id == id);

            if (existing == null)
            {
                _logger.LogWarning(
                    "Bill with ID {BillId} not found for update",
                    id);

                return NotFound(
                    $"Bill with ID {id} not found.");
            }

            existing.Name = bill.Name;
            existing.Amount = bill.Amount;
            existing.BillDate = bill.BillDate;
            existing.Notes = bill.Notes;
            existing.ModifiedDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Bill with ID {BillId} updated successfully",
                id);

            return Ok(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while updating bill with ID {BillId}",
                id);

            return StatusCode(
                500,
                "Something went wrong while updating the bill.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var bill = await _context.Bills
                .FirstOrDefaultAsync(x => x.Id == id);

            if (bill == null)
            {
                _logger.LogWarning(
                    "Bill with ID {BillId} not found for delete",
                    id);

                return NotFound(
                    $"Bill with ID {id} not found.");
            }

            if (bill.DeletedFlag)
            {
                _logger.LogWarning(
                    "Bill with ID {BillId} is already deleted",
                    id);

                return BadRequest(
                    "Bill is already deleted.");
            }

            bill.DeletedFlag = true;
            bill.DeletedDateTime = DateTime.UtcNow;
            bill.ModifiedDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Bill with ID {BillId} soft deleted successfully",
                id);

            return Ok(
                "Bill deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while deleting bill with ID {BillId}",
                id);

            return StatusCode(
                500,
                "Something went wrong while deleting the bill.");
        }
    }

    [HttpPost("restore/{id}")]
    public async Task<IActionResult> Restore(int id)
    {
        try
        {
            var bill = await _context.Bills
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (bill == null)
            {
                _logger.LogWarning(
                    "Bill with ID {BillId} not found for restore",
                    id);

                return NotFound(
                    $"Bill with ID {id} not found.");
            }

            if (!bill.DeletedFlag)
            {
                _logger.LogWarning(
                    "Bill with ID {BillId} is not deleted",
                    id);

                return BadRequest(
                    "Bill is not deleted.");
            }

            bill.DeletedFlag = false;
            bill.DeletedDateTime = null;
            bill.ModifiedDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Bill with ID {BillId} restored successfully",
                id);

            return Ok(
                "Bill restored successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while restoring bill with ID {BillId}",
                id);

            return StatusCode(
                500,
                "Something went wrong while restoring the bill.");
        }
    }

    [HttpGet("deleted")]
    public async Task<IActionResult> GetDeleted()
    {
        try
        {
            var deletedBills = await _context.Bills
                .IgnoreQueryFilters()
                .Where(x => x.DeletedFlag)
                .AsNoTracking()
                .OrderByDescending(x => x.DeletedDateTime)
                .ToListAsync();

            _logger.LogInformation(
                "Fetched {Count} deleted bills successfully",
                deletedBills.Count);

            return Ok(deletedBills);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while fetching deleted bills");

            return StatusCode(
                500,
                "Something went wrong while fetching deleted bills.");
        }
    }

    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        try
        {
            if (page <= 0)
                page = 1;

            if (pageSize <= 0)
                pageSize = 10;

            var query = _context.Bills.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.Name.Contains(search));
            }

            var total = await query.CountAsync();

            var items = await query
                .AsNoTracking()
                .OrderByDescending(x => x.BillDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation(
                "Fetched paged bills successfully. Page: {Page}, PageSize: {PageSize}, Total: {Total}",
                page,
                pageSize,
                total);

            return Ok(new PagedResult<Bill>
            {
                Items = items,
                TotalCount = total
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while fetching paged bills");

            return StatusCode(
                500,
                "Something went wrong while loading bills.");
        }
    }

    [HttpGet("deleted-paged")]
    public async Task<IActionResult> GetDeletedPaged(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        try
        {
            if (page <= 0)
                page = 1;

            if (pageSize <= 0)
                pageSize = 10;

            var query = _context.Bills
                .IgnoreQueryFilters()
                .Where(x => x.DeletedFlag);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.Name.Contains(search));
            }

            var total = await query.CountAsync();

            var items = await query
                .AsNoTracking()
                .OrderByDescending(x => x.DeletedDateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation(
                "Fetched deleted paged bills successfully. Page: {Page}, PageSize: {PageSize}, Total: {Total}",
                page,
                pageSize,
                total);

            return Ok(new PagedResult<Bill>
            {
                Items = items,
                TotalCount = total
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while fetching deleted paged bills");

            return StatusCode(
                500,
                "Something went wrong while loading deleted bills.");
        }
    }
}