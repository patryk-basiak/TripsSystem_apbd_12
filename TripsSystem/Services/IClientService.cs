namespace TripsSystem.Services;

public interface IClientService
{
    Task<bool> DeleteClient(int idClient);
}