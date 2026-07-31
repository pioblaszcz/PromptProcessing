using PromptProcessing.Application.Abstractions.Messaging;
using PromptProcessing.Application.Abstractions.Persistence;
using PromptProcessing.Application.PromptJobs.CreatePromptJobs;
using PromptProcessing.Domain.PromptJobs;

namespace PromptProcessing.Application.Tests.PromptJobs.CreatePromptJobs;

public class CreatePromptJobsCommandHandlerTests
{
    [Fact]
    public async Task Handle_MultiplePrompts_PersistsCreatedJobs()
    {
        var repository = Substitute.For<IPromptJobRepository>();
        var queue = Substitute.For<IPromptJobQueue>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var persistedJobs = new List<PromptJob>();
        repository.AddRangeAsync(Arg.Do<IEnumerable<PromptJob>>(jobs => persistedJobs = jobs.ToList()), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        queue.EnqueueAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));
        var handler = new CreatePromptJobsCommandHandler(repository, queue, unitOfWork);

        await handler.Handle(new CreatePromptJobsCommand(new[] { "First prompt", "Second prompt" }), CancellationToken.None);

        Assert.Collection(persistedJobs,
            first => Assert.Equal("First prompt", first.Content),
            second => Assert.Equal("Second prompt", second.Content));
        Assert.All(persistedJobs, job => Assert.Equal(PromptJobStatus.Pending, job.Status));
    }

    [Fact]
    public async Task Handle_MultiplePrompts_EnqueuesMatchingJobIds()
    {
        var repository = Substitute.For<IPromptJobRepository>();
        var queue = Substitute.For<IPromptJobQueue>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var persistedJobs = new List<PromptJob>();
        var publishedIds = new List<Guid>();
        repository.AddRangeAsync(Arg.Do<IEnumerable<PromptJob>>(jobs => persistedJobs = jobs.ToList()), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        queue.EnqueueAsync(Arg.Do<Guid>(id => publishedIds.Add(id)), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));
        var handler = new CreatePromptJobsCommandHandler(repository, queue, unitOfWork);

        await handler.Handle(new CreatePromptJobsCommand(new[] { "First prompt", "Second prompt" }), CancellationToken.None);

        Assert.Equal(persistedJobs.Select(job => job.Id), publishedIds);
    }

    [Fact]
    public async Task Handle_ValidPrompts_PassesCancellationTokenToDependencies()
    {
        var repository = Substitute.For<IPromptJobRepository>();
        var queue = Substitute.For<IPromptJobQueue>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var cancellationToken = new CancellationTokenSource().Token;
        repository.AddRangeAsync(Arg.Any<IEnumerable<PromptJob>>(), cancellationToken).Returns(Task.CompletedTask);
        queue.EnqueueAsync(Arg.Any<Guid>(), cancellationToken).Returns(Task.CompletedTask);
        unitOfWork.SaveChangesAsync(cancellationToken).Returns(Task.FromResult(1));
        var handler = new CreatePromptJobsCommandHandler(repository, queue, unitOfWork);

        await handler.Handle(new CreatePromptJobsCommand(new[] { "First prompt", "Second prompt" }), cancellationToken);

        await repository.Received(1).AddRangeAsync(Arg.Any<IEnumerable<PromptJob>>(), cancellationToken);
        await queue.Received(2).EnqueueAsync(Arg.Any<Guid>(), cancellationToken);
        await unitOfWork.Received(1).SaveChangesAsync(cancellationToken);
    }

    [Fact]
    public async Task Handle_MultiplePrompts_ReturnsDtosInInputOrder()
    {
        var repository = Substitute.For<IPromptJobRepository>();
        var queue = Substitute.For<IPromptJobQueue>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        repository.AddRangeAsync(Arg.Any<IEnumerable<PromptJob>>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        queue.EnqueueAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));
        var handler = new CreatePromptJobsCommandHandler(repository, queue, unitOfWork);

        var result = await handler.Handle(new CreatePromptJobsCommand(new[] { "First prompt", "Second prompt" }), CancellationToken.None);

        Assert.Collection(result,
            first => Assert.Equal("First prompt", first.Content),
            second => Assert.Equal("Second prompt", second.Content));
        Assert.All(result, item => Assert.NotEqual(Guid.Empty, item.Id));
    }

    [Fact]
    public async Task Handle_RepositoryThrows_DoesNotEnqueueOrSaveChanges()
    {
        var repository = Substitute.For<IPromptJobRepository>();
        var queue = Substitute.For<IPromptJobQueue>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        repository.AddRangeAsync(Arg.Any<IEnumerable<PromptJob>>(), Arg.Any<CancellationToken>()).Returns(_ => throw new InvalidOperationException());
        var handler = new CreatePromptJobsCommandHandler(repository, queue, unitOfWork);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new CreatePromptJobsCommand(new[] { "First prompt" }), CancellationToken.None));

        await queue.DidNotReceiveWithAnyArgs().EnqueueAsync(default, default);
        await unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_EnqueueThrows_DoesNotSaveChangesOrEnqueueRemainingJobs()
    {
        var repository = Substitute.For<IPromptJobRepository>();
        var queue = Substitute.For<IPromptJobQueue>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        repository.AddRangeAsync(Arg.Any<IEnumerable<PromptJob>>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        queue.EnqueueAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(_ => throw new InvalidOperationException());
        var handler = new CreatePromptJobsCommandHandler(repository, queue, unitOfWork);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new CreatePromptJobsCommand(new[] { "First prompt", "Second prompt" }), CancellationToken.None));

        await queue.Received(1).EnqueueAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }
}
