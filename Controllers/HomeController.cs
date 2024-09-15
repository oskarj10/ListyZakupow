using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingListApp.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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
            .Where(s => s.Owner.Id == userId)
            .ToListAsync();
        return View(shoppingLists);
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
    public async Task<IActionResult> Create([Bind("ListName,ShoppingDate,Description")] ShoppingListItem shoppingListItem)
    {
        if (ModelState.IsValid)
        {
            shoppingListItem.Owner = await _context.Users.FindAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _context.Add(shoppingListItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(shoppingListItem);
    }

    // GET: /Home/Edit/{id}
    [Authorize]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var shoppingListItem = await _context.ShoppingListItems.FindAsync(id);
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
    public async Task<IActionResult> Edit(int id, [Bind("Id,ListName,ShoppingDate,Description")] ShoppingListItem shoppingListItem)
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
            return RedirectToAction(nameof(Index));
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

    // GET: /Home/Delete/{id}
    [Authorize]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var shoppingListItem = await _context.ShoppingListItems.FindAsync(id);
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

    // POST: /Home/Delete/{id}
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var shoppingListItem = await _context.ShoppingListItems.FindAsync(id);
        var authorizationResult = await _authorizationService.AuthorizeAsync(User, shoppingListItem, "RequireOwner");
        if (!authorizationResult.Succeeded)
        {
            return Forbid();
        }

        if (shoppingListItem != null)
        {
            _context.ShoppingListItems.Remove(shoppingListItem);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private bool ShoppingListItemExists(int id)
    {
        return _context.ShoppingListItems.Any(e => e.Id == id);
    }
}

