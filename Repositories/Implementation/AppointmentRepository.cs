using IMS_API_.Data;
using IMS_API_.Models.Domains;
using IMS_API_.Models.DTO.Appointment;
using IMS_API_.Models.DTO.Auth;
using IMS_API_.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace IMS_API_.Repositories.Implementation;

public class AppointmentRepository(IMSDbContext imsDbContext) : IAppointmentRepository
{
    public async Task<AuthOperationResult<AppointmentDto>> CreateAppointmentAsync(Guid customerUserId, CreateAppointmentDto request)
    {
        var customer = await imsDbContext.Customers
            .Include(x => x.Vehicles)
            .FirstOrDefaultAsync(x => x.UserId == customerUserId);

        if (customer is null)
        {
            return new AuthOperationResult<AppointmentDto> { Succeeded = false, Message = "Customer profile not found." };
        }

        var vehicle = customer.Vehicles.FirstOrDefault(v => v.Id == request.VehicleId);
        if (vehicle is null)
        {
            return new AuthOperationResult<AppointmentDto> { Succeeded = false, Message = "Vehicle not found." };
        }

        if (request.AppointmentDate <= DateTime.UtcNow)
        {
            return new AuthOperationResult<AppointmentDto> { Succeeded = false, Message = "Appointment date must be in the future." };
        }

        // Ensure the appointment date is treated as UTC
        var appointmentDate = request.AppointmentDate.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(request.AppointmentDate, DateTimeKind.Utc) 
            : request.AppointmentDate.ToUniversalTime();

        var appointment = new ServiceAppointment
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            AppointmentDate = appointmentDate,
            ServiceType = request.ServiceType.Trim(),
            Notes = request.Notes,
            Status = AppointmentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        imsDbContext.ServiceAppointments.Add(appointment);
        
        try
        {
            await imsDbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return new AuthOperationResult<AppointmentDto> 
            { 
                Succeeded = false, 
                Message = $"Failed to save appointment: {ex.InnerException?.Message ?? ex.Message}" 
            };
        }
        catch (Exception ex)
        {
            return new AuthOperationResult<AppointmentDto> 
            { 
                Succeeded = false, 
                Message = $"An error occurred while booking the appointment: {ex.Message}" 
            };
        }

        return new AuthOperationResult<AppointmentDto>
        {
            Succeeded = true,
            Message = "Appointment booked.",
            Data = new AppointmentDto
            {
                Id = appointment.Id,
                VehicleId = appointment.VehicleId,
                VehicleNumber = vehicle.VehicleNumber,
                AppointmentDate = appointment.AppointmentDate,
                ServiceType = appointment.ServiceType,
                Notes = appointment.Notes,
                Status = appointment.Status,
                CreatedAt = appointment.CreatedAt
            }
        };
    }

