using Microsoft.EntityFrameworkCore;
using StudentElection.Model.Data;
using StudentElection.Repository;
using StudentElection.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Uses the existing database (created with the SQL script) - no migrations
builder.Services.AddDbContext<AppDBContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository
builder.Services.AddScoped<ICandidateRepository, CandidateRepository>();
builder.Services.AddScoped<IVoteRepository, VoteRepository>();

// Service
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IExportService, ExportService>();

var app = builder.Build();

app.UseStaticFiles();

// ServiceException -> { message } with the proper status code
app.Use(async (ctx, next) =>
{
    try { await next(); }
    catch (ServiceException ex)
    {
        ctx.Response.StatusCode = ex.StatusCode;
        await ctx.Response.WriteAsJsonAsync(new { message = ex.Message });
    }
});

// Protect /api/admin/* (except login)
var adminPassword = builder.Configuration["AdminPassword"]
    ?? throw new InvalidOperationException("AdminPassword must be configured.");
app.Use(async (ctx, next) =>
{
    var path = ctx.Request.Path;
    if (path.StartsWithSegments("/api/admin") && !path.StartsWithSegments("/api/admin/login"))
    {
        if (ctx.Request.Headers["X-Admin-Key"] != adminPassword)
        {
            ctx.Response.StatusCode = 401;
            return;
        }
    }
    await next();
});

// Pages
var web = app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot");
app.MapGet("/", () => Results.Redirect("/student"));
app.MapGet("/student", () => Results.File(Path.Combine(web, "student.html"), "text/html"));
app.MapGet("/admin", () => Results.File(Path.Combine(web, "admin.html"), "text/html"));

app.MapControllers();

app.Run();