using Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Infrastructure.Services;

public class AppLogger<T>(ILogger<T> logger) : IAppLogger<T>
{
    private readonly ILogger<T> _logger = logger;
    private static readonly EventId InformationEventId = new(1000, nameof(LogInformation));
    private static readonly EventId WarningEventId = new(1001, nameof(LogWarning));
    private static readonly EventId ErrorEventId = new(1002, nameof(LogError));

    public void LogInformation(string messageTemplate, params object?[] args)
    {
        var message = RenderMessage(messageTemplate, args);
        _logger.Log(LogLevel.Information, InformationEventId, message, null, static (state, _) => state);
    }

    public void LogWarning(string messageTemplate, params object?[] args)
    {
        var message = RenderMessage(messageTemplate, args);
        _logger.Log(LogLevel.Warning, WarningEventId, message, null, static (state, _) => state);
    }

    public void LogError(Exception exception, string messageTemplate, params object?[] args)
    {
        var message = RenderMessage(messageTemplate, args);
        _logger.Log(LogLevel.Error, ErrorEventId, message, exception, static (state, _) => state);
    }

    private static string RenderMessage(string messageTemplate, object?[] args)
    {
        if (args.Length == 0)
        {
            return messageTemplate;
        }

        var builder = new StringBuilder(messageTemplate.Length + (args.Length * 8));
        var argIndex = 0;

        for (var i = 0; i < messageTemplate.Length; i++)
        {
            var current = messageTemplate[i];

            if (current == '{')
            {
                if (i + 1 < messageTemplate.Length && messageTemplate[i + 1] == '{')
                {
                    builder.Append('{');
                    i++;
                    continue;
                }

                var end = messageTemplate.IndexOf('}', i + 1);
                if (end > i)
                {
                    if (argIndex < args.Length)
                    {
                        builder.Append(args[argIndex]);
                        argIndex++;
                    }
                    else
                    {
                        builder.Append(messageTemplate, i, end - i + 1);
                    }

                    i = end;
                    continue;
                }
            }

            if (current == '}' && i + 1 < messageTemplate.Length && messageTemplate[i + 1] == '}')
            {
                builder.Append('}');
                i++;
                continue;
            }

            builder.Append(current);
        }

        return builder.ToString();
    }
}