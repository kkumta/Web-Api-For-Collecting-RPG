using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using WebApiForCollectingRPG.DTO.Mail;
using WebApiForCollectingRPG.Services;
using static LogManager;
using ZLogger;

namespace WebApiForCollectingRPG.Controllers;

[ApiController]
[Route("api")]
public class ReceiveMailItems : ControllerBase
{
    readonly IGameService _gameDb;
    readonly IAccountService _accountService;
    readonly ILogger<ReceiveMailItems> _logger;

    public ReceiveMailItems(ILogger<ReceiveMailItems> logger, IGameService gameDb, IAccountService accountService)
    {
        _logger = logger;
        _gameDb = gameDb;
        _accountService = accountService;
    }

    /**
     * parameter: mailId
     */
    [HttpPost]
    [Route("receiveMailItems")]
    public async Task<ReceiveMailItemsRes> Post(ReceiveMailItemsReq request)
    {
        var response = new ReceiveMailItemsRes();

        // 해당되는 아이템들을 수령한다. 
        var errorCode = await _gameDb.ReceiveMailItems(request.MailId);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        _logger.ZLogInformationWithPayload(EventIdDic[EventType.ReceiveMailItems], $"ReceiveMailItems Success");
        return response;
    }
}