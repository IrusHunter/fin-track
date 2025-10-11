using System.Diagnostics;
using System.Threading.Tasks;
using FinTrack.Models;
using FinTrack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

namespace WebApp.Controllers;

public class CategoryController : Controller
{
    private readonly ILogger<CategoryController> _logger;
    private readonly ICategoryService _categoryService;

    public CategoryController(ILogger<CategoryController> logger, ICategoryService categoryService)
    {
        _logger = logger;
        _categoryService = categoryService;
    }

    [Authorize]
    // [Authorize(Policy = "IsUser")]
    public async Task<IActionResult> Index()
    {
        var categories = await _categoryService.FindAll();
        return View(categories);
    }

    [HttpGet]
    [Authorize(Policy = "IsAdmin")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "IsAdmin")]
    public async Task<IActionResult> Create(Category category)
    {
        if (!ModelState.IsValid)
        {
            return View(category);
        }

        try
        {
            await _categoryService.Create(category);
        }
        catch (Exception e)
        {
            ModelState.AddModelError("", e.Message);
            return View(category);
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    [Authorize(Policy = "IsAdmin")]
    public async Task<IActionResult> Update(int? id)
    {
        if (id == null)
        {
            return BadRequest("ID not specified");
        }

        var category = await _categoryService.Find((int)id);
        if (category == null)
        {
            return NotFound("Category not found");
        }

        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "IsAdmin")]
    public async Task<IActionResult> Update(Category category)
    {
        if (!ModelState.IsValid)
        {
            return View(category);
        }

        try
        {
            await _categoryService.Update(category);
        }
        catch (Exception e)
        {
            ModelState.AddModelError("", e.Message);
            return View(category);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "IsAdmin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return BadRequest("ID not specified");
        }

        try
        {
            await _categoryService.Delete((int)id);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }

        return RedirectToAction("Index");
    }
}
