using TimeManager.Backend.Data;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Shared;
using TimeManager.Backend.Controllers.PunchManagement.Utility;
using Microsoft.AspNetCore.Identity;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.WithOrigins("https://locahost:7046")
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var serverVersion = ServerVersion.AutoDetect(connectionString);

builder.Services.AddDbContext<HrmsDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));
builder.Services.AddScoped<PayPeriodUtility>();

//builder.Services.AddDbContext<HrmsDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultServer")));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies();

builder.Services.AddIdentityCore<IdentityUser>(options =>
{
    options.Password.RequiredLength = 5;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<HrmsDbContext>()
    .AddDefaultTokenProviders()
    .AddApiEndpoints();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Require Admin", policy => policy.RequireRole("Admin"))
    .AddPolicy("CanPublish", policy => policy.RequireClaim("permission", "publish"));

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
//builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
//    .AddCookie(IdentityConstants.ApplicationScheme, options =>
//    {
//        options.Cookie.HttpOnly = true;
//        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
//        options.Cookie.SameSite = SameSiteMode.Strict;

//        options.Events.OnRedirectToLogin = context =>
//        {
//            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
//            return Task.CompletedTask;
//        };
//    });

//builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseCors("AllowBlazor");
app.UseAuthentication();
app.UseAuthorization();

//app.MapGroup("/auth").MapIdentityApi<IdentityUser>();

app.MapControllers();

await DataSeeder.SeedDataAsync(app.Services);

app.Run();
