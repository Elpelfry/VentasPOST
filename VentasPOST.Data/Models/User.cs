using Microsoft.AspNetCore.Identity;

namespace VentasPOST.Data.Models;

public class User : IdentityUser
{
    public string? Name { get; set; }
}
