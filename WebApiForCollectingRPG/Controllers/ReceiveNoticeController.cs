using Microsoft.AspNetCore.Mvc;
using static LogManager;
using Microsoft.Extensions.Logging;
using ZLogger;
using WebApiForCollectingRPG.Services;
using WebApiForCollectingRPG.Dtos;
using System.Threading.Tasks;

namespace WebApiForCollectingRPG.Controllers;

[ApiController]
[Route("api")]
public class ReceiveNotice : ControllerBase
{
    readonly IMemoryDb _memoryDb;
    readonly ILogger<ReceiveNotice> _logger;

    public ReceiveNotice(ILogger<ReceiveNotice> logger, IMemoryDb memoryDb)
    {
        _logger = logger;
        _memoryDb = memoryDb;
    }

    /**
     * return: notice
     */
    [HttpPost]
    [Route("receiveNotice")]
    public async Task<ReceiveNoticeRes> Post(ReceiveNoticeReq request)
    {
        var response = new ReceiveNoticeRes();

        // Memory DB로부터 공지를 받아온다.
        (response.Result, response.Notice) = await _memoryDb.GetNoticeAsync();
        if (response.Result != ErrorCode.None)
        {
            return response;
        }
        _logger.ZLogInformationWithPayload(EventIdDic[EventType.ReceiveNotice], $"ReceiveNotice Success");

        return response;
    }
}