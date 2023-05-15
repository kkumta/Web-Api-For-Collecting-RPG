using Microsoft.AspNetCore.Mvc;
using static LogManager;
using Microsoft.Extensions.Logging;
using ZLogger;
using System;
using System.Threading.Tasks;
using WebApiForCollectingRPG.DTO.Mail;
using WebApiForCollectingRPG.Services;

namespace WebApiForCollectingRPG.Controllers;

[ApiController]
[Route("api")]
public class GetMail : ControllerBase
{
    readonly IGameService _gameService;
    readonly ILogger<GetMail> _logger;

    public GetMail(ILogger<GetMail> logger, IGameService gameService)
    {
        _logger = logger;
        _gameService = gameService;
    }

    /**
     * parameter: mailId
     * return: mail
     */
    [HttpPost]
    [Route("getMail")]
    public async Task<GetMailRes> Post(GetMailReq request)
    {
        var response = new GetMailRes();

        (var errorCode, response.Mail, var items) = await _gameService.GetMailByMailId(request.MailId);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }
        foreach (var item in items)
        {
            var itemInfo = new ItemDTO
            {
                ItemId = item.ItemId,
                ItemCount = item.ItemCount,
            };

            response.Items.Add(itemInfo);
        }

        _logger.ZLogInformationWithPayload(EventIdDic[EventType.GetMail], $"GetMail Success");
        return response;
    }
}