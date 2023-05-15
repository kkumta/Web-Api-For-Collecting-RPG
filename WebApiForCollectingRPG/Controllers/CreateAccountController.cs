using System;
using System.Threading.Tasks;
using WebApiForCollectingRPG.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ZLogger;
using static LogManager;
using WebApiForCollectingRPG.DTO.Account;

namespace WebApiForCollectingRPG.Controllers;

[ApiController]
[Route("api")]
public class CreateAccount : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IGameService _gameService;
    private readonly ILogger<CreateAccount> _logger;

    public CreateAccount(ILogger<CreateAccount> logger, IAccountService accountService, IGameService gameService)
    {
        _logger = logger;
        _accountService = accountService;
        _gameService = gameService;
    }

    [HttpPost]
    [Route("createAccount")]
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

        (errorCode, var playerId) = await _gameService.CreatePlayerAsync(accountId);
        if (errorCode != ErrorCode.None)
        {
            _accountService.DeleteAccountAsync(accountId);
            response.Result = errorCode;
            return response;
        }
        _logger.ZLogInformationWithPayload(EventIdDic[EventType.CreateAccount], $"CreatePlayer Success");

        errorCode = await _gameService.CreatePlayerGameDataAsync(playerId);
        if (errorCode != ErrorCode.None)
        {
            _accountService.DeleteAccountAsync(accountId);
            await _gameService.DeletePlayerAsync(playerId);
            response.Result = errorCode;
            return response;
        }
        _logger.ZLogInformationWithPayload(EventIdDic[EventType.CreateAccount], $"CreatePlayerGameData Success");

        return response;
    }
}