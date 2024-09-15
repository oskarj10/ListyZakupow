using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingListApp.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;

    public HomeController(ApplicationDbContext context, IAuthorizationService authorizationService)
    {
        _context = context;
        _authorizationService = authorizationService;
    }

    // GET: /Home/Index
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var shoppingLists = await _context.ShoppingListItems
            .Where(s => s.OwnerId == userId)
            .ToListAsync();
        return View(shoppingLists ?? new List<ShoppingListItem>());
    }

    // GET: /Home/Create
    [Authorize]
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Home/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Create([Bind("ListName,Description,ShoppingDate,IsChecked")] ShoppingListItem shoppingListItem)
    {
        if (!ModelState.IsValid)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }
            return View(shoppingListItem);
        }

      
        if (shoppingListItem.ShoppingDate < DateTime.Today)
        {
            ModelState.AddModelError("ShoppingDate", "Data zakupów nie może być wsteczna.");
            return View(shoppingListItem);
        }

        try
        {
            shoppingListItem.OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(shoppingListItem.OwnerId))
            {
                ModelState.AddModelError("", "Nie znaleziono identyfikatora użytkownika.");
                return View(shoppingListItem);
            }

            _context.Add(shoppingListItem);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd podczas zapisywania: {ex.Message}");
            ModelState.AddModelError("", "Wystąpił problem podczas zapisywania listy.");
            return View(shoppingListItem);
        }
    }

    // GET: /Home/Edit/{id}
    [Authorize]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var shoppingListItem = await _context.ShoppingListItems
            .Include(s => s.Owner)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (shoppingListItem == null)
        {
            return NotFound();
        }

        var authorizationResult = await _authorizationService.AuthorizeAsync(User, shoppingListItem, "RequireOwner");
        if (!authorizationResult.Succeeded)
        {
            return Forbid();
        }

        return View(shoppingListItem);
    }

    // POST: /Home/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ListName,ShoppingDate,Description,IsChecked")] ShoppingListItem shoppingListItem)
    {
        if (id != shoppingListItem.Id)
        {
            return NotFound();
        }

        var authorizationResult = await _authorizationService.AuthorizeAsync(User, shoppingListItem, "RequireOwner");
        if (!authorizationResult.Succeeded)
        {
            return Forbid();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(shoppingListItem);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShoppingListItemExists(shoppingListItem.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction("Details", new { id = shoppingListItem.Id });
        }
        return View(shoppingListItem);
    }

    // GET: /Home/Details/{id}
    [Authorize]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var shoppingListItem = await _context.ShoppingListItems
            .Include(s => s.Owner)
            .Include(s => s.Products)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (shoppingListItem == null)
        {
            return NotFound();
        }

        var authorizationResult = await _authorizationService.AuthorizeAsync(User, shoppingListItem, "RequireOwner");
        if (!authorizationResult.Succeeded)
        {
            return Forbid();
        }

        return View(shoppingListItem);
    }

    // POST: /Home/ToggleStatus/{id}
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var product = await _context.ShoppingProducts.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        product.IsChecked = !product.IsChecked;
        await _context.SaveChangesAsync();

        // Sprawdź, czy wszystkie produkty w liście są skreślone
        var allProductsChecked = await _context.ShoppingProducts
            .Where(p => p.ShoppingListId == product.ShoppingListId)
            .AllAsync(p => p.IsChecked);

        if (allProductsChecked)
        {
            // Skreśl listę
            var shoppingList = await _context.ShoppingListItems
                .FindAsync(product.ShoppingListId);

            if (shoppingList != null && !shoppingList.IsChecked)
            {
                shoppingList.IsChecked = true;
                await _context.SaveChangesAsync();
            }
        }

        return Ok();
    }

    // POST: /Home/ToggleListStatus/{id}
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ToggleListStatus(int id)
    {
        var shoppingList = await _context.ShoppingListItems.FindAsync(id);
        if (shoppingList == null)
        {
            return NotFound();
        }

        // Jeśli lista jest już skreślona, nie pozwól na zmianę statusu
        if (shoppingList.IsChecked)
        {
            return BadRequest("Lista już została skreślona i nie można jej przywrócić.");
        }

        shoppingList.IsChecked = true;
        await _context.SaveChangesAsync();

        return Ok();  // Zwróć prostą odpowiedź HTTP 200 OK
    }


    // POST: /Home/DeleteProduct/{id}
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.ShoppingProducts.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        _context.ShoppingProducts.Remove(product);
        await _context.SaveChangesAsync();

        return Ok();
    }

    // POST: /Home/AddProduct
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddProduct(string name, int listId)
    {
        if (string.IsNullOrEmpty(name))
        {
            return BadRequest("Nazwa produktu nie może być pusta.");
        }

        var listExists = await _context.ShoppingListItems.AnyAsync(l => l.Id == listId);
        if (!listExists)
        {
            return BadRequest("Listy zakupów o podanym ID nie znaleziono.");
        }

        var newProduct = new ShoppingProduct
        {
            ProductName = name,
            ShoppingListId = listId,
            IsChecked = false,
            OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
        };

        _context.ShoppingProducts.Add(newProduct);
        await _context.SaveChangesAsync();

        return Json(newProduct);
    }

    private bool ShoppingListItemExists(int id)
    {
        return _context.ShoppingListItems.Any(e => e.Id == id);
    }
}
