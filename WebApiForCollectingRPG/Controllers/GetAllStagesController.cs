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
public class GetAllStages : ControllerBase
{
    readonly IGameService _gameService;
    readonly IMasterService _masterService;
    readonly ILogger<GetAllStages> _logger;

    public GetAllStages (IGameService gameService, IMasterService masterService, ILogger<GetAllStages> logger)
    {
        _gameService = gameService;
        _masterService = masterService;
        _logger = logger;
    }

    [HttpPost]
    [Route("getAllStages")]
    public async Task<GetAllStagesRes> Post(GetAllStagesReq request)
    {
        var response = new GetAllStagesRes();

        var (errorCode, stages) = await _gameService.GetAllStagesAsync();
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }
        foreach (var stageDetail in stages)
        {
            response.Stages.Add(stageDetail);
        }

        _logger.ZLogInformationWithPayload(EventIdDic[EventType.GetAllStages], $"GetAllStages Success");
        return response;
    }
}