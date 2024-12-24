using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VentasPOST.Data.Models;

namespace VentasPOST.Data.Context;

public class VentasPOSTDbContext : IdentityDbContext<User>
{
    public VentasPOSTDbContext(DbContextOptions<VentasPOSTDbContext> options) : base(options)
    {
    }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
}
