using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SmartWay.WebApi.Data;

public class ApplicationDbContextInitializer
{
    private readonly ILogger<ApplicationDbContextInitializer> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public ApplicationDbContextInitializer(ILogger<ApplicationDbContextInitializer> logger,
        ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }
    public async Task InitializeAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured while initialing the database");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();

        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured while seeding the database.");
            throw;
        }
    }

    private async Task TrySeedAsync()
    {
        if (_userManager.Users.Any())
        {
            _logger.LogInformation("Database is not empty");
            return;
        }
        
        _logger.LogInformation("Creating user...");

        await _userManager.CreateAsync(new IdentityUser()
        {
            Email = "test@mail.ru",
            UserName = "test@mail.ru",
        }, "root");

        await _context.SaveChangesAsync();
        
        _logger.LogInformation("User created successfully...");
    }
}