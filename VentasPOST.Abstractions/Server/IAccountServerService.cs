using VentasPOST.Domain.DTOs.Request.Account;
using VentasPOST.Domain.DTOs.Response.Account;
using VentasPOST.Domain.DTOs.Response;

namespace VentasPOST.Abstractions.Server;

public interface IAccountServerService
{
    Task CreateAdmin();
    Task<GeneralResponse> CreateAccountAsync(CreateAccountDTO model);
    Task<LoginResponse> LoginAccountAsync(LoginDTO model);
    Task<LoginResponse> RefreshTokenAsync(RefreshTokenDTO model);
    Task<GeneralResponse> CreateRoleAsync(CreateRoleDTO model);
    Task<IEnumerable<GetRoleDTO>> GetRolesAsync();
    Task<IEnumerable<GetUsersWithRolesResponseDTO>> GetUsersWithRolesAsync();
    Task<GeneralResponse> ChangeUserRoleAsync(ChangeUserRoleRequestDTO model);
}