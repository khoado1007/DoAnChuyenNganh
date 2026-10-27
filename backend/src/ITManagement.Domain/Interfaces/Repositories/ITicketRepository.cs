using ITManagement.Domain.Entities.Ticket;
using ITManagement.Domain.Enums;

namespace ITManagement.Domain.Interfaces.Repositories;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Ticket?> GetByNumberAsync(string ticketNumber, CancellationToken ct = default);
    Task<IEnumerable<Ticket>> GetByCreatorAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<Ticket>> GetByAssigneeAsync(Guid technicianId, CancellationToken ct = default);
    Task<IEnumerable<Ticket>> GetByStatusAsync(TicketStatus status, CancellationToken ct = default);
    Task<IEnumerable<Ticket>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Ticket ticket, CancellationToken ct = default);
    Task UpdateAsync(Ticket ticket, CancellationToken ct = default);
    Task<int> GetNextSequenceAsync(int year, CancellationToken ct = default);
}
