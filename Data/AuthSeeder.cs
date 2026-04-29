using IMS_API_.Models.Auth;
using IMS_API_.Models.Domains;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IMS_API_.Data;

public static class AuthSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var imsDbContext = scope.ServiceProvider.GetRequiredService<IMSDbContext>();

        var fullName = configuration["SeedAdmin:FullName"] ?? "Admin";
        var email = configuration["SeedAdmin:Email"] ?? "reganbudhathoki69@gmail.com";
        var phone = configuration["SeedAdmin:PhoneNumber"] ?? "9766433737";
        var password = configuration["SeedAdmin:Password"] ?? "Admin123";

        var roles = new[] { "Admin", "Staff", "Customer" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        var admins = await userManager.GetUsersInRoleAsync("Admin");
        if (admins.Count > 1)
        {
            throw new InvalidOperationException("Only one admin is allowed in this application.");
        }

        if (admins.Count == 1)
        {
            var existingAdmin = admins[0];
            await SyncAdminAsync(existingAdmin, fullName, email, phone, password, userManager, imsDbContext);
            return;
        }

        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is null)
        {
            var admin = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                FullName = fullName,
                Email = email,
                UserName = email,
                PhoneNumber = phone,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Unable to create seed admin user: {errors}");
            }

            await userManager.AddToRoleAsync(admin, "Admin");
            await SyncAdminAsync(admin, fullName, email, phone, password, userManager, imsDbContext);
        }
        else
        {
            var userRoles = await userManager.GetRolesAsync(existingUser);
            if (!userRoles.Contains("Admin"))
            {
                if (admins.Count == 0)
                {
                    await userManager.AddToRoleAsync(existingUser, "Admin");
                }
                else
                {
                    throw new InvalidOperationException("Only one admin is allowed in this application.");
                }
            }
            await SyncAdminAsync(existingUser, fullName, email, phone, password, userManager, imsDbContext);
        }
    }

    private static async Task SyncAdminAsync(
        ApplicationUser admin,
        string fullName,
        string email,
        string phone,
        string password,
        UserManager<ApplicationUser> userManager,
        IMSDbContext imsDbContext)
    {
        admin.FullName = fullName;
        admin.Email = email;
        admin.UserName = email;
        admin.PhoneNumber = phone;
        admin.EmailConfirmed = true;

        var updateResult = await userManager.UpdateAsync(admin);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Unable to update seed admin user: {errors}");
        }

        var hasConfiguredPassword = await userManager.CheckPasswordAsync(admin, password);
        if (!hasConfiguredPassword)
        {
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(admin);
            var resetResult = await userManager.ResetPasswordAsync(admin, resetToken, password);
            if (!resetResult.Succeeded)
            {
                var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Unable to set seed admin password: {errors}");
            }
        }

        var refreshedAdmin = await userManager.FindByIdAsync(admin.Id.ToString()) ?? admin;
        var domainAdmin = await imsDbContext.AppUsers.FirstOrDefaultAsync(x => x.Id == refreshedAdmin.Id);
        if (domainAdmin is null)
        {
            imsDbContext.AppUsers.Add(new AppUser
            {
                Id = refreshedAdmin.Id,
                FullName = refreshedAdmin.FullName,
                Email = refreshedAdmin.Email ?? string.Empty,
                PhoneNumber = refreshedAdmin.PhoneNumber ?? string.Empty,
                PasswordHash = refreshedAdmin.PasswordHash ?? string.Empty,
                Role = UserRole.Admin,
                IsActive = true
            });
        }
        else
        {
            domainAdmin.Role = UserRole.Admin;
            domainAdmin.IsActive = true;
            domainAdmin.FullName = refreshedAdmin.FullName;
            domainAdmin.Email = refreshedAdmin.Email ?? string.Empty;
            domainAdmin.PhoneNumber = refreshedAdmin.PhoneNumber ?? string.Empty;
            domainAdmin.PasswordHash = refreshedAdmin.PasswordHash ?? string.Empty;
        }

        await imsDbContext.SaveChangesAsync();
    }
}
