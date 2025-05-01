using Bank.BL.Configuration;
using Bank.BL.Redis;
using Bank.BL.Redis.Messages;
using Bank.BL.Redis.Patterns;
using Bank.DAL.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Bank.BL.ExceptionHandler
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IServiceProvider serviceProvider) : IExceptionHandler
    {
        ILogger<GlobalExceptionHandler> _logger = logger;
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

            _logger.LogError(
                exception,
                $"Error occured. TraceId : {traceId}",
                Environment.MachineName,
                traceId
                ); // optional

            var (statusCode, title) = MapException(exception);

            if (Guid.TryParse(httpContext.Items[RedisConfiguration.RedisWatch] as string, out Guid messageId))
            {
                var redisFacade = _serviceProvider.GetRequiredService<IRedisMessagingFacade>();
                await redisFacade.ProcessRedisMessage<RedisMessage>(messageId, title, statusCode);
            }

            await Results.Problem(
                title: title,
                statusCode: statusCode,
                extensions: new Dictionary<string, object?>
                {
                    ["traceId"] = traceId,
                    ["machine"] = Environment.MachineName,
                    ["data"] = exception.Data
                }
            ).ExecuteAsync(httpContext);

            return true;
        }


        private static (int StatusCode, string Title) MapException(Exception exception)
        {
            return exception switch
            {
                InvalidOperationException => (StatusCodes.Status400BadRequest, exception.Message),
                InvalidLoginException invalidLoginException => (StatusCodes.Status403Forbidden, invalidLoginException.Message),
                InvalidTokenException invalidTokenException => (StatusCodes.Status401Unauthorized, invalidTokenException.Message),
                BadRequestException badRequestException => (StatusCodes.Status400BadRequest, badRequestException.Message),
                NotFoundException notFoundException => (StatusCodes.Status404NotFound, notFoundException.Message),
                ForbiddenException forbiddenException => (StatusCodes.Status403Forbidden, forbiddenException.Message),
                SimulatedException simulatedException => (StatusCodes.Status503ServiceUnavailable, simulatedException.Message),
                _ => (StatusCodes.Status500InternalServerError, "Some stupid error occured")
            };
        }
    }
}
