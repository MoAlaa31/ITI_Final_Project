using ITI_Project.Core.Models.Location;
using ITI_Project.Core.Models.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ITI_Project.Repository.Data
{
    public static class AppDbContextSeed
    {
        // 1. read file
        // 2. deserialize json file into list of Models
        // 3. iterate this list and add this into dbContext

        public async static Task SeedAsync(AppDbContext _dbContext, ILogger logger)
        {
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            /* ======================= seeding Governrate Data ======================= */
            if (!await _dbContext.governorates.AnyAsync())
            {
                var GovernoratesData = await File.ReadAllTextAsync("../ITI_Project.Repository/Data/SeedData/Governorates.json");
                var governorates = JsonSerializer.Deserialize<List<Governorate>>(GovernoratesData, jsonOptions);

                if (governorates?.Count > 0)
                {
                    logger.LogInformation("Total governorates in JSON: {Count}", governorates.Count);

                    await _dbContext.governorates.AddRangeAsync(governorates);
                }
            }   

            /* ======================= seeding Regions Data ======================= */
            if (!await _dbContext.regions.AnyAsync())
            {
                var RegionsData = await File.ReadAllTextAsync("../ITI_Project.Repository/Data/SeedData/Regions.json");
                var regions = JsonSerializer.Deserialize<List<Region>>(RegionsData, jsonOptions);

                if (regions?.Count > 0)
                {
                    logger.LogInformation("Total regions in JSON: {Count}", regions.Count);

                    await _dbContext.regions.AddRangeAsync(regions);
                }
            }

            /* ======================= seeding Services Data ======================= */
            if (!await _dbContext.Services.AnyAsync())
            {
                var ServicesData = await File.ReadAllTextAsync("../ITI_Project.Repository/Data/SeedData/Services.json");
                var services = JsonSerializer.Deserialize<List<Service>>(ServicesData, jsonOptions);

                if (services?.Count > 0)
                {
                    logger.LogInformation("Total services in JSON: {Count}", services.Count);

                    await _dbContext.Services.AddRangeAsync(services);
                }
            }

            // save changes to database
            try
            {
                await _dbContext.SaveChangesAsync();
                logger.LogInformation("Seeding completed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving seed data to database");
            }
        }
    }
}
