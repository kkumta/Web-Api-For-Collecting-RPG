using Microsoft.AspNetCore.Mvc;
using static LogManager;
using Microsoft.Extensions.Logging;
using ZLogger;
using System;
using System.Threading.Tasks;
using WebApiForCollectingRPG.Services;
using WebApiForCollectingRPG.DTO.Enhance;

namespace WebApiForCollectingRPG.Controllers;

[Route("api")]
[ApiController]
public class EnhanceItem : ControllerBase
{
    readonly IGameDb _gameDb;
    readonly IAccountDb _accountDb;
    readonly ILogger<EnhanceItem> _logger;

    public EnhanceItem(ILogger<EnhanceItem> logger, IGameDb gameDb, IAccountDb accountDb)
    {
        _logger = logger;
        _gameDb = gameDb;
        _accountDb = accountDb;
    }

    /**
     * parameter: accountItemId
     * return: errorCode
     */
    [HttpPost]
    [Route("account-items/{accountItemId}/enhancements")]
    public async Task<EnhanceItemRes> Post(EnhanceItemReq request, Int64 accountItemId)
    {
        var response = new EnhanceItemRes();

        // request의 Email을 가지고 해당하는 AccountId를 찾는다.
        var (errorCode, accountId) = await _accountDb.FindAccountIdByEmail(request.Email);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        // 해당하는 아이템을 강화한다.
        (errorCode, response.IsSuccess) = await _gameDb.EnhanceItem(accountId, accountItemId);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }       

        _logger.ZLogInformationWithPayload(EventIdDic[EventType.EnhanceItem], $"EnhanceItem Success");
        return response;
    }
}