using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text.Json;
using System.Security.Claims;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
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
        TokenRefreshResult? result;
        try { result = await TryRefreshAsync(refreshToken ?? string.Empty, oidcScheme, validateContext.HttpContext.RequestAborted); }
        catch (Exception ex) when (IsTransient(ex) && !validateContext.HttpContext.RequestAborted.IsCancellationRequested)
        {
            if (accessTokenExpiration <= oidcOptions.TimeProvider.GetUtcNow()) validateContext.RejectPrincipal();
            return;
        }

        var subject = validateContext.Principal?.FindFirst("sub");
        var refreshedSubject = result?.ClaimsIdentity.FindFirst("sub");
        if (result is null || subject is null || subject.Value != refreshedSubject?.Value || subject.Issuer != refreshedSubject.Issuer)
        {
            validateContext.RejectPrincipal();
            return;
        }

        var expiresAt = oidcOptions.TimeProvider.GetUtcNow() + TimeSpan.FromSeconds(result.Value.ExpiresIn);
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

    /// <summary>使用当前作用域内最新的刷新令牌，不重新读取旧 Cookie。</summary>
    public async Task<TokenRefreshResult?> TryRefreshAsync(string refreshToken, string oidcScheme, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return null;
        var oidcOptions = oidcOptionsMonitor.Get(oidcScheme);
        try
        {
            var configuration = await oidcOptions.ConfigurationManager!.GetConfigurationAsync(cancellationToken).WaitAsync(cancellationToken);
            return await TryRefreshCoreAsync(oidcOptions, configuration, refreshToken, cancellationToken);
        }
        catch (Exception ex) when (ex is JsonException or SecurityTokenException or OpenIdConnectProtocolException or ArgumentException)
        {
            return null;
        }
    }

    internal static bool IsTransient(Exception exception) => exception switch
    {
        HttpRequestException request => request.StatusCode is null or HttpStatusCode.TooManyRequests or >= HttpStatusCode.InternalServerError,
        OperationCanceledException => true, // 调用入口须排除调用方、身份及作用域取消。
        IOException io when io.Data[HttpDocumentRetriever.StatusCode] is HttpStatusCode status
            => status is HttpStatusCode.TooManyRequests or >= HttpStatusCode.InternalServerError,
        IOException or InvalidOperationException when exception.InnerException is { } inner => IsTransient(inner),
        _ => false
    };

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
            if (refreshResponse.StatusCode is HttpStatusCode.TooManyRequests or >= HttpStatusCode.InternalServerError)
                throw new HttpRequestException("Token endpoint temporarily unavailable", null, refreshResponse.StatusCode);
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
        if (validatedIdToken is null) return null;
        validatedIdToken.Payload["nonce"] = null;
        _oidcTokenValidator.ValidateTokenResponse(new()
        {
            ProtocolMessage = message,
            ClientId = oidcOptions.ClientId,
            ValidatedIdToken = validatedIdToken,
        });

        if (string.IsNullOrWhiteSpace(message.AccessToken)
            || !int.TryParse(message.ExpiresIn, NumberStyles.Integer, CultureInfo.InvariantCulture, out var expiresIn)
            || expiresIn <= 0) return null;

        return new TokenRefreshResult
        {
            AccessToken = message.AccessToken,
            IdToken = message.IdToken,
            RefreshToken = string.IsNullOrWhiteSpace(message.RefreshToken) ? refreshToken : message.RefreshToken,
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
