using System.Security.Cryptography;
using System.Text;
using ITManagement.Application.Common.Exceptions;
using ITManagement.Application.Contracts.Agents;
using ITManagement.Application.Services;
using ITManagement.Domain.Entities.Agents;
using ITManagement.Domain.Entities.Asset;
using ITManagement.Domain.Enums;
using ITManagement.Domain.Exceptions;
using ITManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ITManagement.Infrastructure.Services;

public sealed class AgentService(AppDbContext db, TimeProvider clock) : IAgentService
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;
    private static string Secret() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    private static string Hash(string secret) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(secret)));
    private static AuthenticationRequiredException Invalid() => new("Thông tin đăng ký hoặc credential Agent không hợp lệ.");

    private async Task<Asset> LockAsset(Guid id, CancellationToken ct)
    {
        var asset = await db.Assets.FromSqlInterpolated($"SELECT * FROM assets WHERE \"Id\" = {id} FOR UPDATE").SingleOrDefaultAsync(ct);
        if (asset is null) throw new NotFoundException("Asset", id);
        if (asset.Status is AssetStatus.Disabled or AssetStatus.Retired)
            throw new UnauthorizedException("Tài sản đã vô hiệu hóa hoặc ngừng sử dụng.");
        return asset;
    }

    public async Task<EnrollmentIssued> IssueAsync(IssueEnrollmentRequest request, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await LockAsset(request.AssetId, ct);
        if (await db.Agents.AnyAsync(a => a.AssetId == request.AssetId, ct))
            throw new BusinessRuleException("Tài sản đã có Agent; không tự động thay credential.");
        var now = Now;
        await db.EnrollmentTokens.Where(t => t.AssetId == request.AssetId && t.ConsumedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.ExpiresAt, now), ct);
        var secret = Secret();
        var token = new EnrollmentToken { AssetId = request.AssetId, Mode = request.Mode, TokenHash = Hash(secret), ExpiresAt = now.AddMinutes(15) };
        db.EnrollmentTokens.Add(token);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return new(secret, token.ExpiresAt);
    }

    public async Task<AgentEnrolled> EnrollAsync(EnrollRequest request, CancellationToken ct)
    {
        if (request.InstanceId == Guid.Empty) throw new BusinessRuleException("Instance ID không hợp lệ.");
        var hash = Hash(request.Token);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var token = await db.EnrollmentTokens.AsNoTracking().SingleOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (token is null) throw Invalid();
        await LockAsset(token.AssetId, ct);
        var now = Now;
        var consumed = await db.EnrollmentTokens.Where(t => t.Id == token.Id && t.ConsumedAt == null && t.ExpiresAt > now && t.Mode == request.Mode)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.ConsumedAt, now), ct);
        if (consumed != 1) throw Invalid();
        if (await db.Agents.AnyAsync(a => a.AssetId == token.AssetId || a.InstanceId == request.InstanceId, ct))
            throw new BusinessRuleException("Tài sản hoặc instance đã được đăng ký.");
        var credential = Secret();
        var agent = new RegisteredAgent { AssetId = token.AssetId, InstanceId = request.InstanceId, Mode = token.Mode, CredentialHash = Hash(credential), CreatedAt = now };
        db.Agents.Add(agent);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        { throw new BusinessRuleException("Tài sản hoặc instance đã được đăng ký."); }
        await tx.CommitAsync(ct);
        return new(agent.Id, credential);
    }

    public async Task<HeartbeatAccepted> HeartbeatAsync(string credential, HeartbeatRequest request, CancellationToken ct)
    {
        if (credential.Length != 64) throw Invalid();
        var hash = Hash(credential);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var agent = await db.Agents.FromSqlInterpolated($"SELECT * FROM agents WHERE \"CredentialHash\" = {hash} FOR UPDATE").SingleOrDefaultAsync(ct);
        if (agent is null) throw Invalid();
        if (agent.DisabledAt is not null) throw new UnauthorizedException("Agent đã bị vô hiệu hóa.");
        if (!await db.Assets.AnyAsync(a => a.Id == agent.AssetId && a.Status != AssetStatus.Disabled && a.Status != AssetStatus.Retired, ct))
            throw new UnauthorizedException("Tài sản không cho phép heartbeat.");
        // Replays do not advance last-seen or overwrite a newer inventory snapshot.
        if (request.Sequence > agent.LastSequence)
        {
            agent.LastSequence = request.Sequence;
            agent.LastSeenAt = Now;
            agent.Hostname = request.Hostname;
            agent.Os = request.Os;
            agent.Cpu = request.Cpu;
            agent.RamBytes = request.RamBytes;
            agent.AgentVersion = request.AgentVersion;
            await db.SaveChangesAsync(ct);
        }
        await tx.CommitAsync(ct);
        return new(agent.LastSeenAt, agent.LastSequence);
    }

    public async Task<IReadOnlyList<AgentResponse>> ListAsync(CancellationToken ct)
    {
        var threshold = Now.AddSeconds(-90);
        return await (from a in db.Agents.AsNoTracking()
                      join asset in db.Assets.IgnoreQueryFilters() on a.AssetId equals asset.Id
                      orderby a.CreatedAt descending
                      select new AgentResponse(a.Id, a.AssetId, a.InstanceId, asset.AssetCode, asset.Status.ToString(), a.Mode,
                          a.DisabledAt == null ? "active" : "disabled",
                          a.LastSeenAt != null && a.LastSeenAt > threshold ? "online" : "offline", a.LastSeenAt,
                          a.Hostname, a.Os, a.Cpu, a.RamBytes, a.AgentVersion)).ToListAsync(ct);
    }

    public async Task DisableAsync(Guid id, CancellationToken ct)
    {
        var now = Now;
        // UPDATE takes the same row lock as heartbeat; no heartbeat can commit after disable completes.
        var count = await db.Agents.Where(a => a.Id == id).ExecuteUpdateAsync(s => s.SetProperty(a => a.DisabledAt, a => a.DisabledAt ?? now), ct);
        if (count == 0) throw new NotFoundException("Agent", id);
    }

    public async Task<AgentAssetResponse> CreateAssetAsync(CreateAgentAssetRequest request, CancellationToken ct)
    {
        var asset = new Asset { AssetCode = request.AssetCode.Trim(), Name = request.Name.Trim(), Type = AssetType.Desktop };
        db.Assets.Add(asset);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        { throw new BusinessRuleException("Mã tài sản đã tồn tại."); }
        return new(asset.Id, asset.AssetCode, asset.Name, asset.Status.ToString());
    }

    public async Task<IReadOnlyList<AgentAssetResponse>> ListAssetsAsync(CancellationToken ct) =>
        await db.Assets.AsNoTracking().OrderBy(a => a.AssetCode)
            .Select(a => new AgentAssetResponse(a.Id, a.AssetCode, a.Name, a.Status.ToString())).ToListAsync(ct);
}
