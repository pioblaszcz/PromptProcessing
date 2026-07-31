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

    public async Task<IReadOnlyCollection<PromptJob>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.PromptJobs
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
