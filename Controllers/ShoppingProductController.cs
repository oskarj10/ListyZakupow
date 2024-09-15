using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingListApp.Data;
using System.Linq;
using System.Security.Claims;

namespace ShoppingListApp.Controllers
{
    public class ShoppingProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService _authorizationService;

        public ShoppingProductController(ApplicationDbContext context, IAuthorizationService authorizationService)
        {
            _context = context;
            _authorizationService = authorizationService;
        }

        // GET: /ShoppingProduct/Create/{listId}
        [Authorize]
        public IActionResult Create(int listId)
        {
            ViewData["ListId"] = listId;
            return View();
        }

        // POST: /ShoppingProduct/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Create([Bind("ProductName, ShoppingListId")] ShoppingProduct shoppingProduct)
        {
            var list = _context.ShoppingListItems.Find(shoppingProduct.ShoppingListId);
            if (list == null || list.Owner.Id != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                shoppingProduct.Owner = list.Owner;
                _context.Add(shoppingProduct);
                _context.SaveChanges();
                return RedirectToAction("Details", "Home", new { id = shoppingProduct.ShoppingListId });
            }
            return View(shoppingProduct);
        }

        // POST: /ShoppingProduct/ToggleProduct/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult ToggleProduct(int id)
        {
            var product = _context.ShoppingProducts.Find(id);
            if (product == null || product.Owner.Id != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Forbid();
            }

            product.IsChecked = !product.IsChecked;
            _context.SaveChanges();
            return RedirectToAction("Details", "Home", new { id = product.ShoppingListId });
        }

        // POST: /ShoppingProduct/DeleteProduct/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.ShoppingProducts.Find(id);
            if (product == null || product.Owner.Id != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Forbid();
            }

            var listId = product.ShoppingListId;
            _context.ShoppingProducts.Remove(product);
            _context.SaveChanges();
            return RedirectToAction("Details", "Home", new { id = listId });
        }
    }
}

