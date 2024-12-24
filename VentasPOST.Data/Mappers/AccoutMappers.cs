using Microsoft.AspNetCore.Identity;
using VentasPOST.Domain.DTOs.Request.Account;
using VentasPOST.Domain.DTOs.Response.Account;

namespace VentasPOST.Data.Mappers;

public static class AccoutMappers
{
    public static IEnumerable<GetRoleDTO> ToGetRoleDTO(this IEnumerable<IdentityRole> roles)
    {
        return roles.Select(x => new GetRoleDTO(x.Id, x.Name!));
    }

    public static CreateRoleDTO ToCreateRoleDTO(this IdentityRole role)
    {
        return new CreateRoleDTO{ Name = role.Name };
    }
}
