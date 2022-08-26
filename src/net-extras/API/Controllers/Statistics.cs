﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize(Policy = "RequireValidUser")]
[ApiController]
[Route("[controller]")]
public class Statistics : ControllerBase
{
    [HttpGet]
    [Route("")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<string>))]
    public ActionResult<List<string>> ListAvaliable()
    {
        return new List<string> { "RisksOverTime", "SecurityControls" };
    }
    
    [HttpGet]
    [Route("RisksOverTime")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<string>))]
    public ActionResult<Dictionary<string,double>> GetRisksOverTime()
    {
        return new Dictionary<string,double> { { "Jan", 1.0 }, { "Fev", 2.0 }, { "Mar", 8.0 } };
    }
    
}