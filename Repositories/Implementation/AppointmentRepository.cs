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

        var appointment = new ServiceAppointment
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            AppointmentDate = request.AppointmentDate,
            ServiceType = request.ServiceType.Trim(),
            Notes = request.Notes,
            Status = AppointmentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        imsDbContext.ServiceAppointments.Add(appointment);
        await imsDbContext.SaveChangesAsync();

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
}
