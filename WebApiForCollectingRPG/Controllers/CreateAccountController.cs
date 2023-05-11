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
    private readonly IAccountService _accountService;
    private readonly IGameDb _gameDb;
    private readonly ILogger<CreateAccount> _logger;

    public CreateAccount(ILogger<CreateAccount> logger, IAccountService accountService, IGameDb gameDb)
    {
        _logger = logger;
        _accountService = accountService;
        _gameDb = gameDb;
    }

    [HttpPost]
    [Route("account")]
    public async Task<CreateAccountRes> Post(CreateAccountReq request)
    {
        var response = new CreateAccountRes();

        var (errorCode, accountId) = await _accountService.CreateAccountAsync(request.Email, request.Password);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }
        _logger.ZLogInformationWithPayload(EventIdDic[EventType.CreateAccount], new { Email = request.Email }, $"CreateAccount Success");

        (errorCode, var playerId) = await _gameDb.CreatePlayerAsync(accountId);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }
        _logger.ZLogInformationWithPayload(EventIdDic[EventType.CreateAccount], $"CreatePlayer Success");

        errorCode = await _gameDb.CreatePlayerGameDataAsync(playerId);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }
        _logger.ZLogInformationWithPayload(EventIdDic[EventType.CreateAccount], $"CreatePlayerGameData Success");

        return response;
    }
}