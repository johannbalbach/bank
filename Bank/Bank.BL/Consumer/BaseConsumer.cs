using Bank.BL.Redis;
using Bank.BL.Redis.Messages;
using Bank.BL.Services;
using Bank.DAL.Exceptions;
using Bank.DTO.DTOs.ServiceBusDto;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.BL.Consumer
{
    public abstract class BaseConsumer<TMessage> : IConsumer<TMessage> where TMessage : baseMessage
    {
        protected readonly IIdempotencyService _idempotencyService;
        protected readonly IAsyncPolicy _policy;
        private readonly IServiceProvider _serviceProvider;

        public BaseConsumer(IIdempotencyService idempotencyService, IServiceProvider serviceProvider)
        {
            _idempotencyService = idempotencyService;
            _policy = CreatePolicy();
            _serviceProvider = serviceProvider;
        }

        public async Task Consume(ConsumeContext<TMessage> context)
        {
            Stopwatch watch = Stopwatch.StartNew();

            ConsumerMessage consumerRedisMessage = new();
            IRedisMessagingService redisMessagingService = _serviceProvider.GetRequiredService<IRedisMessagingService>();

            var message = context.Message;
            var idempotencyKey = GetIdempotencyKey(message);

            consumerRedisMessage.ConsumerName = this.GetType().Name;
            consumerRedisMessage.MessageName = message.GetType().Name;
            consumerRedisMessage.StatusCode = 200;

            if (await _idempotencyService.IsRequestProcessed(idempotencyKey))
            {
                watch.Stop();
                consumerRedisMessage.ElapsedMilliseconds = watch.ElapsedMilliseconds;
                consumerRedisMessage.Message = "Request processed";
                await redisMessagingService.PushMessageAsync(consumerRedisMessage);

                var cachedResponse = await _idempotencyService.GetCachedResponse(idempotencyKey);
                await SendResponse(context, cachedResponse);
                return;
            }

            if (ShouldSimulateError())
            {
                watch.Stop();
                consumerRedisMessage.ElapsedMilliseconds = watch.ElapsedMilliseconds;
                consumerRedisMessage.StatusCode = 503;
                consumerRedisMessage.Message = "Simulated server error";
                await redisMessagingService.PushMessageAsync(consumerRedisMessage);

                throw new SimulatedException("Simulated server error");
            }

            baseResponse response = await _policy.ExecuteAsync(async () => await ProcessMessageAsync(message));

            await _idempotencyService.SaveResponse(idempotencyKey, response);
            await SendResponse(context, response);

            watch.Stop();
            consumerRedisMessage.ElapsedMilliseconds = watch.ElapsedMilliseconds;
            consumerRedisMessage.Message = "Processing new request";
            await redisMessagingService.PushMessageAsync(consumerRedisMessage);
        }
        protected abstract Task<baseResponse> ProcessMessageAsync(TMessage message);
        protected abstract Task SendResponse(ConsumeContext<TMessage> context, baseResponse response);

        private string GetIdempotencyKey(TMessage message)
        {
            return message.IdempotencyKey;
        }

        private bool ShouldSimulateError()
        {
            var currentMinute = DateTime.UtcNow.Minute;
            var isEvenMinute = currentMinute % 2 == 0;
            var errorProbability = isEvenMinute ? 0.9 : 0.5;
            var random = new Random();
            return random.NextDouble() < errorProbability;
        }

        private IAsyncPolicy CreatePolicy()
        {
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            var circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(5, TimeSpan.FromMinutes(1));

            return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
        }
    }
}
