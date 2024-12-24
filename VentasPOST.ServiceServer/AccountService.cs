using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using VentasPOST.Abstractions.Server;
using VentasPOST.Data.Context;
using VentasPOST.Data.Mappers;
using VentasPOST.Data.Models;
using VentasPOST.Domain;
using VentasPOST.Domain.DTOs.Request.Account;
using VentasPOST.Domain.DTOs.Response;
using VentasPOST.Domain.DTOs.Response.Account;

namespace VentasPOST.ServiceServer;

public class AccountService : IAccountServerService
{
    private readonly RoleManager<IdentityRole> roleManager;
    private readonly UserManager<User> userManager;
    private readonly IConfiguration config;
    private readonly SignInManager<User> signInManager;
    private readonly VentasPOSTDbContext context;

    public AccountService(
        RoleManager<IdentityRole> roleManager,
        UserManager<User> userManager,
        IConfiguration config,
        SignInManager<User> signInManager,
        VentasPOSTDbContext context
    )
    {
        this.roleManager = roleManager;
        this.userManager = userManager;
        this.config = config;
        this.signInManager = signInManager;
        this.context = context;
    }

    public async Task<GeneralResponse> ChangeUserRoleAsync(ChangeUserRoleRequestDTO model)
    {

        if (await FindRoleByNameAsync(model.RoleName) is null) return new GeneralResponse(false, "Role not found");
        if (await FindUserByEmailAsync(model.UserEmail) is null) return new GeneralResponse(false, "User not found");

        var user = await FindUserByEmailAsync(model.UserEmail);
        var previousRole = (await userManager.GetRolesAsync(user)).FirstOrDefault();
        var removeOldRole = await userManager.RemoveFromRoleAsync(user, previousRole!);
        var error = CheckResponse(removeOldRole);
        if (!string.IsNullOrEmpty(error))
            return new GeneralResponse(false, error);

        var result = await userManager.AddToRoleAsync(user, model.RoleName);

        var response = CheckResponse(result);
        if (!string.IsNullOrEmpty(response))
            return new GeneralResponse(false, response);
        else
            return new GeneralResponse(true, "Role Changed");
    }

    public async Task<GeneralResponse> CreateAccountAsync(CreateAccountDTO model)
    {
        try
        {
            if (await FindUserByEmailAsync(model.EmailAddress) != null)
                return new GeneralResponse(false, "Sorry, user is already created");
            var user = new User()
            {
                Name = model.Name,
                UserName = model.EmailAddress,
                Email = model.EmailAddress,
                PasswordHash = model.Password
            };
            var result = await userManager.CreateAsync(user, model.Password);
            string error = CheckResponse(result);
            if (!string.IsNullOrEmpty(error))
                return new GeneralResponse(false, error);
            var (flag, message) = await AssignUserToRole(user, new IdentityRole() { Name = model.Role });

            return new GeneralResponse(flag, message);
        }
        catch (Exception e)
        {
            return new GeneralResponse(false, e.Message);
        }
    }

    public async Task CreateAdmin()
    {
        try
        {
            if (await FindRoleByNameAsync(Constant.Role.Admin) != null) return;
            var admin = new CreateAccountDTO()
            {
                Name = "Admin",
                Password = "Admin@123",
                EmailAddress = "admin@admin.com",
                Role = Constant.Role.Admin
            };
            await CreateAccountAsync(admin);
        }
        catch { }
    }

    public async Task<GeneralResponse> CreateRoleAsync(CreateRoleDTO model)
    {
        try
        {
            if (await FindRoleByNameAsync(model.Name!) == null)
            {
                var response = await roleManager.CreateAsync(new IdentityRole(model.Name!));
                var error = CheckResponse(response);
                if (!string.IsNullOrEmpty(error))
                    throw new Exception(error);
                else
                    return new GeneralResponse(true, $"{model.Name} created");
            }
            return new GeneralResponse(false, $"{model.Name} already created");

        }
        catch (Exception e) { throw new Exception(e.Message); }
    }

    public async Task<IEnumerable<GetRoleDTO>> GetRolesAsync() =>
        (await roleManager.Roles.ToListAsync()).ToGetRoleDTO();

