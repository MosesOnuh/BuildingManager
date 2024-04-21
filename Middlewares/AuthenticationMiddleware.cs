using BuildingManager.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace BuildingManager.Middlewares
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        //check this line of code
        //private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IServiceManager _service;
        //AuthenticatioMiddleware(RequestDelegate next,
        //    ILogger<ErrorHandlingMiddleware> logger,
            //IServiceManager service)
           public AuthenticationMiddleware(RequestDelegate next,
            IServiceManager service)
        {
            _next = next;
            //_logger = logger;
            _service = service;
        }

        public async Task Invoke(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata.GetMetadata<AuthorizeAttribute>() != null)
            {
                var authorizationHandler = context.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrEmpty(authorizationHandler) && authorizationHandler.StartsWith("Bearer "))
                {
                    var token = authorizationHandler.Substring("Bearer ".Length).Trim();
                    SetUserToContext(context, token);
                   // await _next(context);
                }
            }

            await _next(context);
            
        }

        private void SetUserToContext(HttpContext context, string token)
        {
            string userId = _service.TokenService.ValidateToken(token);
            context.Items["UserId"] = userId;
        }
    }

    public static class AuthenticationMiddlewareExtension 
    {
        public static IApplicationBuilder UseAuthenticationHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}