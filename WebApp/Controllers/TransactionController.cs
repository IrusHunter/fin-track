using FinTrack.Models;
using FinTrack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly ILogger<TransactionController> _logger;
        private readonly ITransactionService _transactionService;
        private readonly ICategoryService _categoryService;

        public TransactionController(
            ILogger<TransactionController> logger,
            ITransactionService transactionService,
            ICategoryService categoryService)
        {
            _logger = logger;
            _transactionService = transactionService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var transactions = await _transactionService.FindAll();
            return View(transactions);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _categoryService.FindAll(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Transaction transaction)
        {
            ViewBag.Categories = new SelectList(await _categoryService.FindAll(), "Id", "Name", transaction.CategoryId);

            // if (!ModelState.IsValid)
            //     return View(transaction);

            try
            {
                await _transactionService.Create(transaction);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(transaction);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
                return BadRequest("ID not specified");

            var transaction = await _transactionService.Find((int)id);
            if (transaction == null)
                return NotFound("Transaction not found");

            ViewBag.Categories = new SelectList(await _categoryService.FindAll(), "Id", "Name", transaction.CategoryId);

            return View(transaction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Transaction transaction)
        {
            ViewBag.Categories = new SelectList(await _categoryService.FindAll(), "Id", "Name", transaction.CategoryId);

            if (!ModelState.IsValid)
                return View(transaction);

            try
            {
                await _transactionService.Update(transaction);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(transaction);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _transactionService.Delete(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return RedirectToAction("Index");
        }
    }
}
