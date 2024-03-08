namespace CongressDataCollector.Core.Interfaces;

public interface IOpenAiService
{
    Task<string> AnalyzeBillTextAsync(string billText);
}