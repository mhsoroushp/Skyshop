namespace Core.Interfaces;

public interface IAppLogger<T>
{
    void LogInformation(string messageTemplate, params object?[] args);
    void LogWarning(string messageTemplate, params object?[] args);
    void LogError(Exception exception, string messageTemplate, params object?[] args);
}