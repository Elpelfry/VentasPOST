using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NetcodeHub.Packages.Extensions.LocalStorage;
using VentasPOST.Abstractions.Client;
using VentasPOST.Domain;
using VentasPOST.ServiceClient.SecurityHandlers;

namespace VentasPOST.ServiceClient.DI;

public static class ServiceClientRegister
{
    public static IServiceCollection WebUIServicesRegister(this IServiceCollection services)
    {
        services.AddScoped<IAccountClientService, AccountService>();

        services.AddAuthorizationCore();
        services.AddNetcodeHubLocalStorageService();
        services.AddScoped<LocalStorageService>();
        services.AddScoped<HttpClientService>();
        services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
        services.AddTransient<CustomHttpHandler>(); services.AddCascadingAuthenticationState();
        services.AddScoped<NotificationService>();
        services.AddBlazorBootstrap();

        services.AddHttpClient(Constant.HttpClientName, client =>
        {
            client.BaseAddress = new Uri("https://localhost:7280/");
        }).AddHttpMessageHandler<CustomHttpHandler>();
        return services;
    }
}
