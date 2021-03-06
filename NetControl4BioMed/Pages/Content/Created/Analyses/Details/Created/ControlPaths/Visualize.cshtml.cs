using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;

namespace NetControl4BioMed.Pages.Content.Created.Analyses.Details.Created.ControlPaths
{
    [Authorize]
    public class VisualizeModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly LinkGenerator _linkGenerator;

        public VisualizeModel(UserManager<User> userManager, ApplicationDbContext context, LinkGenerator linkGenerator)
        {
            _userManager = userManager;
            _context = context;
            _linkGenerator = linkGenerator;
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public Analysis Analysis { get; set; }

            public string CytoscapeJson { get; set; }
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
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Get the item with the provided ID.
            var items = _context.ControlPaths
                .Where(item => item.Analysis.AnalysisUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == id);
            // Check if there was no item found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Analysis = items
                    .Select(item => item.Analysis)
                    .First(),
                CytoscapeJson = JsonSerializer.Serialize(items
                    .Include(item => item.Analysis)
                        .ThenInclude(item => item.AnalysisDatabases)
                            .ThenInclude(item => item.Database)
                                .ThenInclude(item => item.DatabaseType)
                    .Include(item => item.Analysis)
                        .ThenInclude(item => item.AnalysisNodes)
                            .ThenInclude(item => item.Node)
                                .ThenInclude(item => item.DatabaseNodeFieldNodes)
                                    .ThenInclude(item => item.DatabaseNodeField)
                    .Include(item => item.Analysis)
                        .ThenInclude(item => item.AnalysisEdges)
                            .ThenInclude(item => item.Edge)
                                .ThenInclude(item => item.EdgeNodes)
                                    .ThenInclude(item => item.Node)
                    .Include(item => item.Paths)
                        .ThenInclude(item => item.PathNodes)
                            .ThenInclude(item => item.Node)
                    .Include(item => item.Paths)
                        .ThenInclude(item => item.PathEdges)
                            .ThenInclude(item => item.Edge)
                    .First()
                    .GetCytoscapeViewModel(_linkGenerator), new JsonSerializerOptions { IgnoreNullValues = true })
            };
            // Return the page.
            return Page();
        }
    }
}
