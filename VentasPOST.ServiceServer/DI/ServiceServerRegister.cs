using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VentasPOST.Abstractions.Server;
using VentasPOST.Data.DI;

namespace VentasPOST.ServiceServer.DI;

public static class ServiceServerRegister
{
    public static IServiceCollection APIServicesRegister(this IServiceCollection services)
    {
        var config = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        services.RegisterDbContext();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>

           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuer = true,
               ValidateAudience = true,
               ValidateIssuerSigningKey = true,
               ValidateLifetime = true,
               ValidIssuer = config["Jwt:Issuer"],
               ValidAudience = config["Jwt:Audience"],
               IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!))
           }
        );
        services.AddAuthentication();
        services.AddAuthorization();

        services.AddCors(options =>
        {
            var allowedOrigins = config.GetSection("AllowedOrigins").Get<string[]>();

            options.AddPolicy("WebUI", builder =>
            {
                builder
                    .WithOrigins(allowedOrigins!)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        services.AddScoped<IAccountServerService, AccountService>();

        return services;
    }
}
