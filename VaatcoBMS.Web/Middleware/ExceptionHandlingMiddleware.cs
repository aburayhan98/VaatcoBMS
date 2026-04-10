namespace VaatcoBMS.Web.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
	public async Task InvokeAsync(HttpContext ctx)
	{
		try
		{
			await next(ctx);
		}
		catch (UnauthorizedAccessException ex)
		{
			logger.LogWarning(ex, "Unauthorized: {Path}", ctx.Request.Path);

			if (ctx.Request.Path.StartsWithSegments("/api"))
			{
				ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
				await ctx.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
			}
			else
			{
				ctx.Response.Redirect("/Auth/Login?error=unauthorized");
			}
		}
		catch (KeyNotFoundException ex)
		{
			logger.LogWarning(ex, "Not found: {Path}", ctx.Request.Path);
			ctx.Response.StatusCode = StatusCodes.Status404NotFound;

			if (ctx.Request.Path.StartsWithSegments("/api"))
			{
				await ctx.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
			}
			else
			{
				ctx.Response.Redirect("/Home/NotFound");
			}
		}
		catch (InvalidOperationException ex)
		{
			logger.LogWarning(ex, "Bad request: {Path}", ctx.Request.Path);
			ctx.Response.StatusCode = StatusCodes.Status400BadRequest;

			if (ctx.Request.Path.StartsWithSegments("/api"))
			{
				await ctx.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
			}
			else
			{
				ctx.Response.Redirect($"/Auth/Login?error={Uri.EscapeDataString(ex.Message)}");
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Unhandled exception on {Path}", ctx.Request.Path);
			ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;

			if (ctx.Request.Path.StartsWithSegments("/api"))
			{
				await ctx.Response.WriteAsJsonAsync(new { success = false, message = "An internal server error occurred." });
			}
			else
			{
				ctx.Response.Redirect("/Home/Error");
			}
		}
	}
}