using ITManagement.Application.Contracts.Agents;
namespace ITManagement.Application.Services;

public interface IAgentService
{
    Task<EnrollmentIssued> IssueAsync(IssueEnrollmentRequest request, CancellationToken ct);
    Task<EnrollmentIssued> IssueRecoveryAsync(IssueRecoveryRequest request, CancellationToken ct);
    Task<AgentEnrolled> EnrollAsync(EnrollRequest request, CancellationToken ct);
    Task<HeartbeatAccepted> HeartbeatAsync(string credential, HeartbeatRequest request, CancellationToken ct);
    Task<IReadOnlyList<AgentResponse>> ListAsync(CancellationToken ct);
    Task DisableAsync(Guid id, CancellationToken ct);
}
