using Microsoft.Extensions.Logging;

namespace DotnetGui.Core.Common.Extensions;
internal static partial class LoggerExtensions
{
    [LoggerMessage(
        EventId = 0,
        Level = LogLevel.Information,
        Message = "Executed {Name} {Arguments}.\r\n{Output}{Error}")]
    public static partial void ExecutedSuccessfully(this ILogger logger, string name, 
                string arguments, string output, string error);

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Executed {Name} {Arguments}.")]
    public static partial void ExecutedFailed(this ILogger logger, string name, string arguments, Exception exception);
}
