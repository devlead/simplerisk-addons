using System.Text;
using API.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Saml2.Authentication.Core.Configuration;
using Serilog;
using ServerServices;

namespace API;

public static class AuthenticationBootstrapper
{
    public static void RegisterAuthentication(IServiceCollection services, IConfiguration config)
    {
        var envService = new EnvironmentService();
        var key = Convert.FromBase64String(envService.ServerSecretToken);
        //var key = Encoding.ASCII.GetBytes(Settings.Secret);
        
        // Add Saml2.Authentication.Core
        services.Configure<Saml2Configuration>(config.GetSection("Saml2"));
        services.AddSaml();
        
        services.AddAuthentication(options =>
            {
                //options.DefaultScheme = "saml2";
                options.DefaultScheme = "headerSelector";
                options.DefaultChallengeScheme = "headerSelector";
            })
            .AddPolicyScheme("headerSelector", "this will select SAML or Basic Authentication", options =>
            {
                options.ForwardDefaultSelector = (context) =>
                {
                    if (context.Request.Headers.ContainsKey("Authorization"))
                    {
                        if(context.Request.Headers["Authorization"].ToString().StartsWith("Bearer "))
                        {
                            Log.Information("Authenticating using Jwt");
                            return "Bearer";
                        }
                        Log.Information("Authenticating using Basic");
                        return "BasicAuthentication";
                    }
                    else if(config["Saml2:Enabled"] == "True")
                    {
                        Log.Information("Authenticating using SAML");
                        return "saml2";
                    }
                    else
                    {
                        return "BasicAuthentication";
                    }
        
                };
                
            })
            .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null)
            .AddScheme<JwtBearerOptions, JwtAuthenticationHandler>("Bearer",
                x =>
                {
                    x.RequireHttpsMetadata = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        RequireExpirationTime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                })
            /*.AddJwtBearer("Bearer",x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    RequireExpirationTime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            })*/
            .AddCookie("saml2.cookies", options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            })
            .AddSaml("saml2", "saml2", options =>
            {
                options.DefaultRedirectUrl = "/Authentication/SAMLSingIn";
                options.SignInScheme = "saml2.cookies";
                options.IdentityProviderName = "stubidp.sustainsys";
            });
        
        
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireValidUser", policy =>
            {
                policy.RequireAuthenticatedUser()
                    .Requirements.Add(new ValidUserRequirement());
            });
            options.AddPolicy("RequireGovernanceAccess", policy =>
            {
                policy.RequireAuthenticatedUser()
                    .Requirements.Add(new ValidUserRequirement());
                policy.RequireClaim("Permission", new[] {"governance"});
            });
            options.AddPolicy("RequireAssessmentAccess", policy =>
            {
                policy.RequireAuthenticatedUser()
                    .Requirements.Add(new ValidUserRequirement());
                policy.Requirements.Add(new ClaimsAuthorizationRequirement("Permission", new []{"assessments"}));
            });
            options.AddPolicy("RequireAdminOnly", policy =>
            {
                policy.RequireAuthenticatedUser()
                    .Requirements.Add(new ValidUserRequirement());
                policy.Requirements.Add(new UserInRoleRequirement("Administrator"));
            });
        });
    }
}