using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VentasPOST.Data.Context;
using VentasPOST.Data.Models;

namespace VentasPOST.Data.DI;

public static class DbContextRegister
{
    public static IServiceCollection RegisterDbContext(this IServiceCollection services)
    {
        var config = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

        services.AddDbContextFactory<VentasPOSTDbContext>(o => o.UseMySql(
            config.GetConnectionString("SqlConStr"), new MySqlServerVersion(new Version(8, 0, 30))));

        services.AddIdentityCore<User>()
            .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<VentasPOSTDbContext>()
                    .AddSignInManager();

        return services;
    }
}
