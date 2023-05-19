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
public class StopStage : ControllerBase
{
    readonly IMemoryService _memoryService;
    readonly ILogger<CompleteStage> _logger;

    public StopStage(IMemoryService memoryService, ILogger<CompleteStage> logger)
    {
        _memoryService = memoryService;
        _logger = logger;
    }

    [HttpPost]
    [Route("StopStage")]
    public async Task<StopStageRes> Post(StopStageReq request)
    {
        var response = new StopStageRes();

        var errorCode = await _memoryService.IsPlaying(request.Email);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        errorCode = await _memoryService.StopStage(request.Email, request.StageId);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        _logger.ZLogInformationWithPayload(EventIdDic[EventType.StopStage], $"StopStage Success");

        return response;
    }
}
