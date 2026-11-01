using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using StudentElection.Model.Data;
using StudentElection.Repository;
using StudentElection.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Uses an existing database (created with the SQL script) - no migrations.
var databaseUrl = builder.Configuration["DATABASE_URL"];
builder.Services.AddDbContext<AppDBContext>(o =>
{
    if (string.IsNullOrWhiteSpace(databaseUrl))
    {
        o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        return;
    }

    var uri = new Uri(databaseUrl);
    if (uri.Scheme != "postgres" && uri.Scheme != "postgresql")
        throw new InvalidOperationException("DATABASE_URL must be a PostgreSQL URL.");
    var credentials = uri.UserInfo.Split(':', 2);
    if (credentials.Length != 2 || string.IsNullOrWhiteSpace(uri.Host))
        throw new InvalidOperationException("DATABASE_URL must include PostgreSQL credentials and a host.");
    var connection = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort ? 5432 : uri.Port,
        Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
        Username = Uri.UnescapeDataString(credentials[0]),
        Password = Uri.UnescapeDataString(credentials[1]),
        SslMode = SslMode.VerifyFull
    };
    o.UseNpgsql(connection.ConnectionString);
});

// Repository
builder.Services.AddScoped<ICandidateRepository, CandidateRepository>();
builder.Services.AddScoped<IVoteRepository, VoteRepository>();

// Service
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IExportService, ExportService>();

var app = builder.Build();

// Serve the election artwork with an explicit media type. .NET 8 may not map AVIF by default.
var contentTypes = new FileExtensionContentTypeProvider();
contentTypes.Mappings[".avif"] = "image/avif";
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = contentTypes });

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
