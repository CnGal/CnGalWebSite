using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace CnGalWebSite.SDK.MainSite.AspNet.Auth;

public sealed class CookieOidcRefresher(
    IOptionsMonitor<OpenIdConnectOptions> oidcOptionsMonitor,
    IOptions<MainSiteOidcOptions> mainSiteOidcOptions)
{
    private readonly OpenIdConnectProtocolValidator _oidcTokenValidator = new()
    {
        RequireNonce = false,
    };

    public async Task ValidateOrRefreshCookieAsync(CookieValidatePrincipalContext validateContext, string oidcScheme)
    {
        var accessTokenExpirationText = validateContext.Properties.GetTokenValue("expires_at");
        if (!DateTimeOffset.TryParse(accessTokenExpirationText, out var accessTokenExpiration))
        {
            return;
        }

        var oidcOptions = oidcOptionsMonitor.Get(oidcScheme);
        var now = oidcOptions.TimeProvider!.GetUtcNow();
        var refreshLeadTime = TimeSpan.FromMinutes(Math.Max(1, mainSiteOidcOptions.Value.RefreshLeadTimeMinutes));
        if (now + refreshLeadTime < accessTokenExpiration)
        {
            return;
        }

        var refreshToken = validateContext.Properties.GetTokenValue("refresh_token");
        var oidcConfiguration = await oidcOptions.ConfigurationManager!.GetConfigurationAsync(validateContext.HttpContext.RequestAborted);
        var result = await TryRefreshCoreAsync(oidcOptions, oidcConfiguration, refreshToken, validateContext.HttpContext.RequestAborted);

        if (result is null)
        {
            validateContext.RejectPrincipal();
            return;
        }

        var expiresAt = now + TimeSpan.FromSeconds(result.Value.ExpiresIn);
        validateContext.ShouldRenew = true;
        validateContext.ReplacePrincipal(new ClaimsPrincipal(result.Value.ClaimsIdentity));
        validateContext.Properties.StoreTokens([
            new() { Name = "access_token", Value = result.Value.AccessToken },
            new() { Name = "id_token", Value = result.Value.IdToken },
            new() { Name = "refresh_token", Value = result.Value.RefreshToken },
            new() { Name = "token_type", Value = result.Value.TokenType },
            new() { Name = "expires_at", Value = expiresAt.ToString("o", CultureInfo.InvariantCulture) },
        ]);
    }

    public async Task<TokenRefreshResult?> TryRefreshAsync(HttpContext httpContext, string oidcScheme, bool forceRefresh = false)
    {
        var accessTokenExpirationText = await httpContext.GetTokenAsync("expires_at");
        if (!DateTimeOffset.TryParse(accessTokenExpirationText, out var accessTokenExpiration))
        {
            return null;
        }

        var oidcOptions = oidcOptionsMonitor.Get(oidcScheme);
        var now = oidcOptions.TimeProvider!.GetUtcNow();
        var refreshLeadTime = TimeSpan.FromMinutes(Math.Max(1, mainSiteOidcOptions.Value.RefreshLeadTimeMinutes));
        if (forceRefresh is false && now + refreshLeadTime < accessTokenExpiration)
        {
            return null;
        }

        var refreshToken = await httpContext.GetTokenAsync("refresh_token");
        var oidcConfiguration = await oidcOptions.ConfigurationManager!.GetConfigurationAsync(httpContext.RequestAborted);
        return await TryRefreshCoreAsync(oidcOptions, oidcConfiguration, refreshToken, httpContext.RequestAborted);
    }

    private async Task<TokenRefreshResult?> TryRefreshCoreAsync(
        OpenIdConnectOptions oidcOptions,
        OpenIdConnectConfiguration oidcConfiguration,
        string? refreshToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        var tokenEndpoint = oidcConfiguration.TokenEndpoint
            ?? throw new InvalidOperationException("Cannot refresh token. TokenEndpoint missing!");

        using var refreshResponse = await oidcOptions.Backchannel.PostAsync(tokenEndpoint,
            new FormUrlEncodedContent(new Dictionary<string, string?>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = oidcOptions.ClientId,
                ["client_secret"] = oidcOptions.ClientSecret,
                ["scope"] = string.Join(" ", oidcOptions.Scope),
                ["refresh_token"] = refreshToken,
            }), cancellationToken);

        if (!refreshResponse.IsSuccessStatusCode)
        {
            return null;
        }

        var refreshJson = await refreshResponse.Content.ReadAsStringAsync(cancellationToken);
        var message = new OpenIdConnectMessage(refreshJson);

        var validationParameters = oidcOptions.TokenValidationParameters.Clone();
        if (oidcOptions.ConfigurationManager is BaseConfigurationManager baseConfigurationManager)
        {
            validationParameters.ConfigurationManager = baseConfigurationManager;
        }
        else
        {
            validationParameters.ValidIssuer = oidcConfiguration.Issuer;
            validationParameters.IssuerSigningKeys = oidcConfiguration.SigningKeys;
        }

        var validationResult = await oidcOptions.TokenHandler.ValidateTokenAsync(message.IdToken, validationParameters);
        if (!validationResult.IsValid)
        {
            return null;
        }

        var validatedIdToken = JwtSecurityTokenConverter.Convert(validationResult.SecurityToken as JsonWebToken);
        validatedIdToken!.Payload["nonce"] = null;
        _oidcTokenValidator.ValidateTokenResponse(new()
        {
            ProtocolMessage = message,
            ClientId = oidcOptions.ClientId,
            ValidatedIdToken = validatedIdToken,
        });

        var expiresIn = int.Parse(message.ExpiresIn, NumberStyles.Integer, CultureInfo.InvariantCulture);

        return new TokenRefreshResult
        {
            AccessToken = message.AccessToken,
            IdToken = message.IdToken,
            RefreshToken = message.RefreshToken,
            TokenType = message.TokenType,
            ExpiresIn = expiresIn,
            ClaimsIdentity = validationResult.ClaimsIdentity,
        };
    }
}

public readonly record struct TokenRefreshResult
{
    public required string AccessToken { get; init; }
    public required string IdToken { get; init; }
    public required string RefreshToken { get; init; }
    public required string TokenType { get; init; }
    public required int ExpiresIn { get; init; }
    public required ClaimsIdentity ClaimsIdentity { get; init; }
}
