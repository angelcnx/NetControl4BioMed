using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.ViewModels;

namespace NetControl4BioMed.Pages.Content.Databases.DatabaseEdgeFields
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly LinkGenerator _linkGenerator;

        public IndexModel(UserManager<User> userManager, ApplicationDbContext context, LinkGenerator linkGenerator)
        {
            _userManager = userManager;
            _context = context;
            _linkGenerator = linkGenerator;
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public SearchViewModel<DatabaseEdgeField> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "Name", "Name" },
                    { "Description", "Description" },
                    { "DatabaseId", "Database ID" },
                    { "DatabaseName", "Database name" },
                    { "DatabaseEdgeFieldEdges", "Edges" }
                },
                Filter = new Dictionary<string, string>
                {
                },
                SortBy = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "Name", "Name" },
                    { "DatabaseId", "Database ID" },
                    { "DatabaseName", "Database name" },
                    { "DatabaseEdgeFieldEdgeCount", "Number of edges" }
                }
            };
        }

        public async Task<IActionResult> OnGetAsync(string searchString = null, IEnumerable<string> searchIn = null, IEnumerable<string> filter = null, string sortBy = null, string sortDirection = null, int? itemsPerPage = null, int? currentPage = 1)
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
            // Define the search input.
            var input = new SearchInputViewModel(ViewModel.SearchOptions, null, searchString, searchIn, filter, sortBy, sortDirection, itemsPerPage, currentPage);
            // Check if any of the provided variables was null before the reassignment.
            if (input.NeedsRedirect)
            {
                // Redirect to the page where they are all explicitly defined.
                return RedirectToPage(new { searchString = input.SearchString, searchIn = input.SearchIn, filter = input.Filter, sortBy = input.SortBy, sortDirection = input.SortDirection, itemsPerPage = input.ItemsPerPage, currentPage = input.CurrentPage });
            }
            // Start with all of the items to which the user has access.
            var query = _context.DatabaseEdgeFields
                .Where(item => item.Database.IsPublic || item.Database.DatabaseUsers.Any(item1 => item1.User == user));
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Id") && item.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Name") && item.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Description") && item.Description.Contains(input.SearchString) ||
                    input.SearchIn.Contains("DatabaseId") && item.Database.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("DatabaseName") && item.Database.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("DatabaseEdgeFieldEdges") && item.DatabaseEdgeFieldEdges.Any(item1 => item1.Edge.Id.Contains(input.SearchString) || item1.Edge.Name.Contains(input.SearchString) || item1.Edge.EdgeNodes.Where(item2 => item2.Node.DatabaseNodeFieldNodes.Any(item3 => item3.DatabaseNodeField.Database.IsPublic || item3.DatabaseNodeField.Database.DatabaseUsers.Any(item4 => item4.User == user))).Any(item2 => item2.Node.Id.Contains(input.SearchString) || item2.Node.Name.Contains(input.SearchString) || item2.Node.DatabaseNodeFieldNodes.Where(item3 => item3.DatabaseNodeField.Database.IsPublic || item3.DatabaseNodeField.Database.DatabaseUsers.Any(item4 => item4.User == user)).Any(item3 => item3.DatabaseNodeField.IsSearchable && item3.Value.Contains(input.SearchString)))));
            // Sort it according to the parameters.
            switch ((input.SortBy, input.SortDirection))
            {
                case var sort when sort == ("Id", "Ascending"):
                    query = query.OrderBy(item => item.Id);
                    break;
                case var sort when sort == ("Id", "Descending"):
                    query = query.OrderByDescending(item => item.Id);
                    break;
                case var sort when sort == ("Name", "Ascending"):
                    query = query.OrderBy(item => item.Name);
                    break;
                case var sort when sort == ("Name", "Descending"):
                    query = query.OrderByDescending(item => item.Name);
                    break;
                case var sort when sort == ("DatabaseId", "Ascending"):
                    query = query.OrderBy(item => item.Database.Id);
                    break;
                case var sort when sort == ("DatabaseId", "Descending"):
                    query = query.OrderByDescending(item => item.Database.Id);
                    break;
                case var sort when sort == ("DatabaseName", "Ascending"):
                    query = query.OrderBy(item => item.Database.Name);
                    break;
                case var sort when sort == ("DatabaseName", "Descending"):
                    query = query.OrderByDescending(item => item.Database.Name);
                    break;
                case var sort when sort == ("DatabaseEdgeFieldEdgeCount", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseEdgeFieldEdges.Count());
                    break;
                case var sort when sort == ("DatabaseEdgeFieldEdgeCount", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseEdgeFieldEdges.Count());
                    break;
                default:
                    break;
            }
            // Include the related entitites.
            query = query
                .Include(item => item.Database)
                    .ThenInclude(item => item.DatabaseType)
                .Include(item => item.DatabaseEdgeFieldEdges);
            // Define the view.
            View = new ViewModel
            {
                Search = new SearchViewModel<DatabaseEdgeField>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
