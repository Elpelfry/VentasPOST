using VentasPOST.Domain.DTOs.Request.Account;
using VentasPOST.Domain.DTOs.Response.Account;
using VentasPOST.Domain.DTOs.Response;

namespace VentasPOST.Abstractions.Client;

public interface IAccountClientService
{
    Task CreateAdmin();
    Task<GeneralResponse> CreateAccountAsync(CreateAccountDTO model);
    Task<LoginResponse> LoginAccountAsync(LoginDTO model);
    Task<LoginResponse> RefreshTokenAsync(RefreshTokenDTO model);
    Task<IEnumerable<GetRoleDTO>> GetRolesAsync();
    Task<IEnumerable<GetUsersWithRolesResponseDTO>> GetUsersWithRolesAsync();
    Task<GeneralResponse> ChangeUserRoleAsync(ChangeUserRoleRequestDTO model);
}