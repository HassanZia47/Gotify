using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Goatify.Core.Models;
using Goatify.Infrastructure.Data;

namespace Goatify.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpenseController : ControllerBase
{
    private readonly GoatifyDbContext _context;
    private readonly ILogger<ExpenseController> _logger;

    public ExpenseController(GoatifyDbContext context, ILogger<ExpenseController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var expenses = await _context.Expenses
                .AsNoTracking()
                .OrderByDescending(x => x.Date)
                .ToListAsync();

            return Ok(expenses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching expenses");
            return StatusCode(500, "Something went wrong while fetching expenses.");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            var expense = await _context.Expenses.FindAsync(id);

            if (expense == null)
                return NotFound($"Expense with ID {id} not found.");

            return Ok(expense);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching expense {Id}", id);
            return StatusCode(500, "Something went wrong while fetching expense.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Expense expense)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = expense.Id }, expense);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense");
            return StatusCode(500, "Something went wrong while creating expense.");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Expense expense)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _context.Expenses.FindAsync(id);

            if (existing == null)
                return NotFound($"Expense with ID {id} not found.");

            existing.Name = expense.Name;
            existing.Quantity = expense.Quantity;
            existing.Unit = expense.Unit;
            existing.PricePerUnit = expense.PricePerUnit;
            existing.Date = expense.Date;
            existing.Notes = expense.Notes;
            existing.ModifiedDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating expense {Id}", id);
            return StatusCode(500, "Something went wrong while updating expense.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(x => x.Id == id);

            if (expense == null)
                return NotFound($"Expense with ID {id} not found.");

            if (expense.DeletedFlag)
                return BadRequest("Expense is already deleted.");

            expense.DeletedFlag = true;
            expense.DeletedDateTime = DateTime.UtcNow;   // ✅ THIS was missing

            await _context.SaveChangesAsync();

            return Ok("Expense deleted successfully (soft delete).");
        }
        catch (Exception ex)
        {
            _logger.LogInformation("Expense {Id} soft deleted", id);
            return StatusCode(500, "Error occurred while deleting expense.");
        }
    }

    [HttpPost("restore/{id}")]
    public async Task<IActionResult> Restore(int id)
    {
        try
        {
            var expense = await _context.Expenses
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (expense == null)
                return NotFound($"Expense with ID {id} not found.");

            if (!expense.DeletedFlag)
                return BadRequest("Expense is not deleted.");

            expense.DeletedFlag = false;
            expense.DeletedDateTime = null;
            expense.ModifiedDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Expense restored successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogInformation("Expense {Id} restored", id);
            return StatusCode(500, "Error occurred while restoring expense.");
        }
    }

    [HttpGet("deleted")]
    public async Task<IActionResult> GetDeleted()
    {
        try
        {
            var deletedExpenses = await _context.Expenses
                .IgnoreQueryFilters()
                .Where(x => x.DeletedFlag)
                .OrderByDescending(x => x.DeletedDateTime)
                .ToListAsync();

            return Ok(deletedExpenses);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error fetching deleted expenses.");
        }
    }

    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(int page = 1, int pageSize = 10, string? search = null)
    {
        try
        {
            var query = _context.Expenses.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Name.Contains(search));
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new PagedResult<Expense>
            {
                Items = data,
                TotalCount = total
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching paged expenses");
            return StatusCode(500, "Error loading expenses");
        }
    }

    [HttpGet("deleted-paged")]
    public async Task<IActionResult> GetDeletedPaged(int page = 1, int pageSize = 10, string? search = null)
    {
        var query = _context.Expenses
            .IgnoreQueryFilters()
            .Where(x => x.DeletedFlag);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Name.Contains(search));

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.DeletedDateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PagedResult<Expense>
        {
            Items = items,
            TotalCount = total
        });
    }
}