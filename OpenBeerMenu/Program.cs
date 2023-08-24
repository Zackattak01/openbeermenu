using Microsoft.EntityFrameworkCore;
using OpenBeerMenu.Data;
using OpenBeerMenu.Extensions;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureLogging(x =>
{
    x.ClearProviders();
    
    var logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.Hosting", LogEventLevel.Information)
        // .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("EndpointMiddleware", LogEventLevel.Warning)
        .MinimumLevel.Override("DefaultAuthorizationService", LogEventLevel.Warning)
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
            theme: AnsiConsoleTheme.Code)
        .CreateLogger();

    x.AddSerilog(logger, true);

    x.Services.Remove(x.Services.First(x => x.ServiceType == typeof(ILogger<>)));
    x.Services.AddSingleton(typeof(ILogger<>), typeof(OpenBeerMenu.Types.Logger<>));
});

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var connString = builder.Configuration["Postgres:ConnectionString"];
builder.Services.AddDbContext<OpenBeerMenuDbContext>(x => x.UseNpgsql(connString).UseSnakeCaseNamingConvention(), ServiceLifetime.Transient);

builder.Services.AddOpenBeerServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
