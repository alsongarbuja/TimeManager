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
        policy.WithOrigins("https://localhost:7046")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
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

builder.Services.AddAuthentication();   

builder.Services.AddIdentityApiEndpoints<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
}).AddRoles<IdentityRole>().AddEntityFrameworkStores<HrmsDbContext>();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Require Admin", policy => policy.RequireRole("Admin"))
    .AddPolicy("CanPublish", policy => policy.RequireClaim("permission", "publish"));

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = ctx =>
    {
        ctx.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = ctx =>
    {
        ctx.Response.StatusCode = 403;
        return Task.CompletedTask;
    };
});

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

app.MapGroup("/api/auth").MapIdentityApi<IdentityUser>();

app.MapControllers();

await DataSeeder.SeedDataAsync(app.Services);

app.Run();
