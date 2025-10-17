using FinTrack.Models;
using FinTrack.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers.Api;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class TransactionApiController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionApiController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetAll()
    {
        var transactions = await _transactionService.FindAll();
        return Ok(transactions);
    }

    [HttpGet("{id:int}")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<Transaction>> Get(int id)
    {
        var transaction = await _transactionService.Find(id);
        if (transaction == null) return NotFound();
        return Ok(transaction);
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<Transaction>> Create(Transaction transaction)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _transactionService.Create(transaction);
        return CreatedAtAction(nameof(Get), new { id = transaction.Id, version = "1.0" }, transaction);
    }

    [HttpPut("{id:int}")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Update(int id, Transaction transaction)
    {
        if (id != transaction.Id) return BadRequest("ID mismatch");
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existing = await _transactionService.Find(id);
        if (existing == null) return NotFound();

        await _transactionService.Update(transaction);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _transactionService.Find(id);
        if (existing == null) return NotFound();

        await _transactionService.Delete(id);
        return NoContent();
    }
}
