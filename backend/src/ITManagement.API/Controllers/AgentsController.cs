using ITManagement.API.Responses;
using ITManagement.Application.Common.Exceptions;
using ITManagement.Application.Contracts.Agents;
using ITManagement.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ITManagement.API.Controllers;

[ApiController]
[Route("api/agents")]
[RequestSizeLimit(8192)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class AgentsController(IAgentService agents) : ControllerBase
{
    [Authorize(Policy = "AdminOnly")]
    [HttpPost("enrollment-tokens")]
    public async Task<ActionResult<ApiResponse<EnrollmentIssued>>> Issue(IssueEnrollmentRequest request, CancellationToken ct)
        => Ok(ApiResponse<EnrollmentIssued>.Ok(await agents.IssueAsync(request, ct)));

    [Authorize(Policy = "AdminOnly")]
    [HttpPost("recovery-tokens")]
    public async Task<ActionResult<ApiResponse<EnrollmentIssued>>> IssueRecovery(IssueRecoveryRequest request, CancellationToken ct)
        => Ok(ApiResponse<EnrollmentIssued>.Ok(await agents.IssueRecoveryAsync(request, ct)));

    [AllowAnonymous, EnableRateLimiting("agent-enrollment")]
    [HttpPost("enroll")]
    public async Task<ActionResult<ApiResponse<AgentEnrolled>>> Enroll(EnrollRequest request, CancellationToken ct)
        => Ok(ApiResponse<AgentEnrolled>.Ok(await agents.EnrollAsync(request, ct)));

    [AllowAnonymous, EnableRateLimiting("agent-heartbeat")]
    [HttpPost("heartbeat")]
    public async Task<ActionResult<ApiResponse<HeartbeatAccepted>>> Heartbeat(HeartbeatRequest request, CancellationToken ct)
    {
        var header = Request.Headers.Authorization.ToString();
        if (!header.StartsWith("Agent ", StringComparison.OrdinalIgnoreCase))
            throw new AuthenticationRequiredException("Cần credential Agent.");
        return Ok(ApiResponse<HeartbeatAccepted>.Ok(await agents.HeartbeatAsync(header[6..], request, ct)));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AgentResponse>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<AgentResponse>>.Ok(await agents.ListAsync(ct)));

    [Authorize(Policy = "AdminOnly")]
    [HttpPost("{id:guid}/disable")]
    public async Task<ActionResult<ApiResponse<bool>>> Disable(Guid id, CancellationToken ct)
    {
        await agents.DisableAsync(id, ct);
        return Ok(ApiResponse<bool>.Ok(true));
    }
}
