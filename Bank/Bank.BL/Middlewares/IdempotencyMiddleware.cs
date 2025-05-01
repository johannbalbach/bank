using Bank.BL.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bank.BL.Middlewares
{
    public class IdempotencyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IIdempotencyService _idempotencyService;

        public IdempotencyMiddleware(RequestDelegate next, IIdempotencyService idempotencyService)
        {
            _next = next;
            _idempotencyService = idempotencyService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Method == "GET")
            {
                await _next(context);
                return;
            }

            if (context.Request.Method != "GET" && context.Request.Headers.TryGetValue("Idempotency-Key", out var key))
            {
                var idempotencyKey = key.ToString();
                if (await _idempotencyService.HttpIsRequestProcessed(idempotencyKey))
                {
                    var cachedResponse = await _idempotencyService.HttpGetCachedResponse(idempotencyKey);
                    context.Response.StatusCode = 200;

                    if (cachedResponse == "Simulated Server Error")
                    {
                        context.Response.StatusCode = 503;
                    }

                    await context.Response.WriteAsync(cachedResponse);
                    return;
                }

                var originalBodyStream = context.Response.Body;
                using var memoryStream = new MemoryStream();
                context.Response.Body = memoryStream;

                await _next(context);

                memoryStream.Seek(0, SeekOrigin.Begin);
                var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

                if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
                {
                    await _idempotencyService.HttpSaveResponse(idempotencyKey, responseBody);
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                await memoryStream.CopyToAsync(originalBodyStream);
                context.Response.Body = originalBodyStream;
            }
            else
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Idempotency Key Not Found");
                return;
            }
        }
    }
}
