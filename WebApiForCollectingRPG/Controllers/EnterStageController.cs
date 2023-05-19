using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using WebApiForCollectingRPG.Services;
using static LogManager;
using ZLogger;
using WebApiForCollectingRPG.DTO.Dungeon;
using System;

namespace WebApiForCollectingRPG.Controllers;

[ApiController]
[Route("api")]
public class EnterStage : ControllerBase
{
    readonly IGameService _gameService;
    readonly IMemoryService _redisService;
    readonly IMemoryCacheService _memoryCacheService;
    readonly ILogger<EnterStage> _logger;

    public EnterStage (IGameService gameService, IMemoryService redisService, IMemoryCacheService memoryCacheService, ILogger<EnterStage> logger)
    {
        _gameService = gameService;
        _redisService = redisService;
        _memoryCacheService = memoryCacheService;
        _logger = logger;
    }

    [HttpPost]
    [Route("enterStage")]
    public async Task<EnterStageRes> Post(EnterStageReq request)
    {
        var response = new EnterStageRes();

        var errorCode = _memoryCacheService.IsValidStageId(request.StageId);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        (errorCode, var items, var attackNpcs) = await _gameService.EnterStageAsync(request.StageId);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }
        foreach (var item in items)
        {
            response.Items.Add(item);
        }
        foreach (var npc in attackNpcs)
        {
            response.AttackNpcs.Add(npc);
        }

        errorCode = await _redisService.EnterStageAsync(request.Email, request.StageId);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        _logger.ZLogInformationWithPayload(EventIdDic[EventType.EnterStage], $"EnterStage Success");
        return response;
    }
}
