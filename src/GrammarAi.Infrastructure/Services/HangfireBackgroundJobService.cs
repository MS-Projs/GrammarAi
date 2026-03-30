using GrammarAi.Application.Common.Interfaces;
using GrammarAi.Infrastructure.Workers;
using Hangfire;

namespace GrammarAi.Infrastructure.Services;

public class HangfireBackgroundJobService : IBackgroundJobService
{
    public string EnqueueOcrJob(Guid exerciseId)
    {
        return BackgroundJob.Enqueue<OcrWorker>(w => w.ProcessExerciseAsync(exerciseId, CancellationToken.None));
    }
}
