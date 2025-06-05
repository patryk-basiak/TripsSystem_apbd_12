using Microsoft.EntityFrameworkCore;
using TripsSystem.DTOs;
using TripsSystem.Models;

namespace TripsSystem.Services;

public class TripService : ITripService
{
    private readonly DatabaseContext _context;

    public TripService(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<TripsResponseDto> GetTrips(int page, int pageSize)
    {
        int totalCount = await _context.Trips.CountAsync();
        int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var tripsList = await _context.Trips
            .Include(t => t.ClientTrips)
            .ThenInclude(ct => ct.IdClientNavigation)
            .Include(t => t.IdCountries)
            .OrderByDescending(t => t.DateFrom)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var tripDtos = tripsList.Select(t => new TripDto
        {
            Name = t.Name,
            Description = t.Description,
            DateFrom = t.DateFrom,
            DateTo = t.DateTo,
            MaxPeople = t.MaxPeople,
            Countries = t.IdCountries
                .Select(c => new CountryDto { Name = c.Name })
                .ToList(),
            Clients = t.ClientTrips
                .Select(ct => new ClientDtOs
                {
                    FirstName = ct.IdClientNavigation.FirstName,
                    LastName = ct.IdClientNavigation.LastName
                })
                .ToList()
        }).ToList();

        return new TripsResponseDto
        {
            PageNum = page,
            PageSize = pageSize,
            AllPages = totalPages,
            Trips = tripDtos
        };
    }

    public async Task AssignClientToTrip(int idTrip, AssignClientDTO dto)
    {
        var trip = await _context.Trips.FindAsync(idTrip)
                   ?? throw new InvalidOperationException("Trip not found");

        if (trip.DateFrom <= DateTime.Now)
            throw new InvalidOperationException("Cannot sign up for a trip that already started");

        var client = await _context.Clients
            .Include(c => c.ClientTrips)
            .FirstOrDefaultAsync(c => c.Pesel == dto.Pesel);

        if (client != null)
        {
            bool isAlreadyAssigned = client.ClientTrips
                .Any(ct => ct.IdTrip == idTrip);

            if (isAlreadyAssigned)
                throw new InvalidOperationException("Client is already assigned to this trip");
        }
        else
        {
            client = new Client
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Telephone = dto.Telephone,
                Pesel = dto.Pesel
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
        }

        var assignment = new ClientTrip
        {
            IdClient = client.IdClient,
            IdTrip = idTrip,
            RegisteredAt = DateTime.Now,
            PaymentDate = dto.PaymentDate
        };

        _context.ClientTrips.Add(assignment);
        await _context.SaveChangesAsync();
    }

    public async Task<object?> GetAllTrips()
    {

        var tripsList = await _context.Trips
            .Include(t => t.ClientTrips)
            .ThenInclude(ct => ct.IdClientNavigation)
            .Include(t => t.IdCountries)
            .OrderByDescending(t => t.DateFrom)
            .ToListAsync();

        var tripDtos = tripsList.Select(t => new TripDto
        {
            Name = t.Name,
            Description = t.Description,
            DateFrom = t.DateFrom,
            DateTo = t.DateTo,
            MaxPeople = t.MaxPeople,
            Countries = t.IdCountries
                .Select(c => new CountryDto { Name = c.Name })
                .ToList(),
            Clients = t.ClientTrips
                .Select(ct => new ClientDtOs
                {
                    FirstName = ct.IdClientNavigation.FirstName,
                    LastName = ct.IdClientNavigation.LastName
                })
                .ToList()
        }).ToList();

        return new TripsResponseDto
        {
            Trips = tripDtos
        };

    }
}
