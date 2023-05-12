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
    readonly IGameDb _gameDb;
    readonly IAccountService _accountService;
    readonly ILogger<CheckAttendance> _logger;

    public CheckAttendance(ILogger<CheckAttendance> logger, IGameDb gameDb, IAccountService accountService)
    {
        _logger = logger;
        _gameDb = gameDb;
        _accountService = accountService;
    }

    /**
    * parameter: mailId
    * return: mail
    */
    [HttpPost]
    [Route("checkAttendance")]
    public async Task<CheckAttendanceRes> Post(CheckAttendanceReq request)
    {
        var response = new CheckAttendanceRes();

        // accountId를 가지고 출석 체크를 시도한다.
        var errorCode = await _gameDb.CheckAttendance();
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        _logger.ZLogInformationWithPayload(EventIdDic[EventType.CheckAttendance], $"CheckAttendance Success");
        return response;
    }
}