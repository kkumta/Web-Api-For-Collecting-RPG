using Microsoft.AspNetCore.Mvc;
using static LogManager;
using Microsoft.Extensions.Logging;
using ZLogger;
using System.Threading.Tasks;
using WebApiForCollectingRPG.Services;
using WebApiForCollectingRPG.DTO.Attendance;

namespace WebApiForCollectingRPG.Controllers;

[ApiController]
[Route("api")]
public class CheckAttendance : ControllerBase
{
    readonly IGameService _gameService;
    readonly ILogger<CheckAttendance> _logger;

    public CheckAttendance(ILogger<CheckAttendance> logger, IGameService gameService)
    {
        _logger = logger;
        _gameService = gameService;
    }

    [HttpPost]
    [Route("checkAttendance")]
    public async Task<CheckAttendanceRes> Post(CheckAttendanceReq request)
    {
        var response = new CheckAttendanceRes();

        var errorCode = await _gameService.CheckAttendance();
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        _logger.ZLogInformationWithPayload(EventIdDic[EventType.CheckAttendance], $"CheckAttendance Success");
        return response;
    }
}