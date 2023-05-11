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
    readonly IAccountService _accountService;
    readonly ILogger<EnhanceItem> _logger;

    public EnhanceItem(ILogger<EnhanceItem> logger, IGameDb gameDb, IAccountService accountService)
    {
        _logger = logger;
        _gameDb = gameDb;
        _accountService = accountService;
    }

    /**
     * parameter: accountItemId
     * return: errorCode
     */
    [HttpPost]
    [Route("account-items/{playerItemId}/enhancements")]
    public async Task<EnhanceItemRes> Post(EnhanceItemReq request, Int64 playerItemId)
    {
        var response = new EnhanceItemRes();

        // 해당하는 아이템을 강화한다.
        (var errorCode, response.IsSuccess) = await _gameDb.EnhanceItem(playerItemId);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }       

        _logger.ZLogInformationWithPayload(EventIdDic[EventType.EnhanceItem], $"EnhanceItem Success");
        return response;
    }
}