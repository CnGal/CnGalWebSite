using CnGalWebSite.BlazorWebAuto.Components;
using CnGalWebSite.BlazorWebAuto.Services;
using CnGalWebSite.Core.Services;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.Shared.Extentions;
using CnGalWebSite.Shared.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;


const string CNGAL_OIDC_SCHEME = "cngal";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(CNGAL_OIDC_SCHEME)
    .AddOpenIdConnect(CNGAL_OIDC_SCHEME, oidcOptions =>
    {
        // For the following OIDC settings, any line that's commented out
        // represents a DEFAULT setting. If you adopt the default, you can
        // remove the line if you wish.

        // ........................................................................
        // Pushed Authorization Requests (PAR) support. By default, the setting is
        // to use PAR if the identity provider's discovery document (usually found 
        // at '.well-known/openid-configuration') advertises support for PAR. If 
        // you wish to require PAR support for the app, you can assign 
        // 'PushedAuthorizationBehavior.Require' to 'PushedAuthorizationBehavior'.
        //
        // Note that PAR isn't supported by Microsoft Entra, and there are no plans
        // for Entra to ever support it in the future.

        //oidcOptions.PushedAuthorizationBehavior = PushedAuthorizationBehavior.UseIfAvailable;
        // ........................................................................

        // ........................................................................
        // The OIDC handler must use a sign-in scheme capable of persisting 
        // user credentials across requests.

        oidcOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        // ........................................................................

        // ........................................................................
        // The "openid" and "profile" scopes are required for the OIDC handler 
        // and included by default. You should enable these scopes here if scopes 
        // are provided by "Authentication:Schemes:MicrosoftOidc:Scope" 
        // configuration because configuration may overwrite the scopes collection.

        //oidcOptions.Scope.Add(OpenIdConnectScope.OpenIdProfile);
        // ........................................................................

        // ........................................................................
        // The "Weather.Get" scope for accessing the external web API for weather
        // data. The following example is based on using Microsoft Entra ID in 
        // an ME-ID tenant domain (the {APP ID URI} placeholder is found in
        // the Entra or Azure portal where the web API is exposed). For any other
        // identity provider, use the appropriate scope.

        oidcOptions.Scope.Add("role");
        oidcOptions.Scope.Add("offline_access");
        oidcOptions.Scope.Add("TaskAPI");
        oidcOptions.Scope.Add("FileAPI");
        oidcOptions.Scope.Add("CnGalAPI");
        oidcOptions.Scope.Add("openid");
        // ........................................................................

        // ........................................................................
        // The following paths must match the redirect and post logout redirect 
        // paths configured when registering the application with the OIDC provider. 
        // The default values are "/signin-oidc" and "/signout-callback-oidc".

        //oidcOptions.CallbackPath = new PathString("/signin-oidc");
        //oidcOptions.SignedOutCallbackPath = new PathString("/signout-callback-oidc");
        // ........................................................................

        // ........................................................................
        // The RemoteSignOutPath is the "Front-channel logout URL" for remote single 
        // sign-out. The default value is "/signout-oidc".

        //oidcOptions.RemoteSignOutPath = new PathString("/signout-oidc");
        // ........................................................................

        // ........................................................................
        // The following example Authority is configured for Microsoft Entra ID
        // and a single-tenant application registration. Set the {TENANT ID} 
        // placeholder to the Tenant ID. The "common" Authority 
        // https://login.microsoftonline.com/common/v2.0/ should be used 
        // for multi-tenant apps. You can also use the "common" Authority for 
        // single-tenant apps, but it requires a custom IssuerValidator as shown 
        // in the comments below. 

        oidcOptions.Authority = builder.Configuration["Authority"];
        // ........................................................................

        // ........................................................................
        // Set the Client ID for the app. Set the {CLIENT ID} placeholder to
        // the Client ID.

        oidcOptions.ClientId = builder.Configuration["ClientId"];
        oidcOptions.ClientSecret = builder.Configuration["ClientSecret"];
        // ........................................................................

        // ........................................................................
        // Setting ResponseType to "code" configures the OIDC handler to use 
        // authorization code flow. Implicit grants and hybrid flows are unnecessary
        // in this mode. In a Microsoft Entra ID app registration, you don't need to 
        // select either box for the authorization endpoint to return access tokens 
        // or ID tokens. The OIDC handler automatically requests the appropriate 
        // tokens using the code returned from the authorization endpoint.

        oidcOptions.ResponseType = OpenIdConnectResponseType.Code;
        // ........................................................................

        // ........................................................................
        // Set MapInboundClaims to "false" to obtain the original claim types from 
        // the token. Many OIDC servers use "name" and "role"/"roles" rather than 
        // the SOAP/WS-Fed defaults in ClaimTypes. Adjust these values if your 
        // identity provider uses different claim types.

        oidcOptions.MapInboundClaims = false;
        oidcOptions.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
        oidcOptions.TokenValidationParameters.RoleClaimType = "role";
        // ........................................................................

        // ........................................................................
        // Many OIDC providers work with the default issuer validator, but the
        // configuration must account for the issuer parameterized with "{TENANT ID}" 
        // returned by the "common" endpoint's /.well-known/openid-configuration
        // For more information, see
        // https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/issues/1731

        //var microsoftIssuerValidator = AadIssuerValidator.GetAadIssuerValidator(oidcOptions.Authority);
        //oidcOptions.TokenValidationParameters.IssuerValidator = microsoftIssuerValidator.Validate;
        // ........................................................................

        // ........................................................................
        // OIDC connect options set later via ConfigureCookieOidcRefresh
        //
        // (1) The "offline_access" scope is required for the refresh token.
        //
        // (2) SaveTokens is set to true, which saves the access and refresh tokens
        // in the cookie, so the app can authenticate requests for weather data and
        // use the refresh token to obtain a new access token on access token
        // expiration.
        // ........................................................................
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);


// ConfigureCookieOidc attaches a cookie OnValidatePrincipal callback to get
// a new access token when the current one expires, and reissue a cookie with the
// new access token saved inside. If the refresh fails, the user will be signed
// out. OIDC connect options are set for saving tokens and the offline access
// scope.
builder.Services.ConfigureCookieOidc(CookieAuthenticationDefaults.AuthenticationScheme, CNGAL_OIDC_SCHEME);

builder.Services.AddAuthorization();

builder.Services.AddCascadingAuthenticationState();

// Remove or set 'SerializeAllClaims' to 'false' if you only want to 
// serialize name and role claims for CSR.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization(options => options.SerializeAllClaims = true);


//判断是否 SSR
ToolHelper.IsSSR = ToolHelper.PreSetIsSSR == null ? true : ToolHelper.PreSetIsSSR.Value;
//覆盖默认api地址
if (string.IsNullOrWhiteSpace(builder.Configuration["WebApiPath"]) == false)
{
    ToolHelper.WebApiPath = builder.Configuration["WebApiPath"];
}
if (string.IsNullOrWhiteSpace(builder.Configuration["ImageApiPath"]) == false)
{
    ToolHelper.ImageApiPath = builder.Configuration["ImageApiPath"];
}
if (string.IsNullOrWhiteSpace(builder.Configuration["TaskApiPath"]) == false)
{
    ToolHelper.TaskApiPath = builder.Configuration["TaskApiPath"];
}

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<IHttpService, HttpService>(client =>
{
    client.BaseAddress = new Uri(ToolHelper.WebApiPath);
});


//设置Json格式化配置
ToolHelper.options.Converters.Add(new DateTimeConverterUsingDateTimeParse());
ToolHelper.options.Converters.Add(new DateTimeConverterUsingDateTimeNullableParse());
ToolHelper.options.Converters.Add(new JsonStringEnumConverter());

//主站
builder.Services.AddMainSite();

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.MapStaticAssets();
app.UseAntiforgery();


app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(CnGalWebSite.Shared.App).Assembly);

app.MapGroup("/authentication").MapLoginAndLogout();

app.Run();
