using Microsoft.EntityFrameworkCore;
using PromptProcessing.Application.Abstractions.Persistence;
using PromptProcessing.Domain.PromptJobs;

namespace PromptProcessing.Infrastructure.Persistence.Repositories;

public class PromptJobRepository(ApplicationDbContext dbContext) : IPromptJobRepository
{
    public Task AddRangeAsync(IEnumerable<PromptJob> promptJobs, CancellationToken cancellationToken)
    {
        return dbContext.PromptJobs.AddRangeAsync(promptJobs, cancellationToken);
    }

    public Task<PromptJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.PromptJobs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<PromptJob>> GetPageAsync(int take, DateTime? createdBeforeUtc, Guid? id, CancellationToken cancellationToken)
    {
        var query = dbContext.PromptJobs.AsNoTracking();

        if (createdBeforeUtc.HasValue && id.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc < createdBeforeUtc.Value || (x.CreatedAtUtc == createdBeforeUtc.Value && x.Id.CompareTo(id.Value) < 0));
        }

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenByDescending(x => x.Id)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<PromptJob>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        return await dbContext.PromptJobs
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }
}
