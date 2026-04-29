using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IMS_API_.Data;
using IMS_API_.Models.Auth;
using IMS_API_.Models.DTO.Auth;
using IMS_API_.Models.Domains;
using IMS_API_.Repositories.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace IMS_API_.Repositories.Implementation;

public class AuthRepository(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    IConfiguration configuration,
    IMSDbContext imsDbContext) : IAuthRepository
{
    public async Task<AuthOperationResult<AuthResponseDto>> RegisterCustomerAsync(RegisterRequestDto request)
    {
        if (!string.IsNullOrWhiteSpace(request.Role) &&
            !request.Role.Equals("Customer", StringComparison.OrdinalIgnoreCase))
        {
            return new AuthOperationResult<AuthResponseDto>
            {
                Succeeded = false,
                Message = "Self registration supports Customer only. Staff registration is admin only."
            };
        }

        return await CreateUserAndTokenAsync(request, "Customer");
    }

    public Task<AuthOperationResult<AuthResponseDto>> RegisterStaffAsync(RegisterRequestDto request)
        => CreateUserAndTokenAsync(request, "Staff");

    public async Task<AuthOperationResult<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return new AuthOperationResult<AuthResponseDto> { Succeeded = false, Message = "Invalid email or password." };
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return new AuthOperationResult<AuthResponseDto> { Succeeded = false, Message = "Your account is deactivated." };
        }

        var isValidPassword = await userManager.CheckPasswordAsync(user, request.Password);
        if (!isValidPassword)
        {
            return new AuthOperationResult<AuthResponseDto> { Succeeded = false, Message = "Invalid email or password." };
        }

        return new AuthOperationResult<AuthResponseDto>
        {
            Succeeded = true,
            Message = "Login successful.",
            Data = await GenerateTokenResponse(user)
        };
    }

    public async Task<IReadOnlyCollection<StaffSummaryDto>> GetStaffAsync()
    {
        var staffUsers = await userManager.GetUsersInRoleAsync("Staff");
        return staffUsers
            .Select(x => new StaffSummaryDto
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email ?? string.Empty,
                PhoneNumber = x.PhoneNumber,
                IsActive = !(x.LockoutEnabled && x.LockoutEnd.HasValue && x.LockoutEnd > DateTimeOffset.UtcNow)
            })
            .ToList();
    }

    public async Task<AuthOperationResult<string>> UpdateStaffStatusAsync(Guid staffId, bool isActive)
    {
        var staffUser = await userManager.FindByIdAsync(staffId.ToString());
        if (staffUser is null)
        {
            return new AuthOperationResult<string> { Succeeded = false, Message = "Staff not found." };
        }

        if (!await userManager.IsInRoleAsync(staffUser, "Staff"))
        {
            return new AuthOperationResult<string> { Succeeded = false, Message = "Target user is not a staff account." };
        }

        await userManager.SetLockoutEnabledAsync(staffUser, true);
        await userManager.SetLockoutEndDateAsync(staffUser, isActive ? null : DateTimeOffset.MaxValue);

        var domainUser = await imsDbContext.AppUsers.FirstOrDefaultAsync(x => x.Id == staffId);
        if (domainUser is not null)
        {
            domainUser.IsActive = isActive;
            await imsDbContext.SaveChangesAsync();
        }

        return new AuthOperationResult<string>
        {
            Succeeded = true,
            Message = isActive ? "Staff activated." : "Staff deactivated.",
            Data = staffUser.Id.ToString()
        };
    }

    public async Task<AuthOperationResult<string>> UpdateStaffRoleAsync(Guid staffId, string role)
    {
        var normalizedRole = role.Trim();
        var allowedRoles = new[] { "Staff", "Customer" };
        if (!allowedRoles.Contains(normalizedRole, StringComparer.OrdinalIgnoreCase))
        {
            return new AuthOperationResult<string>
            {
                Succeeded = false,
                Message = "Invalid role. Allowed roles for staff management are Staff and Customer."
            };
        }

        var targetRole = allowedRoles.First(x => x.Equals(normalizedRole, StringComparison.OrdinalIgnoreCase));
        var user = await userManager.FindByIdAsync(staffId.ToString());
        if (user is null)
        {
            return new AuthOperationResult<string> { Succeeded = false, Message = "Staff not found." };
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Contains("Admin"))
        {
            return new AuthOperationResult<string>
            {
                Succeeded = false,
                Message = "Admin role cannot be modified from this endpoint."
            };
        }

        if (!await roleManager.RoleExistsAsync(targetRole))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(targetRole));
        }

        if (currentRoles.Count > 0)
        {
            await userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        var addRoleResult = await userManager.AddToRoleAsync(user, targetRole);
        if (!addRoleResult.Succeeded)
        {
            return new AuthOperationResult<string>
            {
                Succeeded = false,
                Message = string.Join(", ", addRoleResult.Errors.Select(x => x.Description))
            };
        }

        var domainUser = await imsDbContext.AppUsers.FirstOrDefaultAsync(x => x.Id == staffId);
        if (domainUser is not null)
        {
            domainUser.Role = ParseDomainRole(targetRole);
            await imsDbContext.SaveChangesAsync();
        }

        return new AuthOperationResult<string>
        {
            Succeeded = true,
            Message = $"User role updated to {targetRole}.",
            Data = user.Id.ToString()
        };
    }

    private async Task<AuthOperationResult<AuthResponseDto>> CreateUserAndTokenAsync(RegisterRequestDto request, string role)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return new AuthOperationResult<AuthResponseDto>
            {
                Succeeded = false,
                Message = "User already exists with this email."
            };
        }

        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            UserName = request.Email,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return new AuthOperationResult<AuthResponseDto>
            {
                Succeeded = false,
                Message = string.Join(", ", createResult.Errors.Select(e => e.Description))
            };
        }

        var roleResult = await userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            return new AuthOperationResult<AuthResponseDto>
            {
                Succeeded = false,
                Message = string.Join(", ", roleResult.Errors.Select(e => e.Description))
            };
        }

        var saveResult = await SaveDomainUserAsync(user, role);
        if (!saveResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return new AuthOperationResult<AuthResponseDto>
            {
                Succeeded = false,
                Message = saveResult.Message
            };
        }

        return new AuthOperationResult<AuthResponseDto>
        {
            Succeeded = true,
            Message = $"{role} registration successful.",
            Data = await GenerateTokenResponse(user)
        };
    }

    private async Task<AuthOperationResult<string>> SaveDomainUserAsync(ApplicationUser identityUser, string role)
    {
        if (await imsDbContext.AppUsers.AnyAsync(x => x.Id == identityUser.Id || x.Email == identityUser.Email || x.PhoneNumber == identityUser.PhoneNumber))
        {
            return new AuthOperationResult<string>
            {
                Succeeded = false,
                Message = "A domain user already exists with this email or phone number."
            };
        }

        var domainUser = new AppUser
        {
            Id = identityUser.Id,
            FullName = identityUser.FullName,
            Email = identityUser.Email ?? string.Empty,
            PhoneNumber = identityUser.PhoneNumber ?? string.Empty,
            PasswordHash = identityUser.PasswordHash ?? string.Empty,
            Role = ParseDomainRole(role),
            IsActive = true
        };

        imsDbContext.AppUsers.Add(domainUser);

        if (role.Equals("Customer", StringComparison.OrdinalIgnoreCase))
        {
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                UserId = domainUser.Id,
                RegisteredAt = DateTime.UtcNow
            };

            imsDbContext.Customers.Add(customer);
        }

        await imsDbContext.SaveChangesAsync();
        return new AuthOperationResult<string> { Succeeded = true, Message = "Domain user created." };
    }

    private static UserRole ParseDomainRole(string role) => role.ToLowerInvariant() switch
    {
        "admin" => UserRole.Admin,
        "staff" => UserRole.Staff,
        _ => UserRole.Customer
    };

    private async Task<AuthResponseDto> GenerateTokenResponse(ApplicationUser user)
    {
        var role = (await userManager.GetRolesAsync(user)).FirstOrDefault() ?? "Customer";

        var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
        var issuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is missing.");
        var audience = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience is missing.");

        var expiresAt = DateTime.UtcNow.AddHours(2);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expiresAt,
            UserId = user.Id.ToString(),
            Email = user.Email ?? string.Empty,
            Role = role
        };
    }
}
