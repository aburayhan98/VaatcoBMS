using System.Diagnostics;

namespace VaatcoBMS.Web.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
	public async Task InvokeAsync(HttpContext ctx)
	{
		var sw = Stopwatch.StartNew();
		await next(ctx);
		sw.Stop();

		logger.LogInformation(
			"{Method} {Path}{Query} => {StatusCode} in {Ms}ms",
			ctx.Request.Method,
			ctx.Request.Path,
			ctx.Request.QueryString,
			ctx.Response.StatusCode,
			sw.ElapsedMilliseconds);
	}
}