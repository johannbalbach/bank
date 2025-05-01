using Bank.BL.Configuration;
using Bank.DAL.Enums;
using Core.BL.CQRS.Base;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Core.Controllers.BasedController
{
    [ApiController]
    public class CoreControllerExtended : ControllerBase
    {
        protected IMediator _mediator { get; set; }
        protected ILogger<CoreControllerExtended> _logger { get; set; }

        public CoreControllerExtended(IMediator mediator, ILogger<CoreControllerExtended> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<TResponse> SendWithMedata<TRequest, TResponse>(TRequest request)
            where TRequest : class
            where TResponse : class
        {
            var user = HttpContext.User;
            var userId = user.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value
                        ?? throw new KeyNotFoundException("User does not have authenticator");

            if (!Guid.TryParse(userId, out Guid guidUserId))
            {
                throw new InvalidDataException($"User id {userId} is not Guid");
            }

            var userRole = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value
                            ?? throw new InvalidDataException($"Invalid data");

            if(Enum.TryParse<UserRole>(userRole, out var userRoleParsed) == false)
            {
                throw new InvalidDataException($"Invalid role name");
            }

            var deviceName = HttpContext.Request.Headers["DeviceName"];

            if (!Guid.TryParse(HttpContext.Items[RedisConfiguration.RedisWatch] as string, out Guid redisMessageId))
            {
                throw new InvalidDataException($"Invalid redis message id");
            };

            return await SendToMediator<TRequest, TResponse>(request, new MediatorMetaData
            {
                UserId = guidUserId,
                UserRole = userRoleParsed,
                DeviceName = deviceName.ToString(),
                RedisMessageId = redisMessageId
            });
        }

        public async Task<TResponse> Send<TRequest, TResponse>(TRequest request)
            where TRequest : class
            where TResponse : class
        {
            var deviceName = HttpContext.Request.Headers["DeviceName"];

            if (!Guid.TryParse(HttpContext.Items[RedisConfiguration.RedisWatch] as string, out Guid redisMessageId))
            {
                throw new InvalidDataException($"Invalid redis message id");
            };

            return await SendToMediator<TRequest, TResponse>(request, new MediatorMetaData
            {
                DeviceName = deviceName.ToString(),
                RedisMessageId = redisMessageId
            });
        }

        private async Task<TResponse> SendToMediator<TRequest, TResponse>(TRequest request, MediatorMetaData? metaData = null)
            where TRequest : class
            where TResponse : class
        {
            var result = await _mediator.Send(new MediatorRequest<TRequest, TResponse>()
            {
                Request = request,
                MetaData = metaData
            });

            return result;
        }
    }
}
