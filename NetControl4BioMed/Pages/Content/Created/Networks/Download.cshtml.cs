using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.ViewModels;

namespace NetControl4BioMed.Pages.Content.Created.Networks
{
    [Authorize]
    public class DownloadModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public DownloadModel(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            [RegularExpression("Text|Json|CytoscapeJson|Excel", ErrorMessage = "The value is not valid.")]
            public string FileFormat { get; set; }

            public IEnumerable<string> Ids { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public IEnumerable<Network> Items { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(IEnumerable<string> ids)
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
            // Check if there aren't any IDs provided.
            if (ids == null || !ids.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid IDs have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Items = _context.Networks
                    .Where(item => item.NetworkUsers.Any(item1 => item1.User == user))
                    .Where(item => ids.Contains(item.Id))
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No networks have been found with the provided IDs.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
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
            // Check if there aren't any IDs provided.
            if (Input.Ids == null || !Input.Ids.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid IDs have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Items = _context.Networks
                    .Where(item => item.NetworkUsers.Any(item1 => item1.User == user))
                    .Where(item => Input.Ids.Contains(item.Id))
                    .Include(item => item.NetworkDatabases)
                        .ThenInclude(item => item.Database)
                            .ThenInclude(item => item.DatabaseType)
                    .Include(item => item.NetworkDatabases)
                        .ThenInclude(item => item.Database)
                            .ThenInclude(item => item.DatabaseNodeFields)
                                .ThenInclude(item => item.DatabaseNodeFieldNodes)
                                    .ThenInclude(item => item.Node)
                    .Include(item => item.NetworkDatabases)
                        .ThenInclude(item => item.Database)
                            .ThenInclude(item => item.DatabaseEdgeFields)
                                .ThenInclude(item => item.DatabaseEdgeFieldEdges)
                                    .ThenInclude(item => item.Edge)
                    .Include(item => item.NetworkNodes)
                        .ThenInclude(item => item.Node)
                    .Include(item => item.NetworkEdges)
                        .ThenInclude(item => item.Edge)
                            .ThenInclude(item => item.EdgeNodes)
                                .ThenInclude(item => item.Node)
                    .Include(item => item.AnalysisNetworks)
                        .ThenInclude(item => item.Analysis)
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No networks have been found with the provided IDs.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Check if the provided model isn't valid.
            if (!ModelState.IsValid)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An error has been encountered. Please check again the input fields.");
                // Redisplay the page.
                return Page();
            }
            // Define the stream of the file to return.
            var zipStream = new MemoryStream();
            // Define a new ZIP archive.
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                // Go over each of the networks to download.
                foreach (var network in View.Items)
                {
                    // Check which should be the format of the files within the archive.
                    if (Input.FileFormat == "Text")
                    {
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"{network.Name}-{network.Id}.txt", CompressionLevel.Fastest).Open();
                        // Define the stream writer for the file.
                        using var streamWriter = new StreamWriter(stream);
                        // Get the default values.
                        var interactionType = network.NetworkDatabases
                            .FirstOrDefault()?.Database.DatabaseType.Name.ToLower();
                        // Define the data to be written to the file.
                        var data = string.Join("\n", network.NetworkEdges.Select(item => item.Edge).Select(item =>
                        {
                            var sourceNode = item.EdgeNodes.FirstOrDefault(item1 => item1.Type == EdgeNodeType.Source)?.Node;
                            var targetNode = item.EdgeNodes.FirstOrDefault(item1 => item1.Type == EdgeNodeType.Target)?.Node;
                            return $"{sourceNode?.Name}\t{interactionType}\t{targetNode.Name}";
                        }));
                        // Write the data to the stream corresponding to the file.
                        await streamWriter.WriteAsync(data);
                    }
                    else if (Input.FileFormat == "Json")
                    {
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"{network.Name}-{network.Id}.json", CompressionLevel.Fastest).Open();
                        // Define the data to be serialized to the file.
                        var data = new
                        {
                            Id = network.Id,
                            Name = network.Name,
                            Description = network.Description,
                            Algorithm = network.Algorithm.ToString(),
                            Nodes = network.NetworkNodes
                                .Select(item => item.Node)
                                .Select(item => new
                                {
                                    Id = item.Id,
                                    Name = item.Name,
                                    Description = item.Description,
                                    Values = item.DatabaseNodeFieldNodes
                                        .Select(item1 => new
                                        {
                                            DatabaseId = item1.DatabaseNodeField.Database.Id,
                                            DatabaseName = item1.DatabaseNodeField.Database.Name,
                                            DatabaseFieldId = item1.DatabaseNodeField.Id,
                                            DatabaseFieldName = item1.DatabaseNodeField.Name,
                                            Value = item1.Value
                                        })
                                }),
                            Edges = network.NetworkEdges
                                .Select(item => item.Edge)
                                .Select(item => new
                                {
                                    Id = item.Id,
                                    Description = item.Description,
                                    Nodes = item.EdgeNodes
                                        .Select(item1 => new
                                        {
                                            NodeId = item1.Node.Id,
                                            NodeName = item1.Node.Name,
                                            Type = item1.Type.ToString()
                                        }),
                                    Values = item.DatabaseEdgeFieldEdges
                                        .Select(item1 => new
                                        {
                                            DatabaseId = item1.DatabaseEdgeField.Database.Id,
                                            DatabaseName = item1.DatabaseEdgeField.Database.Name,
                                            DatabaseFieldId = item1.DatabaseEdgeField.Id,
                                            DatabaseFieldName = item1.DatabaseEdgeField.Name,
                                            Value = item1.Value
                                        })
                                })
                        };
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, new JsonSerializerOptions { WriteIndented = true });
                    }
                    else if (Input.FileFormat == "CytoscapeJson")
                    {
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"{network.Name}-{network.Id}.json", CompressionLevel.Fastest).Open();
                        // Get the default values.
                        var interactionType = network.NetworkDatabases
                            .FirstOrDefault()?.Database.DatabaseType.Name.ToLower();
                        var nodeClasses = new List<string> { "node" };
                        var edgeClasses = new List<string> { "edge" };
                        // Define the data to be serialized to the file.
                        var data = new CytoscapeViewModel
                        {
                            Data = new CytoscapeViewModel.CytoscapeData
                            {
                                Elements = new CytoscapeViewModel.CytoscapeData.CytoscapeElements
                                {
                                    Nodes = network.NetworkNodes
                                        .Select(item => item.Node)
                                        .Select(item => new CytoscapeViewModel.CytoscapeData.CytoscapeElements.CytoscapeNode
                                        {
                                            Data = new CytoscapeViewModel.CytoscapeData.CytoscapeElements.CytoscapeNode.CytoscapeNodeData
                                            {
                                                Id = item.Id,
                                                Name = item.Name,
                                                Href = null,
                                                Alias = item.DatabaseNodeFieldNodes
                                                    .Where(item1 => item1.DatabaseNodeField.IsSearchable)
                                                    .Select(item1 => item1.Value)
                                            },
                                            Classes = nodeClasses.Concat(item.NetworkNodes.Select(item => item.Type.ToString().ToLower()))
                                        }),
                                    Edges = network.NetworkEdges
                                        .Select(item => item.Edge)
                                        .Select(item => new CytoscapeViewModel.CytoscapeData.CytoscapeElements.CytoscapeEdge
                                        {
                                            Data = new CytoscapeViewModel.CytoscapeData.CytoscapeElements.CytoscapeEdge.CytoscapeEdgeData
                                            {
                                                Id = item.Id,
                                                Name = item.Name,
                                                Source = item.EdgeNodes.FirstOrDefault(item1 => item1.Type == EdgeNodeType.Source)?.Node.Id,
                                                Target = item.EdgeNodes.FirstOrDefault(item1 => item1.Type == EdgeNodeType.Target)?.Node.Id,
                                                Interaction = interactionType
                                            },
                                            Classes = edgeClasses
                                        })
                                },
                                Layout = new CytoscapeViewModel.CytoscapeData.CytoscapeLayout
                                {
                                    Name = "cose"
                                }
                            }
                        };
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, new JsonSerializerOptions { IgnoreNullValues = true });
                    }
                    else if (Input.FileFormat == "Excel")
                    {
                        // Get the required data.
                        var databases = network.NetworkDatabases
                            .Select(item => item.Database)
                            .Where(item1 => item1.IsPublic || item1.DatabaseUsers.Any(item2 => item2.User == user));
                        var databaseNodeFields = databases
                            .Select(item => item.DatabaseNodeFields)
                            .SelectMany(item => item);
                        var databaseEdgeFields = databases
                            .Select(item => item.DatabaseEdgeFields)
                            .SelectMany(item => item);
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"{network.Name}-{network.Id}.xlsx", CompressionLevel.Fastest).Open();
                        // Define the rows in the first sheet.
                        var worksheet1Rows = new List<List<string>>
                        {
                            new List<string> { "Internal ID", "Date", "Name", "Description", "Algorithm" },
                            new List<string> { network.Id, network.DateTimeCreated.ToString(), network.Name, network.Description, network.Algorithm.ToString() }
                        };
                        // Define the rows in the second sheet.
                        var worksheet2Rows = new List<List<string>>
                        {
                            new List<string> { "Internal ID", "Name", "Type" }
                                .Concat(databaseNodeFields
                                    .Select(item => $"({item.Database.Name}) {item.Name}"))
                                .ToList()
                        }
                        .Concat(network.NetworkNodes
                            .Select(item =>
                                new List<string> { item.Node.Id, item.Node.Name, item.Type.ToString() }
                                    .Concat(databaseNodeFields
                                        .Select(item1 => item1.DatabaseNodeFieldNodes.FirstOrDefault(item2 => item2.Node == item.Node)?.Value))
                                    .ToList()))
                        .ToList();
                        // Define the rows in the third sheet.
                        var worksheet3Rows = new List<List<string>>
                        {
                            new List<string> { "Internal ID", "Source node ID", "Source node name", "Target node ID", "Target node name" }
                                .Concat(databaseEdgeFields
                                    .Select(item => item.Name))
                                .ToList()
                        }
                        .Concat(network.NetworkEdges
                            .Select(item =>
                            {
                                var sourceNode = item.Edge.EdgeNodes.FirstOrDefault(item1 => item1.Type == EdgeNodeType.Source)?.Node;
                                var targetNode = item.Edge.EdgeNodes.FirstOrDefault(item1 => item1.Type == EdgeNodeType.Target)?.Node;
                                return new List<string> { item.Edge.Id, sourceNode?.Id, sourceNode?.Name, targetNode?.Id, targetNode?.Name }
                                    .Concat(databaseEdgeFields
                                        .Select(item1 => item1.DatabaseEdgeFieldEdges.FirstOrDefault(item2 => item2.Edge == item.Edge)?.Value))
                                    .ToList();
                            }))
                        .ToList();
                        // Define the stream for the file.
                        var fileStream = new MemoryStream();
                        // Define the Excel file.
                        using SpreadsheetDocument document = SpreadsheetDocument.Create(fileStream, SpreadsheetDocumentType.Workbook);
                        // Definte a new workbook part.
                        var workbookPart = document.AddWorkbookPart();
                        workbookPart.Workbook = new Workbook();
                        var worksheets = workbookPart.Workbook.AppendChild(new Sheets());
                        // Define the first worksheet.
                        var worksheet1Part = workbookPart.AddNewPart<WorksheetPart>();
                        var worksheet1Data = new SheetData();
                        var worksheet1 = new Sheet { Id = workbookPart.GetIdOfPart(worksheet1Part), SheetId = 1, Name = "Details" };
                        worksheet1Part.Worksheet = new Worksheet(worksheet1Data);
                        worksheet1Data.Append(worksheet1Rows.Select(item => new Row(item.Select(item1 => new Cell { DataType = CellValues.String, CellValue = new CellValue(item1) }))));
                        worksheets.Append(worksheet1);
                        // Define the second worksheet.
                        var worksheet2Part = workbookPart.AddNewPart<WorksheetPart>();
                        var worksheet2Data = new SheetData();
                        var worksheet2 = new Sheet { Id = workbookPart.GetIdOfPart(worksheet2Part), SheetId = 2, Name = "Nodes" };
                        worksheet2Part.Worksheet = new Worksheet(worksheet2Data);
                        worksheet2Data.Append(worksheet2Rows.Select(item => new Row(item.Select(item1 => new Cell { DataType = CellValues.String, CellValue = new CellValue(item1) }))));
                        worksheets.Append(worksheet2);
                        // Define the third worksheet.
                        var worksheet3Part = workbookPart.AddNewPart<WorksheetPart>();
                        var worksheet3Data = new SheetData();
                        var worksheet3 = new Sheet { Id = workbookPart.GetIdOfPart(worksheet3Part), SheetId = 3, Name = "Edges" };
                        worksheet3Part.Worksheet = new Worksheet(worksheet3Data);
                        worksheet3Data.Append(worksheet3Rows.Select(item => new Row(item.Select(item1 => new Cell { DataType = CellValues.String, CellValue = new CellValue(item1) }))));
                        worksheets.Append(worksheet3);
                        // Close the document.
                        document.Close();
                        // Reset the stream position.
                        fileStream.Position = 0;
                        // Copy it to the archive stream.
                        await fileStream.CopyToAsync(stream);
                    }
                    else
                    {
                        // Add an error to the model.
                        ModelState.AddModelError(string.Empty, "The provided file format is not valid or is not yet implemented.");
                        // Redisplay the page.
                        return Page();
                    }
                }
            }
            // Reset the stream position.
            zipStream.Position = 0;
            // Return the archive file.
            return new FileStreamResult(zipStream, MediaTypeNames.Application.Zip) { FileDownloadName = "NetControl4BioMed-Networks.zip" };
        }
    }
}
