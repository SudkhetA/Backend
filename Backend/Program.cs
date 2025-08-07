using Serilog;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Utilities.Helper;
using Backend.Utilities.Authorization;
using Backend.Services.System;
using Backend.Utilities.Authentication;
using StackExchange.Redis;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Backend;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddScoped<AuthenticationService>();
        builder.Services.AddScoped<UserService>();

        builder.Services.AddScoped<IAuthorizationHandler, CrudAuthorizationHandler>();
        builder.Services.AddScoped<PermissionService>();
        builder.Services.AddTransient<EncryptedHelper>();
        builder.Services.AddTransient<JwtHelper>();

        builder.Services.AddScoped<SessionService>();
        builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var connectionString = builder.Configuration["Redis:ConnectionString"] ?? throw new InvalidOperationException("Redis connection string not found");
            return ConnectionMultiplexer.Connect(connectionString);
        });
        
        builder.Services.AddDbContext<DataContext>(options => 
        {
            // If changing the settings below, please verify the code in Backend.Models.DataSeed
            var connectionString = builder.Configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("ConnectionString not found");
            options.UseSqlServer(connectionString, opt => opt.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            if (builder.Environment.IsDevelopment()) 
            {
                options.EnableSensitiveDataLogging();
            }
        });

        builder.Services.AddCors(options => 
        {
            options.AddDefaultPolicy(policy => {
                var origins = builder.Configuration.GetSection("Authentication:Cors:Origins").Get<string[]>() ?? throw new InvalidOperationException("Cors:Origins not found");
                policy.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => JwtAuthenticationHandler.DefaultConfiguration(options, builder.Configuration))
            .AddJwtBearer("Refresh", options => JwtAuthenticationHandler.RefreshConfiguration(options, builder.Configuration));
        builder.Services.AddAuthorizationBuilder()
            .SetDefaultPolicy(new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build())
            .AddPolicy("Refresh", new AuthorizationPolicyBuilder("Refresh")
                .RequireAuthenticatedUser()
                .Build())
            .AddPolicy("Create", new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .AddRequirements(new CrudAuthorizationRequirement("Create"))
                .Build())
            .AddPolicy("Read", new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .AddRequirements(new CrudAuthorizationRequirement("Read"))
                .Build())
            .AddPolicy("Update", new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .AddRequirements(new CrudAuthorizationRequirement("Update"))
                .Build())
            .AddPolicy("Delete", new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .AddRequirements(new CrudAuthorizationRequirement("Delete"))
                .Build());

        builder.Services.AddControllers()
            .AddMvcOptions(options =>
            {
                options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddMemoryCache();
        builder.Services.AddHealthChecks().AddDbContextCheck<DataContext>();
        builder.Services.AddSerilog();

        var app = builder.Build();

        var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        var encrypted = scope.ServiceProvider.GetRequiredService<EncryptedHelper>();
        DataSeed.Seed(context, encrypted);

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = string.Empty;
            });
        }

        app.UseWebSockets(new WebSocketOptions
        {
            KeepAliveInterval = TimeSpan.FromMinutes(2),
        });

        app.MapHealthChecks("/api/HealthCheck");
        app.UseSerilogRequestLogging();

        app.UseHttpsRedirection();

        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
