﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Data.Seed;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Extensions
{
    /// <summary>
    /// Provides extensions for the application builder.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Seeds the database, if needed, with a default "Administrator" role and users and the code-defined algorithms.
        /// </summary>
        /// <param name="app">Represents the application builder.</param>
        /// <param name="configuration">Represents the application configuration options.</param>
        /// <returns></returns>
        public static async Task<IApplicationBuilder> SeedDatabaseAsync(this IApplicationBuilder app, IConfiguration configuration)
        {
            // Create the scope for the application.
            using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
            // Get the service provider.
            var serviceProvider = serviceScope.ServiceProvider;
            // Get the required services.
            var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            // Check if the administrator role doesn't already exist.
            if (!await roleManager.RoleExistsAsync("Administrator"))
            {
                // Define a new administrator role.
                var role = new Role
                {
                    Name = "Administrator",
                    DateTimeCreated = DateTime.Now
                };
                // Save it into the database.
                await roleManager.CreateAsync(role);
            }
            // Check if administrator users don't already exist.
            if (!(await userManager.GetUsersInRoleAsync("Administrator")).Any())
            {
                // Get the data for the default administrator users.
                var items = configuration.GetSection("Administrators").GetChildren().Select(item => new { Email = item.GetSection("Email").Value, Password = item.GetSection("Password").Value });
                // Go over each of the users.
                foreach (var item in items)
                {
                    // Try to get the user with the provided e-mail.
                    var user = await userManager.FindByEmailAsync(item.Email);
                    // Check if the user is already defined.
                    if (user != null)
                    {
                        // Add the user to the administrator role.
                        await userManager.AddToRoleAsync(user, "Administrator");
                        // Go to the next user.
                        continue;
                    }
                    // Define a new user.
                    user = new User()
                    {
                        UserName = item.Email,
                        Email = item.Email,
                        EmailConfirmed = true,
                        DateTimeCreated = DateTime.Now
                    };
                    // Save it into the database.
                    await userManager.CreateAsync(user, item.Password);
                    // Add the user to the administrator role.
                    await userManager.AddToRoleAsync(user, "Administrator");
                }
                // Check if there isn't any entry already defined.
                if (!context.DatabaseTypes.Any())
                {
                    // Mark the seed data for addition.
                    context.DatabaseTypes.AddRange(DatabaseTypes.Seed);
                    // Save the changes.
                    await context.SaveChangesAsync();
                }
                // Check if there isn't any entry already defined.
                if (!context.Databases.Any())
                {
                    // Mark the seed data for addition.
                    context.Databases.AddRange(Databases.Seed);
                    // Save the changes.
                    await context.SaveChangesAsync();
                }
                // Check if there isn't any entry already defined.
                if (!context.DatabaseNodeFields.Any())
                {
                    // Mark the seed data for addition.
                    context.DatabaseNodeFields.AddRange(DatabaseNodeFields.Seed);
                    // Save the changes.
                    await context.SaveChangesAsync();
                }
                // Check if there isn't any entry already defined.
                if (!context.DatabaseEdgeFields.Any())
                {
                    // Mark the seed data for addition.
                    context.DatabaseEdgeFields.AddRange(DatabaseEdgeFields.Seed);
                    // Save the changes.
                    await context.SaveChangesAsync();
                }
            }
            // Return the application.
            return app;
        }
    }
}
