using Microsoft.AspNetCore.Mvc;
using static LogManager;
using Microsoft.Extensions.Logging;
using ZLogger;
using WebApiForCollectingRPG.Services;
using System.Threading.Tasks;
using WebApiForCollectingRPG.DTO;

namespace WebApiForCollectingRPG.Controllers;

[ApiController]
[Route("api")]
public class ReceiveNotice : ControllerBase
{
    readonly IMemoryService _memoryService;
    readonly ILogger<ReceiveNotice> _logger;

    public ReceiveNotice(ILogger<ReceiveNotice> logger, IMemoryService memoryService)
    {
        _logger = logger;
        _memoryService = memoryService;
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
        (response.Result, response.Notice) = await _memoryService.GetNoticeAsync();
        if (response.Result != ErrorCode.None)
        {
            return response;
        }
        _logger.ZLogInformationWithPayload(EventIdDic[EventType.ReceiveNotice], $"ReceiveNotice Success");

        return response;
    }
}