using VentasPOST.Abstractions.Client;
using VentasPOST.Domain.DTOs.Request.Account;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;
using System.Net;
using VentasPOST.Domain;

namespace VentasPOST.ServiceClient.SecurityHandlers;

public class CustomHttpHandler : DelegatingHandler
{
    private readonly LocalStorageService localStorageService;
    private readonly NavigationManager navigationManager;
    private readonly IAccountClientService accountService;

    public CustomHttpHandler(
        LocalStorageService localStorageService, 
        NavigationManager navigationManager,
        IAccountClientService accountService)
    {
        this.localStorageService = localStorageService;
        this.navigationManager = navigationManager;
        this.accountService = accountService;
    }

    protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            bool loginUrl = request.RequestUri!.AbsoluteUri.Contains(Constant.LoginRoute);
            bool registerUrl = request.RequestUri!.AbsoluteUri.Contains(Constant.RegisterRoute);
            bool refreshTokenUrl = request.RequestUri!.AbsoluteUri.Contains(Constant.RefreshTokenRoute);
            bool adminCreateUrl = request.RequestUri!.AbsoluteUri.Contains(Constant.CreateAdminRoute);
            if (loginUrl || registerUrl || refreshTokenUrl || adminCreateUrl)
                return await base.SendAsync(request, cancellationToken);

            var result = await base.SendAsync(request, cancellationToken);
            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                var tokenModel = await localStorageService.GetModelFromToken();
                if (tokenModel == null) return result;
                
                var newJwtToken = await GetReshToken(tokenModel.Refresh!);
                if (string.IsNullOrEmpty(newJwtToken)) return result;
                request.Headers.Authorization =
                    new AuthenticationHeaderValue(Constant.HttpClientHeaderScheme, newJwtToken);
                return await base.SendAsync(request, cancellationToken);
            }
            return result;
        }
        catch { return null!; }
    }

    private async Task<string> GetReshToken(string refreshToken)
    {
        try
        {
            var response = await accountService.RefreshTokenAsync(new RefreshTokenDTO() { Token = refreshToken });
            if (response == null || response.Token == null)
            {
                await ClearBrowserStorage();
                NavigateToLogin();
                return null!;
            }
            await localStorageService.RemoveTokenFromBrowserLocalStroage();
            await localStorageService.SetBrowserLocalStorage(
                new LocalStorageDTO() { Refresh = response!.RefreshToken, Token = response!.Token }
            );
            return response.Token;
        }
        catch { return null!; }
    }
    private void NavigateToLogin() 
        => navigationManager.NavigateTo(navigationManager.BaseUri, true, true);

    private async Task ClearBrowserStorage() 
        => await localStorageService.RemoveTokenFromBrowserLocalStroage();
}
