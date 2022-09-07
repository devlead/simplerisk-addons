﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Model.Authentication;
using Model.Exceptions;
using ServerServices;

namespace API.Controllers;

[Authorize(Policy = "RequireValidUser")]
[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly ILogger<AuthenticationController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IEnvironmentService _environmentService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserManagementService _userManagementService;
    private readonly IRoleManagementService _roleManagementService;
    public AuthenticationController(ILogger<AuthenticationController> logger, 
        IConfiguration configuration,
        IEnvironmentService environmentService,
        IHttpContextAccessor httpContextAccessor,
        IUserManagementService userManagementService,
        IRoleManagementService roleManagementService)
    {
        _logger = logger;
        _configuration = configuration;
        _environmentService = environmentService;
        _httpContextAccessor = httpContextAccessor;
        _userManagementService = userManagementService;
        _roleManagementService = roleManagementService;
    }

    [HttpGet]
    [Route("GetToken")]
    public ActionResult<string> GetToken()
    {
        var symmetricKey = Convert.FromBase64String(_environmentService.ServerSecretToken);
        var tokenHandler = new JwtSecurityTokenHandler();

        var now = DateTime.UtcNow;
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, _httpContextAccessor.HttpContext!.User!.Identity!.Name!)
            }),

            Expires = now.AddMinutes(Convert.ToInt32(_configuration["JWT:Timeout"])),
        
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(symmetricKey), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        var stoken = tokenHandler.CreateToken(tokenDescriptor);
        var token = tokenHandler.WriteToken(stoken);

        _logger.LogInformation("Authentication token created for user: {0} fromip: {1}", 
            _httpContextAccessor.HttpContext!.User!.Identity!.Name!,
            _httpContextAccessor.HttpContext!.Connection.RemoteIpAddress);
        
        return token;
    }

    /// <summary>
    /// The request to this endpoint will start the SAML authentication process.
    /// </summary>
    /// <param name="requestId">Unique id of this request</param>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    [Route("SAMLRequest")]
    public ActionResult SAMLRequest([FromQuery] string requestId)
    {
        Response.Cookies.Append("SAMLReqID", requestId, new CookieOptions
        {
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.None
        });
        
        return Redirect("/Authentication/SAMLSingIn");
    }

    [HttpGet]
    [Route("SAMLSingIn")]
    public ActionResult SAMLSingIn()
    {
        string requestId = Request.Cookies["SAMLReqID"];  
        return Ok("SAML Assertion for request: " + requestId);
    }
    
    [HttpPost]
    [Route("SAMLAssertion")]
    public ActionResult SAMLAssertion([FromBody] string data)
    {
        string requestId = Request.Cookies["SAMLReqID"];  
        return Ok("SAML Assertion for request: " + requestId);
    }
    
    
    [HttpGet]
    [Route("SAMLLogout")]
    public ActionResult SAMLLogout()
    {
        return Ok("Teste");
    }
    
    [HttpGet]
    [Route("AuthenticatedUserInfo")]
    public AuthenticatedUserInfo GetAuthenticatedUserInfo()
    {
        var userAccount = _httpContextAccessor.HttpContext!.User!.Identity!.Name!;
        
        if (userAccount == null)
        {
            _logger.LogError("Authenticated userAccount not found");
            throw new UserNotFoundException();
        }

        var user = _userManagementService.GetUser(userAccount);
        if (user == null )
        {
            _logger.LogError("Authenticated user not found");
            throw new UserNotFoundException();
        }

        string? userRole = null;
        if (user.RoleId > 0)
        {

            var role = _roleManagementService.GetRole(user.RoleId);
            if (role == null)
            {
                _logger.LogError("Invalid role reference");
                throw new InvalidReferenceException($"Invalid role reference for id: {user.RoleId}");
            }
            userRole = role!.Name;
        }
        
        var permissions = _userManagementService.GetUserPermissions(user.Value);
        
        var info = new AuthenticatedUserInfo
        {  
            UserAccount = userAccount,
            UserName = user.Name,
            UserEmail = Encoding.UTF8.GetString(user.Email),
            UserRole = userRole,
            UserPermissions = permissions
        };
        
        _logger.LogDebug("User info requested for user: {0} fromip: {1}", 
            _httpContextAccessor.HttpContext!.User!.Identity!.Name!,
            _httpContextAccessor.HttpContext!.Connection.RemoteIpAddress);

        
        return info;
    }

    [AllowAnonymous]
    [HttpGet]
    [Route("AuthenticationMethods")]
    public IEnumerable<AuthenticationMethod> GetAllAuthenticationMethods()
    {
        var result = new List<AuthenticationMethod>();

        var basic = new AuthenticationMethod
        {
            Name = "Simplerisk",
            Description = "Simplerisk Internal DB Authentication",
            Type = "Basic"
            
        };
        if (_configuration["Saml2:Enabled"] == "True")
        {
            var saml = new AuthenticationMethod
            {
                Name = "SAML",
                Description = "SAML Authentication",
                Type = "SAML"
            };
            result.Add(saml);

        }
        
        result.Add(basic);
        
        _logger.LogDebug("User methods requested fromip: {0}",
            _httpContextAccessor.HttpContext!.Connection.RemoteIpAddress);

        
        return result;
    }
    
    
}