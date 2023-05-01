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
    private readonly IGameDb _gameDb;
    private readonly ILogger<CreateAccount> _logger;

    public CreateAccount(ILogger<CreateAccount> logger, IAccountDb accountDb, IGameDb gameDb)
    {
        _logger = logger;
        _accountDb = accountDb;
        _gameDb = gameDb;
    }

    [HttpPost]
    [Route("account")]
    public async Task<CreateAccountRes> Post(CreateAccountReq request)
    {
        var response = new CreateAccountRes();

        var (errorCode, accountId) = await _accountDb.CreateAccountAsync(request.Email, request.Password);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        _logger.ZLogInformationWithPayload(EventIdDic[EventType.CreateAccount], new { Email = request.Email }, $"CreateAccount Success");

        // 계정이 정상적으로 생성된 경우, 계정 게임 데이터 생성
        errorCode = await _gameDb.CreateAccountGameDataAsync(accountId);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        _logger.ZLogInformationWithPayload(EventIdDic[EventType.CreateAccount], new { Email = request.Email }, $"CreateAccountGame Success");

        errorCode = await _gameDb.CreateAccountItemDataAsync(accountId);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        _logger.ZLogInformationWithPayload(EventIdDic[EventType.CreateAccount], new { Email = request.Email }, $"CreateAccountItem Success");

        return response;
    }
}