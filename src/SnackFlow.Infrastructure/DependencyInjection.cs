﻿using SnackFlow.Infrastructure.Persistence;
using SnackFlow.Infrastructure.Persistence.Interceptors;
using SnackFlow.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using SnackFlow.Domain.Repositories;
using SnackFlow.Infrastructure.Services;
using SnackFlow.Infrastructure.Services.Abstractions;

namespace SnackFlow.Infrastructure;

/// <summary>
/// Fornece métodos e configurações para configurar o container de injeção de dependência da camada de Infrastructure.
/// Esta classe tem como objetivo simplificar o processo de registro de serviços
/// e dependências necessárias para o funcionamento da aplicação.
/// </summary>
public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration,
        ILoggingBuilder logging)
    {
        logging.AddLogging();
        services.AddPersistence(configuration);
        services.AddRepositories();
        services.AddServices();
        services.AddHealthChecksConfiguration(configuration);
    }

    private static void AddLogging(this ILoggingBuilder logging)
    {
        const string output =
            "[{Timestamp:dd/MM/yyyy HH:mm:ss}] {Level:u3} | {SourceContext} | {Message:lj}{NewLine}{Exception}";
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: output)
            .WriteTo.File(
                path: "Common/Logs/app-.log",
                outputTemplate: output,
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Information,
                retainedFileCountLimit: 30)
            .WriteTo.File(
                path: "Common/Logs/errors-.log",
                outputTemplate: output,
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Error,
                retainedFileCountLimit: 90)
            .CreateLogger();

        logging.AddSerilog();
    }

    private static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options
                .UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
                .AddInterceptors(
                    new CaseInsensitiveInterceptor(),
                    new AuditInterceptor());
        });
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddTransient<ICertificateService, CertificateService>();
    }

    private static void AddHealthChecksConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration));

        services
            .AddHealthChecks()
            .AddNpgSql(connectionString, name: "DefaultConnection");
    }
}