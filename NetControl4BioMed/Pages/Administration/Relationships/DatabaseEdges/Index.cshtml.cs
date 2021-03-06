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
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.ViewModels;

namespace NetControl4BioMed.Pages.Administration.Relationships.DatabaseEdges
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
            public SearchViewModel<DatabaseEdge> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "DatabaseId", "Database ID" },
                    { "DatabaseName", "Database name" },
                    { "EdgeId", "Edge ID" },
                    { "EdgeName", "Edge name" },
                    { "SourceNodeId", "Source node ID" },
                    { "SourceNodeName", "Source node name" },
                    { "TargetNodeId", "Target node ID" },
                    { "TargetNodeName", "Target node name" }
                },
                Filter = new Dictionary<string, string>
                {
                    { "IsDatabasePublic", "Database is public" },
                    { "IsNotDatabasePublic", "Database is not public" },
                    { "HasEdgeDatabaseEdgeFieldEdges", "Edge has database edge field edges" },
                    { "HasNoEdgeDatabaseEdgeFieldEdges", "Edge doesn't have database edge field edges" }
                },
                SortBy = new Dictionary<string, string>
                {
                    { "DatabaseId", "Database ID" },
                    { "DatabaseName", "Database name" },
                    { "EdgeId", "Edge ID" },
                    { "EdgeName", "EdgeName" },
                    { "SourceNodeId", "Source node ID" },
                    { "SourceNodeName", "Source node name" },
                    { "TargetNodeId", "Target node ID" },
                    { "TargetNodeName", "Target node name" },
                    { "DatabaseEdgeFieldEdgeCount", "Number of database edge field edges" }
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
            var query = _context.DatabaseEdges
                .Where(item => !item.Edge.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"));
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("DatabaseId") && item.Database.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("DatabaseName") && item.Database.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("EdgeId") && item.Edge.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("EdgeName") && item.Edge.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("SourceNodeId") && item.Edge.EdgeNodes.First(item1 => item1.Type == EdgeNodeType.Source).Node.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("SourceNodeName") && item.Edge.EdgeNodes.First(item1 => item1.Type == EdgeNodeType.Source).Node.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("TargetNodeId") && item.Edge.EdgeNodes.First(item1 => item1.Type == EdgeNodeType.Target).Node.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("TargetNodeName") && item.Edge.EdgeNodes.First(item1 => item1.Type == EdgeNodeType.Target).Node.Name.Contains(input.SearchString));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("IsDatabasePublic") ? item.Database.IsPublic : true)
                .Where(item => input.Filter.Contains("IsNotDatabasePublic") ? !item.Database.IsPublic : true)
                .Where(item => input.Filter.Contains("HasEdgeDatabaseEdgeFieldEdges") ? item.Edge.DatabaseEdgeFieldEdges.Any(item1 => item1.DatabaseEdgeField.Database == item.Database) : true)
                .Where(item => input.Filter.Contains("HasNoEdgeDatabaseEdgeFieldEdges") ? !item.Edge.DatabaseEdgeFieldEdges.Any(item1 => item1.DatabaseEdgeField.Database == item.Database) : true);
            // Sort it according to the parameters.
            switch ((input.SortBy, input.SortDirection))
            {
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
                case var sort when sort == ("EdgeId", "Ascending"):
                    query = query.OrderBy(item => item.Edge.Id);
                    break;
                case var sort when sort == ("EdgeId", "Descending"):
                    query = query.OrderByDescending(item => item.Edge.Id);
                    break;
                case var sort when sort == ("EdgeName", "Ascending"):
                    query = query.OrderBy(item => item.Edge.Name);
                    break;
                case var sort when sort == ("EdgeName", "Descending"):
                    query = query.OrderByDescending(item => item.Edge.Name);
                    break;
                case var sort when sort == ("SourceNodeId", "Ascending"):
                    query = query.OrderBy(item => item.Edge.EdgeNodes.First(item1 => item1.Type == EdgeNodeType.Source).Node.Id);
                    break;
                case var sort when sort == ("SourceNodeId", "Descending"):
                    query = query.OrderByDescending(item => item.Edge.EdgeNodes.First(item1 => item1.Type == EdgeNodeType.Source).Node.Id);
                    break;
                case var sort when sort == ("SourceNodeName", "Ascending"):
                    query = query.OrderBy(item => item.Edge.EdgeNodes.First(item1 => item1.Type == EdgeNodeType.Source).Node.Name);
                    break;
                case var sort when sort == ("SourceNodeName", "Descending"):
                    query = query.OrderByDescending(item => item.Edge.EdgeNodes.First(item1 => item1.Type == EdgeNodeType.Source).Node.Name);
                    break;
                case var sort when sort == ("TargetNodeId", "Ascending"):
                    query = query.OrderBy(item => item.Edge.EdgeNodes.First(item1 => item1.Type == EdgeNodeType.Target).Node.Id);
                    break;
                case var sort when sort == ("TargetNodeId", "Descending"):
                    query = query.OrderByDescending(item => item.Edge.EdgeNodes.First(item1 => item1.Type == EdgeNodeType.Target).Node.Id);
                    break;
                case var sort when sort == ("TargetNodeName", "Ascending"):
                    query = query.OrderBy(item => item.Edge.EdgeNodes.First(item1 => item1.Type == EdgeNodeType.Target).Node.Name);
                    break;
                case var sort when sort == ("TargetNodeName", "Descending"):
                    query = query.OrderByDescending(item => item.Edge.EdgeNodes.First(item1 => item1.Type == EdgeNodeType.Target).Node.Name);
                    break;
                case var sort when sort == ("DatabaseEdgeFieldEdgeCount", "Ascending"):
                    query = query.OrderBy(item => item.Edge.DatabaseEdgeFieldEdges.Count(item1 => item1.DatabaseEdgeField.Database == item.Database));
                    break;
                case var sort when sort == ("DatabaseEdgeFieldEdgeCount", "Descending"):
                    query = query.OrderByDescending(item => item.Edge.DatabaseEdgeFieldEdges.Count(item1 => item1.DatabaseEdgeField.Database == item.Database));
                    break;
                default:
                    break;
            }
            // Include the related entitites.
            query = query
                .Include(item => item.Edge)
                    .ThenInclude(item => item.DatabaseEdgeFieldEdges)
                        .ThenInclude(item => item.DatabaseEdgeField)
                            .ThenInclude(item => item.Database)
                .Include(item => item.Edge)
                    .ThenInclude(item => item.EdgeNodes)
                        .ThenInclude(item => item.Node)
                .Include(item => item.Database);
            // Define the view.
            View = new ViewModel
            {
                Search = new SearchViewModel<DatabaseEdge>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
