using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Pages.Administration.Permissions.DatabaseUsers
{
    [Authorize(Roles = "Administrator")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string UserString { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string DatabaseString { get; set; }
        }

        public IActionResult OnGet(string userString = null, string databaseString = null)
        {
            // Check if there aren't any databases.
            if (!_context.Databases.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No databases could be found. Please create a database first.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Permissions/DatabaseUsers/Index");
            }
            // Define the input.
            Input = new InputModel
            {
                UserString = userString,
                DatabaseString = databaseString
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Check if there aren't any databases.
            if (!_context.Databases.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No databases could be found. Please create a database first.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Permissions/DatabaseUsers/Index");
            }
            // Check if the provided model isn't valid.
            if (!ModelState.IsValid)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An error has been encountered. Please check again the input fields.");
                // Redisplay the page.
                return Page();
            }
            // Get the user based on the provided string.
            var user = _context.Users.FirstOrDefault(item => item.Id == Input.UserString || item.Email == Input.UserString);
            // Check if there was no user found.
            if (user == null)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "No user could be found with the given string.");
                // Redisplay the page.
                return Page();
            }
            // Get the database based on the provided string.
            var database = _context.Databases.FirstOrDefault(item => item.Id == Input.DatabaseString || item.Name == Input.DatabaseString);
            // Check if there was no database found.
            if (database == null)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "No database could be found with the given string.");
                // Redisplay the page.
                return Page();
            }
            // Create a new database user.
            var databaseUser = new DatabaseUser
            {
                DatabaseId = database.Id,
                Database = database,
                UserId = user.Id,
                User = user,
                DateTimeCreated = DateTime.Now
            };
            // Mark it for addition to the database.
            _context.DatabaseUsers.Add(databaseUser);
            // Save the changes.
            await _context.SaveChangesAsync();
            // Display a message.
            TempData["StatusMessage"] = "Success: 1 database user created successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Permissions/DatabaseUsers/Index");
        }
    }
}
