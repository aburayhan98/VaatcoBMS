using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using VaatcoBMS.Application.DTOs.User;
using VaatcoBMS.Application.Interfaces;
using VaatcoBMS.Application.Services;
using VaatcoBMS.Application.Settings;
using VaatcoBMS.Domain.Interfaces;
using VaatcoBMS.Infrastructure.Persistence;
using VaatcoBMS.Infrastructure.Repositories;
using VaatcoBMS.Infrastructure.Services;
using VaatcoBMS.Infrastructure.Utility;
using VaatcoBMS.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog
builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

// 2. Add Database Context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("VaatcoBMSDB"),
        b => b.MigrationsAssembly("VaatcoBMS.Infrastructure")));

// 3. Configure Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

// 4. JWT Authentication
var jwt = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
// Safety check: Prevent app from crashing on startup if user-secrets are missing or empty
var secretKey = string.IsNullOrEmpty(jwt?.Secret) ? "FallbackSuperSecretKeyThatIsAtLeast32Characters!" : jwt.Secret;

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidIssuer = jwt?.Issuer ?? "VaatcoBMS_API",
		ValidateAudience = true,
		ValidAudience = jwt?.Audience ?? "VaatcoBMS_Client",
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
	};

	options.Events = new JwtBearerEvents
	{
		OnMessageReceived = ctx =>
		{
			ctx.Token = ctx.Request.Cookies["access_token"];
			return Task.CompletedTask;
		},
		OnChallenge = ctx =>
		{
			// If not starting with /api, redirect to your login page
			if (!ctx.Request.Path.StartsWithSegments("/api"))
			{
				ctx.HandleResponse();
				ctx.Response.Redirect("/auth/login");
			}
			return Task.CompletedTask;
		}
	};
});

builder.Services.AddAuthorization();

// 5. Register Repositories & Unit of Work
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IInvoiceItemRepository, InvoiceItemRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 6. Application Services & Utilities
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IInvoiceItemService, InvoiceItemService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITokenBuilder, TokenBuilder>();
builder.Services.AddScoped<HashService>();

// 7. Mapster & FluentValidation 
var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
typeAdapterConfig.Scan(typeof(VaatcoBMS.Application.Mappings.MappingProfile).Assembly);
builder.Services.AddSingleton(typeAdapterConfig);
builder.Services.AddScoped<MapsterMapper.IMapper, MapsterMapper.Mapper>();

builder.Services.AddValidatorsFromAssemblyContaining<ChangePasswordDtoValidator>();

// 8. Swagger with Bearer auth
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "VaatcoBMS API", Version = "v1" });
	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Type = SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT",
		In = ParameterLocation.Header,
		Description = "Paste your JWT access token here"
	});
	c.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			Array.Empty<string>()
		}
	});
});

// 9. MVC & Session
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(o => o.IdleTimeout = TimeSpan.FromHours(8));

var app = builder.Build();

// Configure the HTTP request pipeline
//if (!app.Environment.IsDevelopment())
//{
//	app.UseExceptionHandler("/Home/Error");
//	app.UseHsts();
//}

app.UseExceptionHandler("/Home/Error");
app.UseHsts();

// Middleware pipeline (order matters!)
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

// Authentication MUST come before Authorization
app.UseAuthentication();
app.UseAuthorization();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "VaatcoBMS v1"));

// Route configuration
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}"); 

app.MapControllers();

app.Run();