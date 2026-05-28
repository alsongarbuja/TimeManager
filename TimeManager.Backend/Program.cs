using TimeManager.Backend.Data;
using Microsoft.EntityFrameworkCore;

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

//builder.Services.AddDbContext<HrmsDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultServer")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowBlazor");
app.UseAuthorization();

app.MapControllers();

app.Run();
