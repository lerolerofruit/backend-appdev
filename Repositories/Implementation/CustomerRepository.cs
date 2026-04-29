using IMS_API_.Data;
using IMS_API_.Models.Auth;
using IMS_API_.Models.Domains;
using IMS_API_.Models.DTO.Auth;
using IMS_API_.Models.DTO.Customer;
using IMS_API_.Repositories.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IMS_API_.Repositories.Implementation;

public class CustomerRepository(
    IMSDbContext imsDbContext,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager) : ICustomerRepository
{
    public async Task<AuthOperationResult<string>> RegisterCustomerWithVehicleByStaffAsync(Guid staffUserId, RegisterCustomerWithVehicleDto request)
    {
        var staffUser = await imsDbContext.AppUsers.FirstOrDefaultAsync(x => x.Id == staffUserId && x.Role == UserRole.Staff && x.IsActive);
        if (staffUser is null)
        {
            return new AuthOperationResult<string> { Succeeded = false, Message = "Only active staff can register customers." };
        }

        if (await userManager.FindByEmailAsync(request.Email) is not null)
        {
            return new AuthOperationResult<string> { Succeeded = false, Message = "User already exists with this email." };
        }

        if (await imsDbContext.AppUsers.AnyAsync(x => x.PhoneNumber == request.PhoneNumber))
        {
            return new AuthOperationResult<string> { Succeeded = false, Message = "Phone number already exists." };
        }

        if (await imsDbContext.Vehicles.AnyAsync(x => x.VehicleNumber == request.Vehicle.VehicleNumber))
        {
            return new AuthOperationResult<string> { Succeeded = false, Message = "Vehicle number already exists." };
        }

        var identityUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            UserName = request.Email,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = true
        };

        var identityCreate = await userManager.CreateAsync(identityUser, request.Password);
        if (!identityCreate.Succeeded)
        {
            return new AuthOperationResult<string>
            {
                Succeeded = false,
                Message = string.Join(", ", identityCreate.Errors.Select(e => e.Description))
            };
        }

        if (!await roleManager.RoleExistsAsync("Customer"))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>("Customer"));
        }

        var roleResult = await userManager.AddToRoleAsync(identityUser, "Customer");
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(identityUser);
            return new AuthOperationResult<string>
            {
                Succeeded = false,
                Message = string.Join(", ", roleResult.Errors.Select(e => e.Description))
            };
        }

        var domainUser = new AppUser
        {
            Id = identityUser.Id,
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = identityUser.PasswordHash ?? string.Empty,
            Role = UserRole.Customer,
            IsActive = true
        };

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = domainUser.Id,
            RegisteredByStaffId = staffUserId,
            RegisteredAt = DateTime.UtcNow
        };

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            VehicleNumber = request.Vehicle.VehicleNumber,
            Make = request.Vehicle.Make,
            Model = request.Vehicle.Model,
            Year = request.Vehicle.Year,
            Vin = request.Vehicle.Vin,
            Mileage = request.Vehicle.Mileage
        };

        imsDbContext.AppUsers.Add(domainUser);
        imsDbContext.Customers.Add(customer);
        imsDbContext.Vehicles.Add(vehicle);
        await imsDbContext.SaveChangesAsync();

        return new AuthOperationResult<string>
        {
            Succeeded = true,
            Message = "Customer and vehicle registered successfully.",
            Data = customer.Id.ToString()
        };
    }

    public async Task<AuthOperationResult<CustomerProfileDto>> GetCustomerProfileAsync(Guid userId)
    {
        var customer = await imsDbContext.Customers
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (customer is null)
        {
            return new AuthOperationResult<CustomerProfileDto> { Succeeded = false, Message = "Customer profile not found." };
        }

        return new AuthOperationResult<CustomerProfileDto>
        {
            Succeeded = true,
            Message = "Profile retrieved.",
            Data = new CustomerProfileDto
            {
                CustomerId = customer.Id,
                UserId = customer.UserId,
                FullName = customer.User.FullName,
                Email = customer.User.Email,
                PhoneNumber = customer.User.PhoneNumber,
                OutstandingCredit = customer.OutstandingCredit,
                TotalSpend = customer.TotalSpend
            }
        };
    }

    public async Task<AuthOperationResult<string>> UpdateCustomerProfileAsync(Guid userId, UpdateCustomerProfileDto request)
    {
        var domainUser = await imsDbContext.AppUsers.FirstOrDefaultAsync(x => x.Id == userId && x.Role == UserRole.Customer);
        var identityUser = await userManager.FindByIdAsync(userId.ToString());

        if (domainUser is null || identityUser is null)
        {
            return new AuthOperationResult<string> { Succeeded = false, Message = "Customer profile not found." };
        }

        domainUser.FullName = request.FullName;
        domainUser.PhoneNumber = request.PhoneNumber;

        identityUser.FullName = request.FullName;
        identityUser.PhoneNumber = request.PhoneNumber;

        var identityUpdate = await userManager.UpdateAsync(identityUser);
        if (!identityUpdate.Succeeded)
        {
            return new AuthOperationResult<string>
            {
                Succeeded = false,
                Message = string.Join(", ", identityUpdate.Errors.Select(e => e.Description))
            };
        }

        await imsDbContext.SaveChangesAsync();
        return new AuthOperationResult<string> { Succeeded = true, Message = "Profile updated." };
    }

    public async Task<AuthOperationResult<VehicleDto>> AddVehicleAsync(Guid userId, VehicleUpsertDto request)
    {
        var customer = await imsDbContext.Customers.FirstOrDefaultAsync(x => x.UserId == userId);
        if (customer is null)
        {
            return new AuthOperationResult<VehicleDto> { Succeeded = false, Message = "Customer profile not found." };
        }

        if (await imsDbContext.Vehicles.AnyAsync(x => x.VehicleNumber == request.VehicleNumber))
        {
            return new AuthOperationResult<VehicleDto> { Succeeded = false, Message = "Vehicle number already exists." };
        }

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            VehicleNumber = request.VehicleNumber,
            Make = request.Make,
            Model = request.Model,
            Year = request.Year,
            Vin = request.Vin,
            Mileage = request.Mileage
        };

        imsDbContext.Vehicles.Add(vehicle);
        await imsDbContext.SaveChangesAsync();

        return new AuthOperationResult<VehicleDto>
        {
            Succeeded = true,
            Message = "Vehicle added.",
            Data = MapVehicle(vehicle)
        };
    }

    public async Task<AuthOperationResult<string>> UpdateVehicleAsync(Guid userId, Guid vehicleId, VehicleUpsertDto request)
    {
        var customer = await imsDbContext.Customers.FirstOrDefaultAsync(x => x.UserId == userId);
        if (customer is null)
        {
            return new AuthOperationResult<string> { Succeeded = false, Message = "Customer profile not found." };
        }

        var vehicle = await imsDbContext.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicleId && x.CustomerId == customer.Id);
        if (vehicle is null)
        {
            return new AuthOperationResult<string> { Succeeded = false, Message = "Vehicle not found." };
        }

        if (!vehicle.VehicleNumber.Equals(request.VehicleNumber, StringComparison.OrdinalIgnoreCase) &&
            await imsDbContext.Vehicles.AnyAsync(x => x.VehicleNumber == request.VehicleNumber))
        {
            return new AuthOperationResult<string> { Succeeded = false, Message = "Vehicle number already exists." };
        }

        vehicle.VehicleNumber = request.VehicleNumber;
        vehicle.Make = request.Make;
        vehicle.Model = request.Model;
        vehicle.Year = request.Year;
        vehicle.Vin = request.Vin;
        vehicle.Mileage = request.Mileage;

        await imsDbContext.SaveChangesAsync();
        return new AuthOperationResult<string> { Succeeded = true, Message = "Vehicle updated." };
    }

    public async Task<IReadOnlyCollection<VehicleDto>> GetVehiclesAsync(Guid userId)
    {
        var customer = await imsDbContext.Customers.FirstOrDefaultAsync(x => x.UserId == userId);
        if (customer is null)
        {
            return [];
        }

        return await imsDbContext.Vehicles
            .Where(x => x.CustomerId == customer.Id)
            .Select(x => new VehicleDto
            {
                Id = x.Id,
                VehicleNumber = x.VehicleNumber,
                Make = x.Make,
                Model = x.Model,
                Year = x.Year,
                Vin = x.Vin,
                Mileage = x.Mileage
            })
            .ToListAsync();
    }

    private static VehicleDto MapVehicle(Vehicle vehicle) => new()
    {
        Id = vehicle.Id,
        VehicleNumber = vehicle.VehicleNumber,
        Make = vehicle.Make,
        Model = vehicle.Model,
        Year = vehicle.Year,
        Vin = vehicle.Vin,
        Mileage = vehicle.Mileage
    };
}
