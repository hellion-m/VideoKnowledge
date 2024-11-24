using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Threading.Tasks;

namespace VideoKnowledge.Infrastructure.Validation
{
    public class CheckIframeUrlMiddleware
    {
        // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
        private readonly HttpClient _client;
    private readonly RequestDelegate _next;

    public CheckIframeUrlMiddleware(RequestDelegate next)
    {
        _next = next;
        _client = new HttpClient();
    }

    public async Task InvokeAsync(HttpContext context, IframeReferenceBuilder _builder)
    {
        try
        {
            var response = await _client.GetAsync(_builder.BuildIFrameReference(context.Request.GetDisplayUrl()));

            if (!response.IsSuccessStatusCode)
                context.Response.Redirect(_builder.Settings.RedirectDomain);

            if (response.Headers.TryGetValues("X-Frame-Options", out var values) && values.Contains("DENY"))
                context.Response.Redirect(_builder.Settings.RedirectDomain);

            await _next.Invoke(context);
        }
        catch
        {
            context.Response.Redirect(_builder.Settings.RedirectDomain);
        }
    }
}
    //public class CheckIframeUrlMiddleware
    //{
    //    private readonly RequestDelegate _next;

    //    public CheckIframeUrlMiddleware(RequestDelegate next)
    //    {
    //        _next = next;
    //    }

    //    public Task Invoke(HttpContext httpContext)
    //    {

    //        return _next(httpContext);
    //    }
    //}

    //// Extension method used to add the middleware to the HTTP request pipeline.
    //public static class CheckIframeUrlMiddlewareExtensions
    //{
    //    public static IApplicationBuilder UseCheckIframeUrlMiddleware(this IApplicationBuilder builder)
    //    {
    //        return builder.UseMiddleware<CheckIframeUrlMiddleware>();
    //    }
    //}
}
