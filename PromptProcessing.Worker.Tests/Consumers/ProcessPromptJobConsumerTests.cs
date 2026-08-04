using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PromptProcessing.Application.Abstractions.AI;
using PromptProcessing.Application.Abstractions.Persistence;
using PromptProcessing.Contracts.PromptJobs;
using PromptProcessing.Domain.PromptJobs;
using PromptProcessing.Worker.Consumers;
using Xunit;

namespace PromptProcessing.Worker.Tests.Consumers;

public class ProcessPromptJobConsumerTests
{
    [Fact]
    public async Task Consume_PendingJob_CompletesAndRegistersOneAttempt()
    {
        var fixture = new ConsumerFixture(PromptJob.Create("Explain queues."));
        fixture.TextGenerationService.GenerateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns("Generated result");

        await fixture.Consumer.Consume(fixture.Context);

        Assert.Equal(PromptJobStatus.Completed, fixture.Job.Status);
        Assert.Equal(1, fixture.Job.AttemptCount);
        await fixture.TextGenerationService.Received(1).GenerateAsync(fixture.Job.Content, Arg.Any<CancellationToken>());
        await fixture.UnitOfWork.Received(3).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_ProcessingJob_ResumesAndCompletes()
    {
        var job = PromptJob.Create("Explain queues.");
        job.StartProcessing();
        var fixture = new ConsumerFixture(job);
        fixture.TextGenerationService.GenerateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns("Generated result");

        await fixture.Consumer.Consume(fixture.Context);

        Assert.Equal(PromptJobStatus.Completed, fixture.Job.Status);
        Assert.Equal(1, fixture.Job.AttemptCount);
        await fixture.UnitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Consume_TerminalJob_DoesNotGenerate(bool completed)
    {
        var job = PromptJob.Create("Explain queues.");
        job.StartProcessing();
        if (completed)
            job.Complete("Existing result");
        else
            job.Fail("Existing error");
        var fixture = new ConsumerFixture(job);

        await fixture.Consumer.Consume(fixture.Context);

        await fixture.TextGenerationService.DidNotReceive().GenerateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await fixture.UnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_TransientErrorBeforeFinalRetry_PropagatesException()
    {
        var fixture = new ConsumerFixture(PromptJob.Create("Explain queues."));
        fixture.TextGenerationService.GenerateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<string>(new TransientTextGenerationException("Temporary outage", new HttpRequestException())));

        await Assert.ThrowsAsync<TransientTextGenerationException>(() => fixture.Consumer.Consume(fixture.Context));

        Assert.Equal(PromptJobStatus.Processing, fixture.Job.Status);
        Assert.Equal(1, fixture.Job.AttemptCount);
    }

    [Fact]
    public async Task Consume_NonTransientError_FailsJobWithoutRetry()
    {
        var fixture = new ConsumerFixture(PromptJob.Create("Explain queues."));
        fixture.TextGenerationService.GenerateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<string>(new InvalidOperationException("Invalid response")));

        await fixture.Consumer.Consume(fixture.Context);

        Assert.Equal(PromptJobStatus.Failed, fixture.Job.Status);
        Assert.Equal(1, fixture.Job.AttemptCount);
    }

    [Fact]
    public void RetryPolicy_UsesTwoRetriesAndMarksTheThirdInvocationAsFinal()
    {
        Assert.Equal(2, PromptGenerationRetryPolicy.RetryCount);
        Assert.False(PromptGenerationRetryPolicy.IsFinalAttempt(1));
        Assert.True(PromptGenerationRetryPolicy.IsFinalAttempt(2));
    }

    private sealed class ConsumerFixture
    {
        public ConsumerFixture(PromptJob job)
        {
            Job = job;
            PromptJobRepository = Substitute.For<IPromptJobRepository>();
            UnitOfWork = Substitute.For<IUnitOfWork>();
            TextGenerationService = Substitute.For<ITextGenerationService>();
            Context = Substitute.For<ConsumeContext<ProcessPromptJob>>();
            var message = new ProcessPromptJob(job.Id);
            Context.Message.Returns(message);
            Context.CancellationToken.Returns(CancellationToken.None);
            PromptJobRepository.GetByIdAsync(message.PromptJobId, Arg.Any<CancellationToken>()).Returns(Task.FromResult<PromptJob?>(job));
            UnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));
            Consumer = new ProcessPromptJobConsumer(PromptJobRepository, UnitOfWork, TextGenerationService, Substitute.For<ILogger<ProcessPromptJobConsumer>>());
        }

        public PromptJob Job { get; }
        public IPromptJobRepository PromptJobRepository { get; }
        public IUnitOfWork UnitOfWork { get; }
        public ITextGenerationService TextGenerationService { get; }
        public ConsumeContext<ProcessPromptJob> Context { get; }
        public ProcessPromptJobConsumer Consumer { get; }
    }
}
