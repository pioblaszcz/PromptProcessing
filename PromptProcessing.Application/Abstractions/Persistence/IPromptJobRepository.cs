using PromptProcessing.Domain.PromptJobs;

namespace PromptProcessing.Application.Abstractions.Persistence;

public interface IPromptJobRepository
{
    Task AddRangeAsync(IEnumerable<PromptJob> promptJobs, CancellationToken cancellationToken);
    Task<PromptJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PromptJob>> GetAllAsync(CancellationToken cancellationToken);
}
