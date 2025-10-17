using FinTrack.Models;
using FinTrack.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers.Api;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class CategoryApiController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryApiController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<IEnumerable<Category>>> GetAllV1()
    {
        var categories = await _categoryService.FindAll();
        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<Category>> GetV1(int id)
    {
        var category = await _categoryService.Find(id);
        if (category == null) return NotFound();
        return Ok(category);
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<Category>> CreateV1(Category category)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        await _categoryService.Create(category);
        return CreatedAtAction(nameof(GetV1), new { id = category.Id, version = "1.0" }, category);
    }

    [HttpPut("{id:int}")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> UpdateV1(int id, Category category)
    {
        if (id != category.Id) return BadRequest("ID mismatch");
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existing = await _categoryService.Find(id);
        if (existing == null) return NotFound();

        await _categoryService.Update(category);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> DeleteV1(int id)
    {
        var existing = await _categoryService.Find(id);
        if (existing == null) return NotFound();

        await _categoryService.Delete(id);
        return NoContent();
    }

    [HttpGet]
    [MapToApiVersion("2.0")]
    public async Task<ActionResult<IEnumerable<object>>> GetAllV2()
    {
        var categories = await _categoryService.FindAll();
        var result = categories.Select(c => new
        {
            c.Id,
            c.Name,
            c.TaxType
        });
        return Ok(result);
    }
}
