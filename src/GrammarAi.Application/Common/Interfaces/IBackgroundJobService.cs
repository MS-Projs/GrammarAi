namespace GrammarAi.Application.Common.Interfaces;

public interface IBackgroundJobService
{
    string EnqueueOcrJob(Guid exerciseId);
}
