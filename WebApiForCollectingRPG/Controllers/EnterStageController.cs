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
    readonly ILogger<EnterStage> _logger;

    public EnterStage (IGameService gameService, ILogger<EnterStage> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    [HttpPost]
    [Route("enterStage")]
    public async Task<EnterStageRes> Post(EnterStageReq request)
    {
        var response = new EnterStageRes();

        var (errorCode, items, attackNpcs) = await _gameService.EnterStageAsync(request.StageId);
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

        _logger.ZLogInformationWithPayload(EventIdDic[EventType.EnterStage], $"EnterStage Success");
        return response;
    }
}
