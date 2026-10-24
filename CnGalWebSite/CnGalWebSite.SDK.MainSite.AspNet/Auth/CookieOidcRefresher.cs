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
    SessionTokenStore sessions)
{
    private readonly OpenIdConnectProtocolValidator _oidcTokenValidator = new()
    {
        RequireNonce = false,
    };

    public async Task ValidateOrRefreshCookieAsync(CookieValidatePrincipalContext validateContext)
    {
        var tokens = validateContext.Principal is { } principal
            ? await GetSessionTokensAsync(principal, null, validateContext.HttpContext.RequestAborted)
            : null;
        if (tokens is null || tokens.Changed.IsCancellationRequested)
        {
            sessions.Remove(validateContext.Principal);
            validateContext.RejectPrincipal();
            return;
        }

        validateContext.ShouldRenew = validateContext.Properties.GetTokenValue("id_token") != tokens.IdToken;
        validateContext.ReplacePrincipal(new ClaimsPrincipal(tokens.Principal.Identities.Select(identity => identity.Clone())));
        validateContext.Properties.StoreTokens([
            new() { Name = "id_token", Value = tokens.IdToken },
        ]);
    }

    internal Task<SessionTokenStore.Tokens?> GetSessionTokensAsync(ClaimsPrincipal principal, string? rejectedAccessToken,
        CancellationToken cancellationToken) => sessions.GetAsync(principal, rejectedAccessToken,
            (token, cancellation) => TryRefreshAsync(token, MainSiteOidcOptions.OidcScheme, cancellation), cancellationToken);

    /// <summary>仅由会话存储协调刷新；不重新读取 Cookie 中的旧令牌。</summary>
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
