using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ShoppingListApp.Data;
using System.Security.Claims;
using System.Threading.Tasks;

public class ShoppingListItemAuthorizationHandler : AuthorizationHandler<OwnershipRequirement, ShoppingListItem>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ShoppingListItemAuthorizationHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OwnershipRequirement requirement, ShoppingListItem resource)
    {
        if (context.User == null || resource == null)
        {
          
            return Task.CompletedTask;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        
        Console.WriteLine($"User ID: {userId}, Resource Owner ID: {resource.OwnerId}");

    
        if (resource.OwnerId == userId)
        {
            context.Succeed(requirement);  
        }
        else
        {
            context.Fail();  
        }

        return Task.CompletedTask;
    }
}

