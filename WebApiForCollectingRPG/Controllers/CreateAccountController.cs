using System;
using System.Threading.Tasks;
using WebApiForCollectingRPG.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ZLogger;
using static LogManager;
using WebApiForCollectingRPG.Dtos;

namespace WebApiForCollectingRPG.Controllers;

[ApiController]
[Route("api")]
public class CreateAccount : ControllerBase
{
    private readonly IAccountDb _accountDb;
    private readonly ILogger<CreateAccount> _logger;

    [HttpPost]
    [Route("account")]
    public async Task<PkCreateAccountRes> Post(PkCreateAccountReq request)
    {
        var response = new PkCreateAccountRes();

        var errorCode = await _accountDb.CreateAccountAsync(request.Email, request.Password);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        _logger.ZLogInformationWithPayload(EventIdDic[EventType.CreateAccount], new { Email = request.Email }, $"CreateAccount Success");
        return response;
    }
}