    public async Task<IEnumerable<GetUsersWithRolesResponseDTO>> GetUsersWithRolesAsync()
    {
        var allusers = await userManager.Users.ToListAsync();
        if (allusers is null) return null!;

        var List = new List<GetUsersWithRolesResponseDTO>();

        foreach (var user in allusers)
        {
            var getUserRole = (await userManager.GetRolesAsync(user)).FirstOrDefault();
            var getRoleInfo =
                await roleManager.Roles.FirstOrDefaultAsync(r => r.Name!.ToLower() == getUserRole!.ToLower());

            List.Add(new GetUsersWithRolesResponseDTO()
            {
                Name = user.Name,
                Email = user.Email,
                RoleId = getRoleInfo!.Id,
                RoleName = getRoleInfo.Name
            });
        }

        return List;
    }

    public async Task<LoginResponse> LoginAccountAsync(LoginDTO model)
    {
        try
        {
            var user = await FindUserByEmailAsync(model.EmailAddress);
            if (user is null)
                return new LoginResponse(false, "User not found");
            SignInResult result;

            try
            {
                result = await signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            }
            catch
            {
                return new LoginResponse(false, "Invalid credentials");
            }

            if (!result.Succeeded)
                return new LoginResponse(false, "Invalid credentials");

            string jwtToken = await GenerateToken(user);
            var refreshToken = GenerateRefreshToken();

            if (string.IsNullOrEmpty(jwtToken) || string.IsNullOrEmpty(refreshToken))
            {
                return new LoginResponse(false, "Error occured while logging in account, please contact administration");
            }
            else
            {
                var saveResult = await SaveRefreshToken(user.Id, refreshToken);
                if (saveResult.Flag)
                    return new LoginResponse(true, $"{user.Name} successfully logged in", jwtToken, refreshToken);
                else
                    return new LoginResponse();
            }
        }
        catch (Exception e)
        {
            return new LoginResponse(false, e.Message);
        }
    }

    public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenDTO model)
    {
        var token = await context.RefreshTokens!.FirstOrDefaultAsync(t => t.Token == model.Token);
        if (token is null) return new LoginResponse();
        var user = await userManager.FindByIdAsync(token.UserID!);
        string newToken = await GenerateToken(user!);
        string newRefreshToken = GenerateRefreshToken();

        var saveResult = await SaveRefreshToken(user!.Id, newRefreshToken);

        if (saveResult.Flag)
            return new LoginResponse(true, $"{user.Name} successfully re-logged in", newToken, newRefreshToken);
        else
            return new LoginResponse();
    }

    private async Task<User> FindUserByEmailAsync(string email) => (await userManager.FindByEmailAsync(email))!;
    private async Task<IdentityRole> FindRoleByNameAsync(string roleName) => (await roleManager.FindByNameAsync(roleName))!;
    private static string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    private async Task<string> GenerateToken(User user)
    {
        try
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userClaims = new[]
            {
                new Claim(ClaimTypes.Name, user.Name!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Role, (await userManager.GetRolesAsync(user)).FirstOrDefault()!.ToString()),
                new Claim("Fullname", user.Name!),

            };

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: userClaims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch { return null!; }
    }
    private async Task<GeneralResponse> AssignUserToRole(User user, IdentityRole role)
    {
        if (user is null || role is null) return new GeneralResponse(false, "Model state cannot be empty");
        if (await FindRoleByNameAsync(role.Name!) == null)
            await CreateRoleAsync(role.ToCreateRoleDTO());
        IdentityResult result = await userManager.AddToRoleAsync(user, role.Name!);
        string error = CheckResponse(result);
        if (!string.IsNullOrEmpty(error))
            return new GeneralResponse(false, error);
        else
            return new GeneralResponse(true, $"{user.Name} assigned to {role.Name} role");
    }
    private static string CheckResponse(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(_ => _.Description);
            return string.Join(Environment.NewLine, errors);
        }

        return null!;
    }
    private async Task<GeneralResponse> SaveRefreshToken(string userId, string token)
    {
        try
        {
            var user = await context.RefreshTokens!.FirstOrDefaultAsync(t => t.UserID == userId);
            if (user == null)
                context.RefreshTokens!.Add(new RefreshToken() { UserID = userId, Token = token });
            else
                user.Token = token;
            await context.SaveChangesAsync();
            return new GeneralResponse(true, null!);
        }
        catch (Exception e)
        {
            return new GeneralResponse(false, e.Message);
        }
    }
}
