using Microsoft.AspNetCore.Identity;

namespace TodoApp.Api.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        
        if (await userManager.FindByEmailAsync("admin@email.com") == null)
        {
            var user = new IdentityUser
            {
                UserName = "admin@email.com",
                Email = "admin@email.com",
                EmailConfirmed = true
            };
            
            await userManager.CreateAsync(user, "Admin@123");
        }
    }
}
