using FleetManagement.Application.DTOs.Fuel;
using FleetManagement.Application.Interfaces;
using FleetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FleetManagement.Application.Services;

public class FuelService : IFuelService
{
    private readonly IFleetDbContext _context;

    public FuelService(IFleetDbContext context) => _context = context;

    public async Task<IEnumerable<FuelRecordDto>> GetAllAsync()
    {
        var records = await _context.FuelRecords
            .Include(f => f.Vehicle)
            .Include(f => f.Driver)
            .OrderByDescending(f => f.FuelDate)
            .ToListAsync();

        return records.Select(MapToDto);
    }

    public async Task<IEnumerable<FuelRecordDto>> GetByVehicleAsync(Guid vehicleId)
    {
        var records = await _context.FuelRecords
            .Include(f => f.Vehicle)
            .Include(f => f.Driver)
            .Where(f => f.VehicleId == vehicleId)
            .OrderByDescending(f => f.FuelDate)
            .ToListAsync();

        return records.Select(MapToDto);
    }

    public async Task<FuelRecordDto> GetByIdAsync(Guid id)
    {
        var record = await _context.FuelRecords
            .Include(f => f.Vehicle)
            .Include(f => f.Driver)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new Exception($"Fuel record {id} not found.");

        return MapToDto(record);
    }

    public async Task<FuelRecordDto> CreateAsync(CreateFuelRecordRequest request)
    {
        var record = new FuelRecord
        {
            VehicleId      = request.VehicleId,
            DriverId       = request.DriverId,
            FuelDate       = request.FuelDate,
            Litres         = request.Litres,
            CostPerLitre   = request.CostPerLitre,
            OdometerReading = request.OdometerReading,
            FuelType       = request.FuelType,
            Station        = request.Station,
            Notes          = request.Notes
        };

        _context.FuelRecords.Add(record);
        await _context.SaveChangesAsync();

        // Update vehicle mileage if the new odometer reading is higher
        var vehicle = await _context.Vehicles.FindAsync(request.VehicleId);
        if (vehicle != null && request.OdometerReading > vehicle.Mileage)
        {
            vehicle.Mileage = request.OdometerReading;
            await _context.SaveChangesAsync();
        }

        return await GetByIdAsync(record.Id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var record = await _context.FuelRecords.FindAsync(id)
            ?? throw new Exception($"Fuel record {id} not found.");

        record.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    public async Task<FuelEfficiencyDto> GetEfficiencyAsync(Guid vehicleId)
    {
        var records = await _context.FuelRecords
            .Include(f => f.Vehicle)
            .Where(f => f.VehicleId == vehicleId && !f.IsDeleted)
            .OrderBy(f => f.OdometerReading)
            .ToListAsync();

        var totalLitres    = records.Sum(r => r.Litres);
        var totalCost      = records.Sum(r => r.TotalCost);
        decimal kmPerLitre = 0;

        if (records.Count >= 2 && totalLitres > 0)
        {
            var distanceCovered = records.Last().OdometerReading - records.First().OdometerReading;
            kmPerLitre = distanceCovered / totalLitres;
        }

        return new FuelEfficiencyDto
        {
            VehicleId            = vehicleId,
            VehicleRegistration  = records.FirstOrDefault()?.Vehicle?.RegistrationNumber,
            AverageKmPerLitre    = kmPerLitre,
            TotalFuelCost        = totalCost,
            TotalLitres          = totalLitres
        };
    }

    private static FuelRecordDto MapToDto(FuelRecord f) => new()
    {
        Id                  = f.Id,
        VehicleId           = f.VehicleId,
        VehicleRegistration = f.Vehicle?.RegistrationNumber,
        DriverId            = f.DriverId,
        DriverName          = f.Driver != null ? $"{f.Driver.FirstName} {f.Driver.LastName}" : null,
        FuelDate            = f.FuelDate,
        Litres              = f.Litres,
        CostPerLitre        = f.CostPerLitre,
        TotalCost           = f.TotalCost,
        OdometerReading     = f.OdometerReading,
        FuelType            = f.FuelType.ToString(),
        Station             = f.Station,
        Notes               = f.Notes
    };
}
