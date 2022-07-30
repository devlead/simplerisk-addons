﻿using System.Text;
using DAL;
using DAL.Context;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;

namespace API.Security;

public class ValidSamlUserRequirementHandler: AuthorizationHandler<ValidSamlUserRequirement>
{
    
    private SRDbContext _dbContext = null;

    public ValidSamlUserRequirementHandler(DALManager dalManager)
    {
        _dbContext = dalManager.GetContext();
    }
    
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ValidSamlUserRequirement requirement)
    {
        var userClaimPrincipal = context.User;

        var userName = userClaimPrincipal.Identities.FirstOrDefault().Name;
        if (userName == null)
        {
            context.Fail(new AuthorizationFailureReason(this, "User do not exists"));
            return Task.CompletedTask;
        }
            
        var user = _dbContext.Users.Where(u => u.Type == "saml")
            .FirstOrDefault<User>(u => u.Username ==  Encoding.UTF8.GetBytes(userName));

        if (user != null)
        {
            context.Succeed(requirement);
            
        }
        else
        {
            context.Fail(new AuthorizationFailureReason(this, "User do not exists"));
        }
        
        return Task.CompletedTask;
        
        //throw new NotImplementedException();
    }
}