    public async Task<IReadOnlyCollection<AppointmentDto>> GetMyAppointmentsAsync(Guid customerUserId)
    {
        var customerId = await imsDbContext.Customers
            .AsNoTracking()
            .Where(x => x.UserId == customerUserId)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();

        if (!customerId.HasValue)
        {
            return [];
        }

        return await imsDbContext.ServiceAppointments
            .AsNoTracking()
            .Include(x => x.Vehicle)
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.AppointmentDate)
            .Select(x => new AppointmentDto
            {
                Id = x.Id,
                VehicleId = x.VehicleId,
                VehicleNumber = x.Vehicle.VehicleNumber,
                AppointmentDate = x.AppointmentDate,
                ServiceType = x.ServiceType,
                Notes = x.Notes,
                Status = x.Status,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<AuthOperationResult<AppointmentDto>> UpdateAppointmentAsync(Guid customerUserId, Guid appointmentId, UpdateAppointmentDto request)
    {
        var customerId = await imsDbContext.Customers
            .AsNoTracking()
            .Where(x => x.UserId == customerUserId)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();

        if (!customerId.HasValue)
        {
            return new AuthOperationResult<AppointmentDto> { Succeeded = false, Message = "Customer profile not found." };
        }

        var appointment = await imsDbContext.ServiceAppointments
            .Include(x => x.Vehicle)
            .FirstOrDefaultAsync(x => x.Id == appointmentId && x.CustomerId == customerId);

        if (appointment is null)
        {
            return new AuthOperationResult<AppointmentDto> { Succeeded = false, Message = "Appointment not found." };
        }

        if (appointment.Status != AppointmentStatus.Pending)
        {
            return new AuthOperationResult<AppointmentDto> { Succeeded = false, Message = "Only pending appointments can be updated." };
        }

        var vehicle = await imsDbContext.Vehicles.FirstOrDefaultAsync(v => v.Id == request.VehicleId && v.CustomerId == customerId);
        if (vehicle is null)
        {
            return new AuthOperationResult<AppointmentDto> { Succeeded = false, Message = "Vehicle not found." };
        }

        if (request.AppointmentDate <= DateTime.UtcNow)
        {
            return new AuthOperationResult<AppointmentDto> { Succeeded = false, Message = "Appointment date must be in the future." };
        }

        var appointmentDate = request.AppointmentDate.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(request.AppointmentDate, DateTimeKind.Utc) 
            : request.AppointmentDate.ToUniversalTime();

        appointment.VehicleId = vehicle.Id;
        appointment.AppointmentDate = appointmentDate;
        appointment.ServiceType = request.ServiceType.Trim();
        appointment.Notes = request.Notes;

        try
        {
            await imsDbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return new AuthOperationResult<AppointmentDto> 
            { 
                Succeeded = false, 
                Message = $"Failed to update appointment: {ex.InnerException?.Message ?? ex.Message}" 
            };
        }
        catch (Exception ex)
        {
            return new AuthOperationResult<AppointmentDto> 
            { 
                Succeeded = false, 
                Message = $"An error occurred while updating the appointment: {ex.Message}" 
            };
        }

        return new AuthOperationResult<AppointmentDto>
        {
            Succeeded = true,
            Message = "Appointment updated.",
            Data = new AppointmentDto
            {
                Id = appointment.Id,
                VehicleId = appointment.VehicleId,
                VehicleNumber = vehicle.VehicleNumber,
                AppointmentDate = appointment.AppointmentDate,
                ServiceType = appointment.ServiceType,
                Notes = appointment.Notes,
                Status = appointment.Status,
                CreatedAt = appointment.CreatedAt
            }
        };
    }

    public async Task<AuthOperationResult<string>> DeleteAppointmentAsync(Guid customerUserId, Guid appointmentId)
    {
        var customerId = await imsDbContext.Customers
            .AsNoTracking()
            .Where(x => x.UserId == customerUserId)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();

        if (!customerId.HasValue)
        {
            return new AuthOperationResult<string> { Succeeded = false, Message = "Customer profile not found." };
        }

        var appointment = await imsDbContext.ServiceAppointments
            .FirstOrDefaultAsync(x => x.Id == appointmentId && x.CustomerId == customerId);

        if (appointment is null)
        {
            return new AuthOperationResult<string> { Succeeded = false, Message = "Appointment not found." };
        }

        if (appointment.Status != AppointmentStatus.Pending)
        {
            return new AuthOperationResult<string> { Succeeded = false, Message = "Only pending appointments can be cancelled." };
        }

        try
        {
            imsDbContext.ServiceAppointments.Remove(appointment);
            await imsDbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return new AuthOperationResult<string> 
            { 
                Succeeded = false, 
                Message = $"Failed to cancel appointment: {ex.InnerException?.Message ?? ex.Message}" 
            };
        }
        catch (Exception ex)
        {
            return new AuthOperationResult<string> 
            { 
                Succeeded = false, 
                Message = $"An error occurred while cancelling the appointment: {ex.Message}" 
            };
        }

        return new AuthOperationResult<string>
        {
            Succeeded = true,
            Message = "Appointment cancelled.",
            Data = "Appointment cancelled successfully."
        };
    }

    // Staff Methods
    public async Task<IReadOnlyCollection<StaffAppointmentDto>> GetAllAppointmentsAsync()
    {
        return await imsDbContext.ServiceAppointments
            .AsNoTracking()
            .Include(x => x.Customer.User)
            .Include(x => x.Vehicle)
            .OrderByDescending(x => x.AppointmentDate)
            .Select(x => new StaffAppointmentDto
            {
                Id = x.Id,
                CustomerId = x.CustomerId,
                CustomerName = x.Customer.User.FullName,
                CustomerEmail = x.Customer.User.Email,
                VehicleId = x.VehicleId,
                VehicleNumber = x.Vehicle.VehicleNumber,
                AppointmentDate = x.AppointmentDate,
                ServiceType = x.ServiceType,
                Notes = x.Notes,
                ServiceNotes = x.ServiceNotes,
                Status = x.Status,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<StaffAppointmentDto?> GetAppointmentByIdAsync(Guid appointmentId)
    {
        return await imsDbContext.ServiceAppointments
            .AsNoTracking()
            .Include(x => x.Customer.User)
            .Include(x => x.Vehicle)
            .Where(x => x.Id == appointmentId)
            .Select(x => new StaffAppointmentDto
            {
                Id = x.Id,
                CustomerId = x.CustomerId,
                CustomerName = x.Customer.User.FullName,
                CustomerEmail = x.Customer.User.Email,
                VehicleId = x.VehicleId,
                VehicleNumber = x.Vehicle.VehicleNumber,
                AppointmentDate = x.AppointmentDate,
                ServiceType = x.ServiceType,
                Notes = x.Notes,
                ServiceNotes = x.ServiceNotes,
                Status = x.Status,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<AuthOperationResult<StaffAppointmentDto>> UpdateAppointmentStatusAsync(Guid appointmentId, UpdateAppointmentStatusDto request)
    {
        var appointment = await imsDbContext.ServiceAppointments
            .Include(x => x.Customer.User)
            .Include(x => x.Vehicle)
            .FirstOrDefaultAsync(x => x.Id == appointmentId);

        if (appointment is null)
        {
            return new AuthOperationResult<StaffAppointmentDto> { Succeeded = false, Message = "Appointment not found." };
        }

        appointment.Status = request.Status;
        if (!string.IsNullOrWhiteSpace(request.ServiceNotes))
        {
            appointment.ServiceNotes = request.ServiceNotes;
        }

        try
        {
            await imsDbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return new AuthOperationResult<StaffAppointmentDto>
            {
                Succeeded = false,
                Message = $"Failed to update appointment: {ex.InnerException?.Message ?? ex.Message}"
            };
        }
        catch (Exception ex)
        {
            return new AuthOperationResult<StaffAppointmentDto>
            {
                Succeeded = false,
                Message = $"An error occurred while updating the appointment: {ex.Message}"
            };
        }

        return new AuthOperationResult<StaffAppointmentDto>
        {
            Succeeded = true,
            Message = "Appointment status updated.",
            Data = new StaffAppointmentDto
            {
                Id = appointment.Id,
                CustomerId = appointment.CustomerId,
                CustomerName = appointment.Customer.User.FullName,
                CustomerEmail = appointment.Customer.User.Email,
                VehicleId = appointment.VehicleId,
                VehicleNumber = appointment.Vehicle.VehicleNumber,
                AppointmentDate = appointment.AppointmentDate,
                ServiceType = appointment.ServiceType,
                Notes = appointment.Notes,
                ServiceNotes = appointment.ServiceNotes,
                Status = appointment.Status,
                CreatedAt = appointment.CreatedAt
            }
        };
    }

    public async Task<AuthOperationResult<string>> DeleteAppointmentAsync(Guid appointmentId)
    {
        var appointment = await imsDbContext.ServiceAppointments
            .FirstOrDefaultAsync(x => x.Id == appointmentId);

        if (appointment is null)
        {
            return new AuthOperationResult<string> { Succeeded = false, Message = "Appointment not found." };
        }

        try
        {
            imsDbContext.ServiceAppointments.Remove(appointment);
            await imsDbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return new AuthOperationResult<string>
            {
                Succeeded = false,
                Message = $"Failed to delete appointment: {ex.InnerException?.Message ?? ex.Message}"
            };
        }
        catch (Exception ex)
        {
            return new AuthOperationResult<string>
            {
                Succeeded = false,
                Message = $"An error occurred while deleting the appointment: {ex.Message}"
            };
        }

        return new AuthOperationResult<string>
        {
            Succeeded = true,
            Message = "Appointment deleted.",
            Data = "Appointment deleted successfully."
        };
    }
}
