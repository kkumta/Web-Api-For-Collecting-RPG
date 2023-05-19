using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using WebApiForCollectingRPG.Services;
using static LogManager;
using ZLogger;
using WebApiForCollectingRPG.DTO.Dungeon;

namespace WebApiForCollectingRPG.Controllers;

[ApiController]
[Route("api")]
public class CompleteStage : ControllerBase
{
    readonly IGameService _gameService;
    readonly IMemoryService _memoryService;
    readonly ILogger<CompleteStage> _logger;

    public CompleteStage(IGameService gameService, IMemoryService memoryService, ILogger<CompleteStage> logger)
    {
        _gameService = gameService;
        _memoryService = memoryService;
        _logger = logger;
    }

    [HttpPost]
    [Route("completeStage")]
    public async Task<CompleteStageRes> Post(CompleteStageReq request)
    {
        var response = new CompleteStageRes();

        var errorCode = await _memoryService.IsPlaying(request.Email);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        (errorCode, var items) = await _memoryService.CompleteStage(request.Email, request.StageId);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        await _gameService.SaveStageRewardToPlayer(request.StageId, items);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        _logger.ZLogInformationWithPayload(EventIdDic[EventType.CompleteStage], $"CompleteStage Success");

        return response;
    }
}
