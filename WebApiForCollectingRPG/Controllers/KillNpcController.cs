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
public class KillNpc : ControllerBase
{
    readonly IGameService _gameService;
    readonly IMemoryService _memoryService;
    readonly ILogger<KillNpc> _logger;

    public KillNpc(IGameService gameService, IMemoryService memoryService, ILogger<KillNpc> logger)
    {
        _gameService = gameService;
        _memoryService = memoryService;
        _logger = logger;
    }

    [HttpPost]
    [Route("killNpc")]
    public async Task<KillNpcRes> Post(KillNpcReq request)
    {
        var response = new KillNpcRes();

        var (errorCode, allNpcsKilled, items) = await _memoryService.KillNpcAsync(request.StageId, request.NpcId);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        if (!allNpcsKilled)
        {
            _logger.ZLogInformationWithPayload(EventIdDic[EventType.ItemFarming], $"killNpc Success");
            return response;
        }
        await _gameService.SaveStageRewardToPlayer(request.StageId, items);
        response.AllNpcsKilled = true;

        return response;
    }
}