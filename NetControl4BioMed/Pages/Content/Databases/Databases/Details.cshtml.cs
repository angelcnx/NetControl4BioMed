using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Pages.Content.Databases.Databases
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public DetailsModel(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public Database Database { get; set; }

            public bool IsGeneric { get; set; }

            public IQueryable<DatabaseNodeField> DatabaseNodeFields { get; set; }

            public IQueryable<DatabaseEdgeField> DatabaseEdgeFields { get; set; }

            public IQueryable<NodeCollectionDatabase> NodeCollectionDatabases { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if the user does not exist.
            if (user == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: An error occured while trying to load the user data. If you are already logged in, please log out and try again.";
                // Redirect to the home page.
                return RedirectToPage("/Index");
            }
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Databases/Databases/Index");
            }
            // Get the item with the provided ID.
            var items = _context.Databases
                .Where(item => item.IsPublic || item.DatabaseUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == id);
            // Check if there was no item found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Databases/Databases/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Database = items
                    .Include(item => item.DatabaseType)
                    .First(),
                IsGeneric = items
                    .Any(item => item.DatabaseType.Name == "Generic"),
                DatabaseNodeFields = items
                    .Select(item => item.DatabaseNodeFields)
                    .SelectMany(item => item),
                DatabaseEdgeFields = items
                    .Select(item => item.DatabaseEdgeFields)
                    .SelectMany(item => item),
                NodeCollectionDatabases = items
                    .Select(item => item.NodeCollectionDatabases)
                    .SelectMany(item => item)
            };
            // Return the page.
            return Page();
        }
    }
}
