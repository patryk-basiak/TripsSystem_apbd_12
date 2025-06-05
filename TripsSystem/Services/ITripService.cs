using TripsSystem.DTOs;

namespace TripsSystem.Services;

public interface ITripService
{
    Task<TripsResponseDto> GetTrips(int page, int pageSize);
    Task AssignClientToTrip(int idTrip, AssignClientDTO dto);
    Task<object?> GetAllTrips();
}