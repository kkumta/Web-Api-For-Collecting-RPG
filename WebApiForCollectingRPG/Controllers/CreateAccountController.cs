using System;
using System.Threading.Tasks;
using WebApiForCollectingRPG.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ZLogger;
using static LogManager;
using WebApiForCollectingRPG.Dtos;
using WebApiForCollectingRPG.ModelDB;

namespace WebApiForCollectingRPG.Controllers;

[ApiController]
[Route("api")]
public class CreateAccount : ControllerBase
{
    private readonly IAccountDb _accountDb;
    private readonly ILogger<Account> _logger;

    public CreateAccount(ILogger<Account> logger, IAccountDb accountDb)
    {
        _logger = logger;
        _accountDb = accountDb;
    }

    [HttpPost]
    [Route("account")]
    public async Task<CreateAccountRes> Post(CreateAccountReq request)
    {
        var response = new CreateAccountRes();

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
