using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Azure.Data.Tables;
using Ez.Reasons.Core.Repositories;
using Ez.Reasons.Core.Services;
using Ez.Reasons.Infrastructure.Repositories;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(worker =>
    {
        worker.UseMiddleware<Ez.Reasons.Api.Middleware.JwtMiddleware>();
    })
    .ConfigureServices(services =>
    {
        var connectionString = Environment.GetEnvironmentVariable("TableStorageConnection")
            ?? "UseDevelopmentStorage=true";

        services.AddSingleton(new TableServiceClient(connectionString));

        services.AddSingleton<ILetterRepository>(sp =>
            new TableLetterRepository(sp.GetRequiredService<TableServiceClient>()));
        services.AddSingleton<IUserRepository>(sp =>
            new TableUserRepository(sp.GetRequiredService<TableServiceClient>()));

        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? throw new InvalidOperationException("JWT_SECRET environment variable is required");

        services.AddSingleton<ILetterService>(sp =>
            new LetterService(sp.GetRequiredService<ILetterRepository>()));
        services.AddSingleton<IAuthService>(sp =>
            new AuthService(sp.GetRequiredService<IUserRepository>(), jwtSecret));
    })
    .Build();

host.Run();
