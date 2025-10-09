using System.Diagnostics;
using System.Threading.Tasks;
using FinTrack.Models;
using FinTrack.Services;
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

    public async Task<IActionResult> Index()
    {
        var categories = await _categoryService.FindAll();
        return View(categories);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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
    public IActionResult Update(int? id)
    {
        if (id == null)
        {
            return BadRequest("ID not specified");
        }


        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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
