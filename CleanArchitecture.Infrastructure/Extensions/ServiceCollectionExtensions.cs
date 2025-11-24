using CleanArchitecture.Domain.DomainEvents;
using CleanArchitecture.Domain.Interfaces;
using CleanArchitecture.Domain.Interfaces.Repositories;
using CleanArchitecture.Domain.Notifications;
using CleanArchitecture.Infrastructure.Database;
using CleanArchitecture.Infrastructure.EventSourcing;
using CleanArchitecture.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace CleanArchitecture.Infrastructure.Extensions;

public enum DatabaseProvider
{
    SqlServer,
    PostgreSQL
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string migrationsAssemblyName,
        string connectionString,
        DatabaseProvider databaseProvider = DatabaseProvider.SqlServer)
    {
        // Add event store db context
        services.AddDbContext<EventStoreDbContext>(
            options =>
            {
                ConfigureDbContext(options, connectionString, migrationsAssemblyName, databaseProvider);
            });

        services.AddDbContext<DomainNotificationStoreDbContext>(
            options =>
            {
                ConfigureDbContext(options, connectionString, migrationsAssemblyName, databaseProvider);
            });

        // Core Infra
        services.AddScoped<IUnitOfWork, UnitOfWork<ApplicationDbContext>>();
        services.AddScoped<IEventStoreContext, EventStoreContext>();
        services.AddScoped<INotificationHandler<DomainNotification>, DomainNotificationHandler>();
        services.AddScoped<IDomainEventStore, DomainEventStore>();
        services.AddScoped<IMediatorHandler, InMemoryBus>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();

        return services;
    }

    private static void ConfigureDbContext(
        DbContextOptionsBuilder options,
        string connectionString,
        string migrationsAssemblyName,
        DatabaseProvider databaseProvider)
    {
        if (databaseProvider == DatabaseProvider.PostgreSQL)
        {
            options.UseNpgsql(
                connectionString,
                b => b.MigrationsAssembly(migrationsAssemblyName));
        }
        else
        {
            options.UseSqlServer(
                connectionString,
                b => b.MigrationsAssembly(migrationsAssemblyName));
        }
    }
}