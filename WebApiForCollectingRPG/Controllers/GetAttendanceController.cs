using Microsoft.AspNetCore.Mvc;
using static LogManager;
using Microsoft.Extensions.Logging;
using ZLogger;
using System.Threading.Tasks;
using WebApiForCollectingRPG.Services;
using WebApiForCollectingRPG.DTO.Attendance;
using System;

namespace WebApiForCollectingRPG.Controllers;

[ApiController]
[Route("api")]
public class GetAttendance : ControllerBase
{
    readonly IGameService _gameService;
    readonly ILogger<GetAttendance> _logger;

    public GetAttendance(ILogger<GetAttendance> logger, IGameService gameService)
    {
        _logger = logger;
        _gameService = gameService;
    }

    [HttpPost]
    [Route("getAttendance")]
    public async Task<GetAttendanceRes> Post(GetAttendanceReq request)
    {
        var response = new GetAttendanceRes();

        var (errorCode, attendance) = await _gameService.GetAttendanceAsync();
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        foreach (var attendanceDetail in attendance)
        {
            response.Attendance.Add(attendanceDetail);
        }

        _logger.ZLogInformationWithPayload(EventIdDic[EventType.GetAttendance], $"GetAttendance Success");
        return response;
    }
}