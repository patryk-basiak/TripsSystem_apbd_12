using System.Data.Entity;

namespace TripsSystem.Services;

public class ClientService : IClientService
{
    private readonly DatabaseContext _context;
    
    public ClientService(DatabaseContext context)
    {
        _context = context;
    }
    public async Task<bool> DeleteClient(int idClient)
    {
        var client = await _context.Clients
            .Where(c => c.IdClient == idClient)
            .Include(c => c.ClientTrips)
            .SingleOrDefaultAsync();

        if (client is null)
            throw new InvalidOperationException("Client not found");

        if (client.ClientTrips.Count > 0)
            throw new InvalidOperationException("Cannot delete client who is assigned to trips");

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();

        return true;
    }
}