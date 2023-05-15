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
    readonly IGameService _gameDb;
    readonly IAccountService _accountService;
    readonly ILogger<EnhanceItem> _logger;

    public EnhanceItem(ILogger<EnhanceItem> logger, IGameService gameDb, IAccountService accountService)
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
    [Route("enhanceItem")]
    public async Task<EnhanceItemRes> Post(EnhanceItemReq request)
    {
        var response = new EnhanceItemRes();

        // 해당하는 아이템을 강화한다.
        (var errorCode, response.IsSuccess) = await _gameDb.EnhanceItem(request.PlayerItemId);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }       

        _logger.ZLogInformationWithPayload(EventIdDic[EventType.EnhanceItem], $"EnhanceItem Success");
        return response;
    }
}