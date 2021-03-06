using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.ViewModels;

namespace NetControl4BioMed.Pages.Administration.Relationships.NodeCollectionDatabases
{
    [Authorize(Roles = "Administrator")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly LinkGenerator _linkGenerator;

        public IndexModel(ApplicationDbContext context, LinkGenerator linkGenerator)
        {
            _context = context;
            _linkGenerator = linkGenerator;
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public SearchViewModel<NodeCollectionDatabase> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "NodeCollectionId", "Node collection ID" },
                    { "NodeCollectionName", "Node collection name" },
                    { "DatabaseId", "Database ID" },
                    { "DatabaseName", "Database name" }
                },
                Filter = new Dictionary<string, string>
                {
                    { "IsDatabasePublic", "Database is public" },
                    { "IsNotDatabasePublic", "Database is not public" },
                    { "HasNodeCollectionNodes", "Node collection has node collection nodes" },
                    { "HasNoNodeCollectionNodes", "Node collection doesn't have node collection nodes nodes" }
                },
                SortBy = new Dictionary<string, string>
                {
                    { "DatabaseId", "Database ID" },
                    { "DatabaseName", "Database name" },
                    { "NodeCollectionId", "Node collection ID" },
                    { "NodeCollectionName", "Node collection name" },
                    { "NodeCollectionNodeCount", "Number of node collection nodes" }
                }
            };
        }

        public IActionResult OnGet(string searchString = null, IEnumerable<string> searchIn = null, IEnumerable<string> filter = null, string sortBy = null, string sortDirection = null, int? itemsPerPage = null, int? currentPage = 1)
        {
            // Define the search input.
            var input = new SearchInputViewModel(ViewModel.SearchOptions, null, searchString, searchIn, filter, sortBy, sortDirection, itemsPerPage, currentPage);
            // Check if any of the provided variables was null before the reassignment.
            if (input.NeedsRedirect)
            {
                // Redirect to the page where they are all explicitly defined.
                return RedirectToPage(new { searchString = input.SearchString, searchIn = input.SearchIn, filter = input.Filter, sortBy = input.SortBy, sortDirection = input.SortDirection, itemsPerPage = input.ItemsPerPage, currentPage = input.CurrentPage });
            }
            // Start with all of the items in the non-generic databases.
            var query = _context.NodeCollectionDatabases
                .Where(item => !item.NodeCollection.NodeCollectionDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"));
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("NodeCollectionId") && item.NodeCollection.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("NodeCollectionName") && item.NodeCollection.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("DatabaseId") && item.Database.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("DatabaseName") && item.Database.Name.Contains(input.SearchString));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("IsDatabasePublic") ? item.Database.IsPublic : true)
                .Where(item => input.Filter.Contains("IsNotDatabasePublic") ? !item.Database.IsPublic : true)
                .Where(item => input.Filter.Contains("HasNodeCollectionNodes") ? item.NodeCollection.NodeCollectionNodes.Any(item1 => item1.Node.DatabaseNodes.Any(item2 => item2.Database == item.Database)) : true)
                .Where(item => input.Filter.Contains("HasNoNodeCollectionNodes") ? !item.NodeCollection.NodeCollectionNodes.Any(item1 => item1.Node.DatabaseNodes.Any(item2 => item2.Database == item.Database)) : true);
            // Sort it according to the parameters.
            switch ((input.SortBy, input.SortDirection))
            {
                case var sort when sort == ("NodeCollectionId", "Ascending"):
                    query = query.OrderBy(item => item.NodeCollection.Id);
                    break;
                case var sort when sort == ("NodeCollectionId", "Descending"):
                    query = query.OrderByDescending(item => item.NodeCollection.Id);
                    break;
                case var sort when sort == ("NodeCollectionName", "Ascending"):
                    query = query.OrderBy(item => item.NodeCollection.Name);
                    break;
                case var sort when sort == ("NodeCollectionName", "Descending"):
                    query = query.OrderByDescending(item => item.NodeCollection.Name);
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
                case var sort when sort == ("NodeCollectionNodeCount", "Ascending"):
                    query = query.OrderBy(item => item.NodeCollection.NodeCollectionNodes.Count(item1 => item1.Node.DatabaseNodes.Any(item2 => item2.Database == item.Database)));
                    break;
                case var sort when sort == ("NodeCollectionNodeCount", "Descending"):
                    query = query.OrderByDescending(item => item.NodeCollection.NodeCollectionNodes.Count(item1 => item1.Node.DatabaseNodes.Any(item2 => item2.Database == item.Database)));
                    break;
                default:
                    break;
            }
            // Include the related entitites.
            query = query
                .Include(item => item.NodeCollection)
                    .ThenInclude(item => item.NodeCollectionNodes)
                        .ThenInclude(item => item.Node)
                            .ThenInclude(item => item.DatabaseNodes)
                                .ThenInclude(item => item.Database)
                .Include(item => item.Database);
            // Define the view.
            View = new ViewModel
            {
                Search = new SearchViewModel<NodeCollectionDatabase>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
