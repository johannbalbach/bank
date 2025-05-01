using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.BL.OperationFilters
{
    public class AddIdempotencyHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation == null || operation.RequestBody == null)
                return;

            var methodsRequiringIdempotency = new[] { "POST", "PUT", "PATCH", "DELETE" };
            var method = context.ApiDescription.HttpMethod;

            if (method != null && methodsRequiringIdempotency.Contains(method.ToUpper()))
            {
                operation.Parameters ??= new List<OpenApiParameter>();

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "Idempotency-Key",
                    In = ParameterLocation.Header,
                    Required = false,
                    Schema = new OpenApiSchema
                    {
                        Type = "string"
                    },
                    Description = "Idempotency Key to prevent duplicate operations."
                });
            }
        }
    }
}
