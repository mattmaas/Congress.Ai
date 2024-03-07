using CongressDataCollector.Core.Models;

namespace CongressDataCollector.Core.Interfaces;

public interface IStateService
{
    Task SaveStateAsync(FetchState state);
    Task<FetchState?> LoadStateAsync();
}