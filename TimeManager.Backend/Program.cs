using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Controllers.PunchManagement.Utility;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models.AuthManagement;
using TimeManager.Backend.Services;
using TimeManager.Backend.Shared;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddIdentity<User, Role>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;

    options.Lockout.MaxFailedAccessAttempts = 10;
    options.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<HrmsDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});

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

//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
//var serverVersion = ServerVersion.AutoDetect(connectionString);

//builder.Services.AddDbContext<HrmsDbContext>(options =>
//    options.UseMySql(connectionString, serverVersion));

var connectionString = builder.Configuration.GetConnectionString("SQLConnectionString");

builder.Services.AddDbContext<HrmsDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<PayPeriodUtility>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IUnitService, UnitService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPayFrequencyService, PayFrequencyService>();
builder.Services.AddScoped<IEmployeeTypeService, EmployeeTypeService>();
builder.Services.AddScoped<IPayPeriodService, PayPeriodService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IProfileTemplateService, ProfileTemplateService>();
builder.Services.AddScoped<IJobProfileService, JobProfileService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IKioskService, KioskService>();
builder.Services.AddScoped<CurrentEmployeeService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();



var app = builder.Build();

app.UseStatusCodePagesWithReExecute("/Error/{0}");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AllowBlazor");
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

//app.MapGroup("/api/auth").MapIdentityApi<IdentityUser>();
app.MapControllers();

// MVC view route setup 
app.MapControllerRoute(
    name: "login",
    pattern: "auth/login",
    defaults: new { controller = "Auth", action = "Login" }
);

app.MapControllerRoute(
    name: "accessdenied",
    pattern: "auth/accessdenied",
    defaults: new { controller = "Auth", action = "AccessDenied" }
);

app.MapControllerRoute(
    name: "app",
    pattern: "app/{controller=Dashboard}/{action=Index}/{id?}"
);

await DataSeeder.SeedDataAsync(app.Services);

app.Run();
